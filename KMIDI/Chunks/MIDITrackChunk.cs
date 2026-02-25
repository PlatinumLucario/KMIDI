using Kermalis.EndianBinaryIO;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Kermalis.MIDI;

public sealed class MIDITrackChunk : MIDIChunk
{
	internal const string EXPECTED_NAME = "MTrk";

	private MIDIEvent? _first;
	private MIDIEvent? _last;

	/// <summary>
	/// The first event found in the track chunk
	/// </summary>
	public IMIDIEvent? First => _first;
	/// <summary>
	/// The last event found in the track chunk
	/// </summary>
	public IMIDIEvent? Last => _last;

	/// <summary>Includes the end of track event</summary>
	public int NumEvents { get; private set; }
	/// <summary>
	/// The total number of ticks in the track chunk
	/// </summary>
	public int NumTicks => Last is null ? 0 : Last.Ticks;

	/// <summary>
	/// If the track chunk has errors, this will be set to <b>true</b>
	/// </summary>
	public override bool HasErrors { get; internal set; }

	public MIDITrackChunk()
	{
		//
	}
	internal MIDITrackChunk(uint size, EndianBinaryReader r)
	{
		long endOffset = GetEndOffset(r, size);

		if (endOffset > r.Stream.Length)
		{
			// Critical error, since the size is beyond the scope of the MIDI file and can't go any further
			HasErrors = true;
			throw new InvalidDataException("The end offset for this track is invalid, it goes beyond the size of this MIDI file");
		}

		int ticks = 0;
		byte runningStatus = 0;
		bool foundEnd = false;
		bool sysexContinue = false;
		bool isInvalid = false;
		while (r.Stream.Position < endOffset)
		{
			if (foundEnd)
			{
				HasErrors = true;
				InvalidDataException ex = new($"Events found after the {nameof(MetaMessageType.EndOfTrack)} {nameof(MetaMessage)}");
				if (!MIDIFile.SkipErrors)
				{
					throw ex;
				}
				else
				{
					Debug.WriteLine(ex.Message);
					Debug.WriteLine("Proceeding to next event, despite error");
					isInvalid = true;
				}
			}

			ReadEvent(r, ref ticks, ref runningStatus, ref foundEnd, ref sysexContinue, ref isInvalid);
		}
		if (!foundEnd)
		{
			HasErrors = true;
			throw new InvalidDataException($"Could not find the {nameof(MetaMessageType.EndOfTrack)} {nameof(MetaMessage)}");
		}

		if (r.Stream.Position > endOffset)
		{
			HasErrors = true;
			throw new InvalidDataException("Expected to read a certain amount of events, but the data was read incorrectly...");
		}
	}
	private void ReadEvent(EndianBinaryReader r, ref int ticks, ref byte runningStatus, ref bool foundEnd, ref bool sysexContinue, ref bool isInvalid)
	{
		long startOffset = r.Stream.Position;

		ticks += Utils.ReadVariableLength(r);

		// Get command
		byte cmd = r.ReadByte();
		if (sysexContinue && cmd != 0xF7)
		{
			HasErrors = true;
			throw new InvalidDataException($"{nameof(SysExContinuationMessage)} was missing at 0x{r.Stream.Position - 1:X}");
		}
		if (cmd < 0x80)
		{
			cmd = runningStatus;
			r.Stream.Position--;
		}

		// Check which message it is
		if (cmd is >= 0x80 and <= 0xEF)
		{
			runningStatus = cmd;
			byte channel = (byte)(cmd & 0xF);
			switch (cmd & ~0xF)
			{
				case 0x80: InsertMessage(ticks, new NoteOffMessage(r, channel, isInvalid)); break;
				case 0x90: InsertMessage(ticks, new NoteOnMessage(r, channel, isInvalid)); break;
				case 0xA0: InsertMessage(ticks, new PolyphonicPressureMessage(r, channel, isInvalid)); break;
				case 0xB0: InsertMessage(ticks, new ControllerMessage(r, channel, isInvalid)); break;
				case 0xC0: InsertMessage(ticks, new ProgramChangeMessage(r, channel, isInvalid)); break;
				case 0xD0: InsertMessage(ticks, new ChannelPressureMessage(r, channel, isInvalid)); break;
				case 0xE0: InsertMessage(ticks, new PitchBendMessage(r, channel, isInvalid)); break;
			}
		}
		else if (cmd == 0xF0)
		{
			runningStatus = 0;
			var msg = new SysExMessage(r, isInvalid);
			if (!msg.IsComplete)
			{
				sysexContinue = true;
			}
		}
		else if (cmd == 0xF7)
		{
			runningStatus = 0;
			if (sysexContinue)
			{
				var msg = new SysExContinuationMessage(r, isInvalid);
				if (msg.IsFinished)
				{
					sysexContinue = false;
				}
			}
			else
			{
				InsertMessage(ticks, new EscapeMessage(r, isInvalid));
			}
		}
		else if (cmd == 0xFF)
		{
			var msg = new MetaMessage(r, isInvalid);
			if (msg.Type == MetaMessageType.EndOfTrack)
			{
				foundEnd = true;
			}
			InsertMessage(ticks, msg);
		}
		else
		{
			HasErrors = true;
			throw new InvalidDataException($"Unknown MIDI command found at 0x{startOffset:X} (0x{cmd:X})");
		}
	}

	/// <summary><inheritdoc cref="InsertMessage{T}(int, T)"/></summary>
	public IMIDIEvent InsertMessage(int ticks, MIDIMessage msg)
	{
		if (ticks < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(ticks), ticks, null);
		}

		// We always want to be able to cast to IMIDIEvent<T> where T is a non-abstract MIDIMessage
		Type tType = msg.GetType();
		Type eType = typeof(MIDIEvent<>).MakeGenericType(tType);
		object?[] constructorParams = new object?[] { ticks, Convert.ChangeType(msg, tType) };
		if (Activator.CreateInstance(eType, constructorParams) is not MIDIEvent e)
		{
			throw new Exception();
		}
		InsertMessage_Private(ticks, e);
		return e;
	}
	/// <summary>If there are other events at <paramref name="ticks"/>, <paramref name="msg"/> will be inserted after them.</summary>
	public IMIDIEvent<T> InsertMessage<T>(int ticks, T msg)
		where T : MIDIMessage
	{
		if (ticks < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(ticks), ticks, null);
		}

		var e = new MIDIEvent<T>(ticks, msg);
		InsertMessage_Private(ticks, e);
		return e;
	}
	private void InsertMessage_Private(int ticks, MIDIEvent e)
	{
		if (NumEvents == 0)
		{
			_first = e;
			_last = e;
		}
		else if (ticks < _first!.Ticks)
		{
			e.INext = _first;
			_first.IPrev = e;
			_first = e;
		}
		else if (ticks >= _last!.Ticks)
		{
			e.IPrev = _last;
			_last.INext = e;
			_last = e;
		}
		else // Somewhere between
		{
			MIDIEvent next = _first;

			while (next.Ticks <= ticks)
			{
				next = next.INext!;
			}

			MIDIEvent prev = next.IPrev!;

			e.INext = next;
			e.IPrev = prev;
			prev.INext = e;
			next.IPrev = e;
		}

		NumEvents++;
	}
	/// <summary>
	/// Removes a specified MIDI event
	/// </summary>
	/// <param name="ev">The MIDI event to remove</param>
	/// <returns><b>true</b> if the event was found and removed, otherwise <b>false</b></returns>
	public bool RemoveEvent(IMIDIEvent ev)
	{
		if (ev is not MIDIEvent e)
		{
			return false;
		}
		if (NumEvents == 0)
		{
			return false;
		}

		MIDIEvent first = _first!;
		MIDIEvent last = _last!;
		if (NumEvents == 1)
		{
			if (e == first && e == last)
			{
				_first = null;
				_last = null;
				NumEvents = 0;
				return true;
			}
			// If it wasn't the only event, then it's not in this track
			return false;
		}

		// Below here, we have at least 2 events
		if (e == first)
		{
			_first = e.INext!;
			_first.IPrev = null;
			NumEvents--;
			return true;
		}
		if (e == last)
		{
			_last = e.IPrev!;
			_last.INext = null;
			NumEvents--;
			return true;
		}

		// Either e is not in this track, or it's in the range (first, last)
		for (MIDIEvent i = first.INext!; i != last; i = i.INext!)
		{
			if (e == i)
			{
				MIDIEvent prev = e.IPrev!;
				MIDIEvent next = e.INext!;
				prev.INext = next;
				next.IPrev = prev;
				NumEvents--;
				return true;
			}
		}

		return false;
	}
	/// <summary>
	/// If an event is in an invalid location, such as after an EndOfTrack event, use this method to remove all invalid location events
	/// </summary>
	public void RemoveInvalidEvents()
	{
		for (var ev = First; ev != null; ev = ev.Next)
		{
			if (ev.Msg.IsInvalid)
			{
				RemoveEvent(ev);
			}
		}
	}

	/// <summary>
	/// Writes the track chunk to a EndianBinaryWriter stream
	/// </summary>
	/// <param name="w">The EndianBinaryWriter stream to use</param>
	/// <exception cref="InvalidDataException">If there's events found after EndOfTrack Meta Message type or if there's no EndOfTrack Meta Message in this track chunk</exception>
	public override void Write(EndianBinaryWriter w)
	{
		w.WriteChars_Count(EXPECTED_NAME, 4);

		long sizeOffset = w.Stream.Position;
		w.WriteUInt32(0); // We will update the size later

		byte runningStatus = 0;
		bool foundEnd = false;
		bool sysexContinue = false;
		for (IMIDIEvent? e = _first; e is not null; e = e.Next)
		{
			if (foundEnd)
			{
				throw new InvalidDataException($"Events found after the {nameof(MetaMessageType.EndOfTrack)} {nameof(MetaMessage)}");
			}

			WriteEvent(w, e, ref runningStatus, ref foundEnd, ref sysexContinue);
		}
		if (!foundEnd)
		{
			throw new InvalidDataException($"You must insert an {nameof(MetaMessageType.EndOfTrack)} {nameof(MetaMessage)}");
		}

		// Update size now
		long endOffset = w.Stream.Position;
		uint size = (uint)(endOffset - sizeOffset - 4);
		w.Stream.Position = sizeOffset;
		w.WriteUInt32(size);

		w.Stream.Position = endOffset; // Go back to the end
	}
	private static void WriteEvent(EndianBinaryWriter w, IMIDIEvent e, ref byte runningStatus, ref bool foundEnd, ref bool sysexContinue)
	{
		Utils.WriteVariableLength(w, e.DeltaTicks);

		MIDIMessage msg = e.Msg;
		byte cmd = msg.GetCMDByte();
		if (sysexContinue && cmd != 0xF7)
		{
			throw new InvalidDataException($"{nameof(SysExContinuationMessage)} was missing");
		}

		if (cmd is >= 0x80 and <= 0xEF)
		{
			if (runningStatus != cmd)
			{
				runningStatus = cmd;
				w.WriteByte(cmd);
			}
		}
		else if (cmd == 0xF0)
		{
			runningStatus = 0;
			var sysex = (SysExMessage)msg;
			if (!sysex.IsComplete)
			{
				sysexContinue = true;
			}
			w.WriteByte(0xF0);
		}
		else if (cmd == 0xF7)
		{
			runningStatus = 0;
			if (sysexContinue)
			{
				var sysex = (SysExContinuationMessage)msg;
				if (sysex.IsFinished)
				{
					sysexContinue = false;
				}
			}
			w.WriteByte(0xF0);
		}
		else if (cmd == 0xFF)
		{
			var meta = (MetaMessage)msg;
			if (meta.Type == MetaMessageType.EndOfTrack)
			{
				foundEnd = true;
			}
			w.WriteByte(0xFF);
		}
		else
		{
			throw new InvalidDataException($"Unknown MIDI command 0x{cmd:X}");
		}

		msg.Write(w);
	}

	/// <summary>
	/// Outputs the NumEvents, NumTicks and each event to the string
	/// </summary>
	/// <returns>A string containing the NumEvents, NumTicks and each event</returns>
	public override string ToString()
	{
		var str = new StringBuilder($"<{EXPECTED_NAME}>");
		str.AppendLine();

		str.AppendLine($"\t{nameof(NumEvents)}: {NumEvents}");
		str.AppendLine($"\t{nameof(NumTicks)}: {NumTicks}");

		for (IMIDIEvent? e = _first; e is not null; e = e.Next)
		{
			str.Append("\t\t");
			str.AppendLine(e.ToString());
		}

		return str.ToString();
	}
}
