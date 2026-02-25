using Kermalis.EndianBinaryIO;
using System.IO;

namespace Kermalis.MIDI;

public sealed partial class MetaMessage : MIDIMessage
{
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a Sequence Number type
	/// </summary>
	/// <param name="sequenceID">The sequence number to add</param>
	/// <returns>A new <see cref="MetaMessage"/> as a Sequence Number type with the specified sequence ID</returns>
	public static MetaMessage CreateSequenceNumberMessage(ushort sequenceID)
	{
		byte[] data = new byte[2];
		EndianBinaryPrimitives.WriteUInt16_Unsafe(data, sequenceID, Endianness.BigEndian);
		return new MetaMessage(MetaMessageType.SequenceNumber, data);
	}
	/// <summary>
	/// Reads and outputs the sequence number from the <see cref="MetaMessage"/>
	/// </summary>
	/// <param name="sequenceID">The sequence ID to output</param>
	public void ReadSequenceNumberMessage(out ushort sequenceID)
	{
		if (Type is not MetaMessageType.SequenceNumber)
		{
			throw new InvalidDataException("This Meta Message is not a Sequence Number Meta Message");
		}
		sequenceID = EndianBinaryPrimitives.ReadUInt16(Data, Endianness.BigEndian);
	}
}
