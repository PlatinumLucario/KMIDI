using System;
using System.Diagnostics;
using System.IO;

namespace Kermalis.MIDI;

public sealed partial class MetaMessage : MIDIMessage
{
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a Time Signature type
	/// </summary>
	/// <param name="numerator">The Numerator to specify</param>
	/// <param name="denominator">The Denominator to specify</param>
	/// <param name="clocksPerMetronomeClick">The amount of Clocks Per Metronome Click (CPMC) to use</param>
	/// <param name="num32ndNotesPerQuarterNote">The number of 32nd Notes Per Quarter Note (32NPQN) to use</param>
	/// <returns>A new <see cref="MetaMessage"/> as a Time Signature type with the specified Numerator, Denominator, Clocks Per Metronome Click and Number of 32nd Notes Per Quarter Notes</returns>
	/// <exception cref="ArgumentOutOfRangeException">If the Numerator, Clocks Per Metronome Click, or the Number of 32nd Notes Per Quarter Note is 0, or if the Denominator is less than 2 or more than 32</exception>
	/// <exception cref="ArgumentException">If the denominator isn't a power of two</exception>
	public static MetaMessage CreateTimeSignatureMessage(byte numerator, byte denominator, byte clocksPerMetronomeClick = 24, byte num32ndNotesPerQuarterNote = 8)
	{
		if (numerator == 0)
		{
			throw new ArgumentOutOfRangeException(nameof(numerator), numerator, null);
		}
		if (denominator is < 2 or > 32)
		{
			throw new ArgumentOutOfRangeException(nameof(denominator), denominator, null);
		}
		if (!Utils.IsPowerOfTwo(denominator))
		{
			throw new ArgumentException("Denominator must be a power of 2", nameof(denominator));
		}
		if (clocksPerMetronomeClick == 0)
		{
			throw new ArgumentOutOfRangeException(nameof(clocksPerMetronomeClick), clocksPerMetronomeClick, null);
		}
		if (num32ndNotesPerQuarterNote == 0)
		{
			throw new ArgumentOutOfRangeException(nameof(num32ndNotesPerQuarterNote), num32ndNotesPerQuarterNote, null);
		}

		byte[] data =
		[
			numerator,
			(byte)Math.Log(denominator, 2),
			clocksPerMetronomeClick,
			num32ndNotesPerQuarterNote,
		];
		return new MetaMessage(MetaMessageType.TimeSignature, data);
	}
	/// <summary>
	/// Reads and outputs the Time Signature values of the <see cref="MetaMessage"/>
	/// </summary>
	/// <param name="numerator">The Numerator value output</param>
	/// <param name="denominator">The Denominator value output</param>
	/// <param name="clocksPerMetronomeClick">The Clocks Per Metronome Click value output</param>
	/// <param name="num32ndNotesPerQuarterNote">The Number of 32nd Notes Per Quarter Note value output</param>
	/// <exception cref="InvalidDataException">If the <see cref="MetaMessage"/> type is not a Time Signature or the Denominator is more than 15</exception>
	public void ReadTimeSignatureMessage(out byte numerator, out byte denominator, out byte clocksPerMetronomeClick, out byte num32ndNotesPerQuarterNote)
	{
		if (Type is not MetaMessageType.TimeSignature)
		{
			throw new InvalidDataException("This Meta Message is not a Time Signature Meta Message");
		}
		numerator = Data[0];
		if (Data[1] >= 16)
		{
			InvalidDataException ex = new("Denominator needs to be a value of 15 or less.");
			if (!MIDIFile.SkipErrors)
			{
				throw ex;
			}
			else
			{
				IsInvalid = true;
				Debug.WriteLine(ex.Message);
			}
		}
		denominator = (byte)Math.Pow(Data[1], 2);
		clocksPerMetronomeClick = Data[2];
		num32ndNotesPerQuarterNote = Data[3];
	}
}
