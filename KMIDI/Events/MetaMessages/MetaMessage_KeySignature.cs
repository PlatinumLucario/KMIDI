using System;
using System.IO;

namespace Kermalis.MIDI;

public sealed partial class MetaMessage : MIDIMessage
{
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a Key Signature type
	/// </summary>
	/// <param name="sf">The Sharp/C/Flat value</param>
	/// <param name="mi">To determine if it's Minor, Major or MAX</param>
	/// <returns>A new <see cref="MetaMessage"/> as a Key Signature type with specified SF and MI values</returns>
	/// <exception cref="ArgumentOutOfRangeException">If the SF or MI values are out of range</exception>
	public static MetaMessage CreateKeySignatureMessage(KeySignatureSF sf, KeySignatureMI mi)
	{
		if (sf is <= KeySignatureSF.MIN or >= KeySignatureSF.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(sf), sf, null);
		}
		if (mi >= KeySignatureMI.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(mi), mi, null);
		}

		byte[] data = [(byte)sf, (byte)mi];
		return new MetaMessage(MetaMessageType.KeySignature, data);
	}
	/// <summary>
	/// Reads and output the Key Signature values from the <see cref="MetaMessage"/>
	/// </summary>
	/// <param name="sf">The Sharp/C/Flat value output</param>
	/// <param name="mi">The Minor, Major, MAX value output</param>
	/// <exception cref="InvalidDataException">If the <see cref="MetaMessage"/> is not a Key Signature</exception>
	public void ReadKeySignatureMessage(out KeySignatureSF sf, out KeySignatureMI mi)
	{
		if (Type is not MetaMessageType.KeySignature)
		{
			throw new InvalidDataException("This Meta Message is not a Key Signature Meta Message");
		}
		sf = (KeySignatureSF)Data[0];
		mi = (KeySignatureMI)Data[1];
	}
}
