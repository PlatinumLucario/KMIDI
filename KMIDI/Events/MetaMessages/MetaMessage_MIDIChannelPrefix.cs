using Kermalis.EndianBinaryIO;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Kermalis.MIDI;

public sealed partial class MetaMessage : MIDIMessage
{
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a MIDI Channel Prefix
	/// </summary>
	/// <param name="channel">The MIDI Channel Prefix value</param>
	/// <returns>A new <see cref="MetaMessage"/> as a MIDI Channel Prefix type</returns>
	public static MetaMessage CreateMIDIChannelPrefixMessage(byte channel)
	{
		Utils.ValidateMIDIChannel(channel);

		byte[] data = [channel];
		return new MetaMessage(MetaMessageType.MIDIChannelPrefix, data);
	}
	/// <summary>
	/// Reads and outputs the MIDI Channel Prefix of the <see cref="MetaMessage"/>
	/// </summary>
	/// <param name="channel">The MIDI Channel Prefix output</param>
	/// <exception cref="InvalidDataException">If the type isn't a MIDI Channel Prefix Meta Message type</exception>
	public void ReadMIDIChannelPrefixMessage(out byte channel)
	{
		if (Type is not MetaMessageType.MIDIChannelPrefix)
		{
			throw new InvalidDataException("This Meta Message is not a MIDI Channel Prefix Meta Message");
		}
		channel = Data[0];
	}
}
