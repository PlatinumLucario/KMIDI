using Kermalis.EndianBinaryIO;
using System;

namespace Kermalis.MIDI;

public sealed class ChannelPressureMessage : MIDIMessage, IMIDIChannelMessage
{
	internal override bool IsInvalid { get; set; }

	/// <summary>
	/// The MIDI Channel used
	/// </summary>
	public byte Channel { get; }

	/// <summary>
	/// The amount of pressure used
	/// </summary>
	public byte Pressure { get; }

	internal ChannelPressureMessage(EndianBinaryReader r, byte channel, bool isInvalid)
	{
		IsInvalid = isInvalid;
		Channel = channel;

		Pressure = r.ReadByte();
		if (Pressure > 127)
		{
			Utils.ThrowInvalidMessageDataException(nameof(ChannelPressureMessage), nameof(Pressure), r.Stream.Position - 1, Pressure);
		}
	}
	/// <summary>
	/// Creates a new Channel Pressure Message
	/// </summary>
	/// <param name="channel">The MIDI Channel to use</param>
	/// <param name="pressure">The amount of Pressure to apply</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public ChannelPressureMessage(byte channel, byte pressure)
	{
		Utils.ValidateMIDIChannel(channel);
		if (pressure > 127)
		{
			throw new ArgumentOutOfRangeException(nameof(pressure), pressure, null);
		}

		Channel = channel;
		Pressure = pressure;
	}

	internal override byte GetCMDByte()
	{
		return (byte)(0xD0 + Channel);
	}

	internal override void Write(EndianBinaryWriter w)
	{
		w.WriteByte(Pressure);
	}

	/// <summary>
	/// Outputs a string with the details of the <see cref="ChannelPressureMessage"/>
	/// </summary>
	/// <returns>A string containing details of the <see cref="ChannelPressureMessage"/></returns>
	public override string ToString()
	{
		return $"{nameof(ChannelPressureMessage)} [{nameof(Channel)} {Channel}"
			+ $", {nameof(Pressure)}: {Pressure}"
			+ ']';
	}
}
