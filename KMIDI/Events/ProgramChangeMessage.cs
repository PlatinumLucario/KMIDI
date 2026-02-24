using Kermalis.EndianBinaryIO;
using System;

namespace Kermalis.MIDI;

public sealed class ProgramChangeMessage : MIDIMessage, IMIDIChannelMessage
{
	internal override bool IsInvalid { get; set; }

	/// <summary>
	/// The MIDI Channel used
	/// </summary>
	public byte Channel { get; }

	/// <summary>
	/// The MIDI Program used
	/// </summary>
	public MIDIProgram Program { get; }

	internal ProgramChangeMessage(EndianBinaryReader r, byte channel, bool isInvalid)
	{
		IsInvalid = isInvalid;
		Channel = channel;

		Program = r.ReadEnum<MIDIProgram>();
		if (Program >= MIDIProgram.MAX)
		{
			Utils.ThrowInvalidMessageDataException(nameof(ProgramChangeMessage), nameof(Program), r.Stream.Position - 1, Program);
		}
	}

	/// <summary>
	/// Creates a new Program Change Message
	/// </summary>
	/// <param name="channel">The MIDI Channel to use</param>
	/// <param name="program">The MIDI Program to change to</param>
	/// <exception cref="ArgumentOutOfRangeException">If the MIDI Program value is more than 127</exception>
	public ProgramChangeMessage(byte channel, MIDIProgram program)
	{
		Utils.ValidateMIDIChannel(channel);
		if (program >= MIDIProgram.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(program), program, null);
		}

		Channel = channel;
		Program = program;
	}

	internal override byte GetCMDByte()
	{
		return (byte)(0xC0 + Channel);
	}

	internal override void Write(EndianBinaryWriter w)
	{
		w.WriteEnum(Program);
	}

	/// <summary>
	/// Outputs a string with the details of the <see cref="ProgramChangeMessage"/>
	/// </summary>
	/// <returns>A string containing details of the <see cref="ProgramChangeMessage"/></returns>
	public override string ToString()
	{
		return $"{nameof(ProgramChangeMessage)} [{nameof(Channel)} {Channel}"
			+ $", {nameof(Program)}: {Program}"
			+ ']';
	}
}