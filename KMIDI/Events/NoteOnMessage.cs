using Kermalis.EndianBinaryIO;
using System;

namespace Kermalis.MIDI;

public sealed class NoteOnMessage : MIDIMessage, IMIDIChannelMessage
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

	internal NoteOnMessage(EndianBinaryReader r, byte channel, bool isInvalid)
	{
		IsInvalid = isInvalid;
		Channel = channel;

		Note = r.ReadEnum<MIDINote>();
		if (Note >= MIDINote.MAX)
		{
			Utils.ThrowInvalidMessageDataException(nameof(NoteOnMessage), nameof(Note), r.Stream.Position - 1, Note);
		}

		Velocity = r.ReadByte();
		if (Velocity > 127)
		{
			Utils.ThrowInvalidMessageDataException(nameof(NoteOnMessage), nameof(Velocity), r.Stream.Position - 1, Velocity);
		}
	}

	/// <summary>
	/// Creates a new Note On Message
	/// </summary>
	/// <param name="channel">The MIDI Channel to use</param>
	/// <param name="note">The Note to use</param>
	/// <param name="velocity">The amount of Velocity to use</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public NoteOnMessage(byte channel, MIDINote note, byte velocity)
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
		return (byte)(0x90 + Channel);
	}

	internal override void Write(EndianBinaryWriter w)
	{
		w.WriteEnum(Note);
		w.WriteByte(Velocity);
	}

	/// <summary>
	/// Outputs a string with the details of the <see cref="NoteOnMessage"/>
	/// </summary>
	/// <returns>A string containing details of the <see cref="NoteOnMessage"/></returns>
	public override string ToString()
	{
		return $"{nameof(NoteOnMessage)} [{nameof(Channel)} {Channel}"
			+ $", {nameof(Note)}: {Note}"
			+ $", {nameof(Velocity)}: {Velocity}"
			+ ']';
	}
}
