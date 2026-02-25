using Kermalis.EndianBinaryIO;
using System;
using System.Diagnostics;
using System.IO;

namespace Kermalis.MIDI;

// Section 2.1
public sealed class MIDIHeaderChunk : MIDIChunk
{
	internal const string EXPECTED_NAME = "MThd";

	/// <summary>
	/// The type of format that the MIDI file uses, either Format0, Format1 or Format2
	/// </summary>
	public MIDIFormat Format { get; }
	/// <summary>
	/// The Number of Tracks in this MIDI file
	/// </summary>
	public ushort NumTracks { get; internal set; }
	/// <summary>
	/// The Time Division value of this MIDI file
	/// </summary>
	public TimeDivisionValue TimeDivision { get; }


	/// <summary>
	/// If the header chunk has errors, this will be set to <b>true</b>
	/// </summary>
	public override bool HasErrors { get; internal set; }

	internal MIDIHeaderChunk(MIDIFormat format, TimeDivisionValue timeDivision)
	{
		if (format > MIDIFormat.Format2)
		{
			throw new ArgumentOutOfRangeException(nameof(format), format, null);
		}
		if (!timeDivision.IsValid())
		{
			throw new ArgumentOutOfRangeException(nameof(timeDivision), timeDivision, null);
		}

		Format = format;
		TimeDivision = timeDivision;
	}
	internal MIDIHeaderChunk(uint size, EndianBinaryReader r)
	{
		if (size < 6)
		{
			// Critical error, can't proceed with reading the MIDI if the header size is too small
			throw new InvalidDataException($"Invalid MIDI header length ({size})");
		}

		long endOffset = GetEndOffset(r, size);

		Format = r.ReadEnum<MIDIFormat>();
		if (Format > MIDIFormat.Format2)
		{
			// Section 2.2 states that unknown formats should be supported
			Debug.WriteLine($"Unknown MIDI format ({Format}), so behavior is unknown");
		}

		NumTracks = r.ReadUInt16();
		if (NumTracks == 0)
		{
			HasErrors = true;
			InvalidDataException ex = new("MIDI has no tracks");
			if (!MIDIFile.SkipErrors)
			{
				throw ex;
			}
			else
			{
				Debug.WriteLine(ex.Message);
			}
		}
		if (Format == MIDIFormat.Format0 && NumTracks != 1)
		{
			HasErrors = true;
			InvalidDataException ex = new($"MIDI format 0 must have 1 track, but this MIDI has {NumTracks}");
			if (!MIDIFile.SkipErrors)
			{
				throw ex;
			}
			else
			{
				Debug.WriteLine(ex.Message);
			}
		}

		TimeDivision = new TimeDivisionValue(r.ReadUInt16());
		if (!TimeDivision.IsValid())
		{
			HasErrors = true;
			InvalidDataException ex = new($"Invalid MIDI time division ({TimeDivision})");
			if (!MIDIFile.SkipErrors)
			{
				throw ex;
			}
			else
			{
				Debug.WriteLine(ex.Message);
			}
		}

		if (size > 6)
		{
			// Section 2.2 states that the length should be honored
			Debug.WriteLine($"MIDI Header was longer than 6 bytes ({size}), so the extra data is being ignored");
			EatRemainingBytes(r, endOffset, EXPECTED_NAME, size);
		}
	}

	/// <summary>
	/// Writes the MIDI file data to memory
	/// </summary>
	/// <param name="w">The EndianBinaryWriter stream to use</param>
	/// <exception cref="InvalidDataException">If the MIDI Format is Format0 and doesn't have exactly 1 track</exception>
	public override void Write(EndianBinaryWriter w)
	{
		if (Format == MIDIFormat.Format0 && NumTracks != 1)
		{
			throw new InvalidDataException($"MIDI format 0 must have 1 track, but this MIDI has {NumTracks}");
		}

		w.WriteChars_Count(EXPECTED_NAME, 4);
		w.WriteUInt32(6);

		w.WriteEnum(Format);
		w.WriteUInt16(NumTracks);
		w.WriteUInt16(TimeDivision.RawValue);
	}

	/// <summary>
	/// Outputs the Format, NumTracks and TimeDivision values as a string
	/// </summary>
	/// <returns>A string containing the Format, NumTracks and TimeDivision values</returns>
	public override string ToString()
	{
		return $"<{EXPECTED_NAME}>"
			+ $"{Environment.NewLine}\t{nameof(Format)}: {Format}"
			+ $"{Environment.NewLine}\t{nameof(NumTracks)}: {NumTracks}"
			+ $"{Environment.NewLine}\t{nameof(TimeDivision)}: {TimeDivision}";
	}
}
