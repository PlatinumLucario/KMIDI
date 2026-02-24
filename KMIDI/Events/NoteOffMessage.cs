using Kermalis.EndianBinaryIO;
using System;

namespace Kermalis.MIDI;

public sealed class NoteOffMessage : MIDIMessage, IMIDIChannelMessage
{
	internal override bool IsInvalid { get; set; }

	/// <summary>
	/// The MIDI Channel used
	/// </summary>
	public byte Channel { get; }

	/// <summary>
	/// The MIDI Note used
	/// </summary>
	public MIDINote Note { get; }
	/// <summary>
	/// The amount of Velocity used
	/// </summary>
	public byte Velocity { get; }

	internal NoteOffMessage(EndianBinaryReader r, byte channel, bool isInvalid)
	{
		IsInvalid = isInvalid;
		Channel = channel;

		Note = r.ReadEnum<MIDINote>();
		if (Note >= MIDINote.MAX)
		{
			Utils.ThrowInvalidMessageDataException(nameof(NoteOffMessage), nameof(Note), r.Stream.Position - 1, Note);
		}

		Velocity = r.ReadByte();
		if (Velocity > 127)
		{
			Utils.ThrowInvalidMessageDataException(nameof(NoteOffMessage), nameof(Velocity), r.Stream.Position - 1, Velocity);
		}
	}

	/// <summary>
	/// Creates a new Note Off Message
	/// </summary>
	/// <param name="channel">The MIDI Channel to use</param>
	/// <param name="note">The Note to use</param>
	/// <param name="velocity">The amount of Velocity to use</param>
	/// <exception cref="ArgumentOutOfRangeException">If the Note or Velocity is out of range</exception>
	public NoteOffMessage(byte channel, MIDINote note, byte velocity)
	{
		Utils.ValidateMIDIChannel(channel);
		if (note >= MIDINote.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(note), note, null);
		}
		if (velocity > 127)
		{
			throw new ArgumentOutOfRangeException(nameof(velocity), velocity, null);
		}

		Channel = channel;
		Note = note;
		Velocity = velocity;
	}

	internal override byte GetCMDByte()
	{
		return (byte)(0x80 + Channel);
	}

	internal override void Write(EndianBinaryWriter w)
	{
		w.WriteEnum(Note);
		w.WriteByte(Velocity);
	}

	/// <summary>
	/// Outputs a string with the details of the <see cref="NoteOffMessage"/>
	/// </summary>
	/// <returns>A string containing details of the <see cref="NoteOffMessage"/></returns>
	public override string ToString()
	{
		return $"{nameof(NoteOffMessage)} [{nameof(Channel)} {Channel}"
			+ $", {nameof(Note)}: {Note}"
			+ $", {nameof(Velocity)}: {Velocity}"
			+ ']';
	}
}
