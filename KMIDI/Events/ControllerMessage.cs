using Kermalis.EndianBinaryIO;
using System;

namespace Kermalis.MIDI;

public sealed class ControllerMessage : MIDIMessage, IMIDIChannelMessage
{
	internal override bool IsInvalid { get; set; }

	/// <summary>
	/// The MIDI Channel used
	/// </summary>
	public byte Channel { get; }

	/// <summary>
	/// The Controller Type used
	/// </summary>
	public ControllerType Controller { get; }
	/// <summary>
	/// The Controller Value used
	/// </summary>
	public byte Value { get; }

	internal ControllerMessage(EndianBinaryReader r, byte channel, bool isInvalid)
	{
		IsInvalid = isInvalid;
		Channel = channel;

		Controller = r.ReadEnum<ControllerType>();
		if (Controller >= ControllerType.MAX)
		{
			Utils.ThrowInvalidMessageDataException(nameof(ControllerMessage), nameof(Controller), r.Stream.Position - 1, Controller);
		}

		Value = r.ReadByte();
		if (Value > 127)
		{
			Utils.ThrowInvalidMessageDataException(nameof(ControllerMessage), nameof(Value), r.Stream.Position - 1, Value);
		}
	}

	/// <summary>
	/// Creates a new Controller Message
	/// </summary>
	/// <param name="channel">The MIDI Channel to use</param>
	/// <param name="controller">The Controller Type to use</param>
	/// <param name="value">The Controller Value to use</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public ControllerMessage(byte channel, ControllerType controller, byte value)
	{
		Utils.ValidateMIDIChannel(channel);
		if (controller >= ControllerType.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(controller), controller, null);
		}
		if (value > 127)
		{
			throw new ArgumentOutOfRangeException(nameof(value), value, null);
		}

		Channel = channel;
		Controller = controller;
		Value = value;
	}

	internal override byte GetCMDByte()
	{
		return (byte)(0xB0 + Channel);
	}

	internal override void Write(EndianBinaryWriter w)
	{
		w.WriteEnum(Controller);
		w.WriteByte(Value);
	}

	/// <summary>
	/// Outputs a string with the details of the <see cref="ControllerMessage"/>
	/// </summary>
	/// <returns>A string containing details of the <see cref="ControllerMessage"/></returns>
	public override string ToString()
	{
		return $"{nameof(ControllerMessage)} [{nameof(Channel)} {Channel}"
			+ $", {nameof(Controller)}: {Controller}"
			+ $", {nameof(Value)}: {Value}"
			+ ']';
	}
}