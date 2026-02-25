using System;
using System.IO;
using System.Text;

namespace Kermalis.MIDI;

public sealed partial class MetaMessage : MIDIMessage
{
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a text type
	/// </summary>
	/// <param name="type">The type of text MetaMessage to create</param>
	/// <param name="text">The text to insert into the MetaMessage</param>
	/// <returns>A new <see cref="MetaMessage"/> as one of the text types with the specified text string</returns>
	/// <exception cref="ArgumentOutOfRangeException">If the MetaMessageType is invalid</exception>
	public static MetaMessage CreateTextMessage(MetaMessageTextType type, string text)
	{
		if (type is < MetaMessageTextType.Text or > MetaMessageTextType.Reserved_F)
		{
			throw new ArgumentOutOfRangeException(nameof(type), type, null);
		}

		byte[] data = Encoding.ASCII.GetBytes(text);
		return new MetaMessage((MetaMessageType)type, data);
	}
	[Obsolete("Using the MetaMessageType enum with CreateTextMessage has been deprecated, please use the MetaMessageTextType enum with CreateTextMessage instead")]
	public static MetaMessage CreateTextMessage(MetaMessageType type, string text) => CreateTextMessage(type, text);
	/// <summary>
	/// Reads and outputs the text of the <see cref="MetaMessage"/>
	/// </summary>
	/// <param name="text">The text string output</param>
	/// <exception cref="InvalidDataException">If the type isn't a text Meta Message type</exception>
	public void ReadTextMessage(out string text)
	{
		if (Type is < MetaMessageType.Text or > MetaMessageType.Reserved_F)
		{
			throw new InvalidDataException("This Meta Message is not a Text Meta Message");
		}
		text = Encoding.ASCII.GetString(Data);
	}
}
