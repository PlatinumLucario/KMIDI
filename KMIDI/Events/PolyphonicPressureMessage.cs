using Kermalis.EndianBinaryIO;
using System;

namespace Kermalis.MIDI;

public sealed class PolyphonicPressureMessage : MIDIMessage, IMIDIChannelMessage
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
	/// The amount of Pressure used
	/// </summary>
	public byte Pressure { get; }

	internal PolyphonicPressureMessage(EndianBinaryReader r, byte channel, bool isInvalid)
	{
		IsInvalid = isInvalid;
		Channel = channel;

		Note = r.ReadEnum<MIDINote>();
		if (Note >= MIDINote.MAX)
		{
			Utils.ThrowInvalidMessageDataException(nameof(PolyphonicPressureMessage), nameof(Note), r.Stream.Position - 1, Note);
		}

		Pressure = r.ReadByte();
		if (Pressure > 127)
		{
			Utils.ThrowInvalidMessageDataException(nameof(PolyphonicPressureMessage), nameof(Pressure), r.Stream.Position - 1, Pressure);
		}
	}

	/// <summary>
	/// Creates a new Polyphonic Pressure Message
	/// </summary>
	/// <param name="channel">The MIDI Channel to use</param>
	/// <param name="note">The Note to use</param>
	/// <param name="pressure">The amount of Pressure to use</param>
	/// <exception cref="ArgumentOutOfRangeException">If the Pressure is more than 127</exception>
	public PolyphonicPressureMessage(byte channel, MIDINote note, byte pressure)
	{
		Utils.ValidateMIDIChannel(channel);
		if (pressure > 127)
		{
			throw new ArgumentOutOfRangeException(nameof(pressure), pressure, null);
		}

		Channel = channel;
		Note = note;
		Pressure = pressure;
	}

	internal override byte GetCMDByte()
	{
		return (byte)(0xA0 + Channel);
	}

	internal override void Write(EndianBinaryWriter w)
	{
		w.WriteEnum(Note);
		w.WriteByte(Pressure);
	}

	/// <summary>
	/// Outputs a string with the details of the <see cref="PolyphonicPressureMessage"/>
	/// </summary>
	/// <returns>A string containing details of the <see cref="PolyphonicPressureMessage"/></returns>
	public override string ToString()
	{
		return $"{nameof(PolyphonicPressureMessage)} [{nameof(Channel)} {Channel}"
			+ $", {nameof(Note)}: {Note}"
			+ $", {nameof(Pressure)}: {Pressure}"
			+ ']';
	}
}
