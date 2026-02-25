using Kermalis.EndianBinaryIO;
using System;
using System.IO;

namespace Kermalis.MIDI;

public sealed partial class MetaMessage : MIDIMessage
{
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a Proprietary Event
	/// </summary>
	/// <param name="manufacturer">The value that determines the Manufacturer</param>
	/// <param name="msgData">The raw Message Data in a byte array</param>
	/// <returns>A new <see cref="MetaMessage"/> as a Proprietary Event with the specified Manufacturer and Message Data</returns>
	public static MetaMessage CreateProprietaryEventMessage(ushort manufacturer, ReadOnlySpan<byte> msgData)
	{
		Span<byte> id = stackalloc byte[3];
		int idLen;
		if (manufacturer is 0 or > byte.MaxValue)
		{
			idLen = 3;
			id[0] = 0;
			EndianBinaryPrimitives.WriteUInt16(id[1..], manufacturer, Endianness.BigEndian);
		}
		else
		{
			idLen = 1;
			id[0] = (byte)manufacturer;
		}

		byte[] data = new byte[idLen + msgData.Length];
		id[..idLen].CopyTo(data);
		msgData.CopyTo(data.AsSpan(idLen));
		return new MetaMessage(MetaMessageType.ProprietaryEvent, data);
	}
	/// <summary>
	/// Reads and outputs the Proprietary Event values from the <see cref="MetaMessage"/>
	/// </summary>
	/// <param name="manufacturer">The value output that determines the Menufacturer</param>
	/// <param name="msgData">The Message Data output as a byte array</param>
	/// <exception cref="InvalidDataException">If the <see cref="MetaMessage"/> is not a Proprietary Message type</exception>
	public void ReadProprietaryEventMessage(out ushort manufacturer, out ReadOnlySpan<byte> msgData)
	{
		if (Type is not MetaMessageType.ProprietaryEvent)
		{
			throw new InvalidDataException("This Meta Message is not a Proprietary Event Meta Message");
		}
		manufacturer = Data[0];
		int idLen = 1;
		if (manufacturer == 0)
		{
			manufacturer = EndianBinaryPrimitives.ReadUInt16(Data.AsSpan(1, 2), Endianness.BigEndian);
			idLen = 3;
		}
		msgData = Data.AsSpan(idLen);
	}
}
