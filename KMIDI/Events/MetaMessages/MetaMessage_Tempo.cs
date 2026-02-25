using System;
using System.IO;

namespace Kermalis.MIDI;

public sealed partial class MetaMessage : MIDIMessage
{
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a Tempo type using a Beats Per Minute (BPM) value
	/// </summary>
	/// <param name="beatsPerMinute">The Beats Per Minute (BPM) value</param>
	/// <returns>A new <see cref="MetaMessage"/> as a Tempo type with specified Beats Per Minute (BPM)</returns>
	/// <exception cref="ArgumentOutOfRangeException">When the Beats Per Minute is 0 or lower</exception>
	public static MetaMessage CreateTempoMessage(in decimal beatsPerMinute)
	{
		if (beatsPerMinute <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(beatsPerMinute), beatsPerMinute, null);
		}

		return CreateTempoMessage((uint)(60_000_000m / beatsPerMinute));
	}
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a Tempo type using a Microseconds Per Quarter Note (MSPQN) value
	/// </summary>
	/// <param name="microsecondsPerQuarterNote">The Microseconds Per Quarter Note (MSPQN) value</param>
	/// <returns>A new <see cref="MetaMessage"/> as a Tempo type with specified Microseconds Per Quarter Note (MSPQN)</returns>
	/// <exception cref="ArgumentOutOfRangeException">If the Microseconds Per Quarter Note (MSPQN) is higher than 16777215 (0xFFFFFF)</exception>
	public static MetaMessage CreateTempoMessage(uint microsecondsPerQuarterNote)
	{
		if (microsecondsPerQuarterNote > 0xFFFFFF)
		{
			throw new ArgumentOutOfRangeException(nameof(microsecondsPerQuarterNote), microsecondsPerQuarterNote, null);
		}

		byte[] data = new byte[3];
		data[2] = (byte)microsecondsPerQuarterNote;
		data[1] = (byte)(microsecondsPerQuarterNote >> 8);
		data[0] = (byte)(microsecondsPerQuarterNote >> 16);
		return new MetaMessage(MetaMessageType.Tempo, data);
	}
	/// <summary>
	/// Reads and outputs the Tempo value of the <see cref="MetaMessage"/> in both Microseconds Per Quarter Note (MSPQN) and Beats Per Minute (BPM)
	/// </summary>
	/// <param name="microsecondsPerQuarterNote">The Microseconds Per Quarter Note (MSPQN) value to output</param>
	/// <param name="beatsPerMinute">The Beats Per Minute (BPM) value to output</param>
	/// <exception cref="InvalidDataException">If the type isn't a Tempo Meta Message type</exception>
	public void ReadTempoMessage(out uint microsecondsPerQuarterNote, out decimal beatsPerMinute)
	{
		if (Type is not MetaMessageType.Tempo)
		{
			throw new InvalidDataException("This Meta Message is not a Tempo Meta Message");
		}
		microsecondsPerQuarterNote = Data[2] | ((uint)Data[1] << 8) | ((uint)Data[0] << 16);
		beatsPerMinute = 60_000_000m / microsecondsPerQuarterNote;
	}
}
