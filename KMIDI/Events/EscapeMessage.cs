using Kermalis.EndianBinaryIO;
using System;

namespace Kermalis.MIDI;

public sealed class EscapeMessage : MIDIMessage
{
	/// <summary>
	/// The Variable Length of the data
	/// </summary>
	public int VariableLength { get; }
	/// <summary>
	/// The raw data in a byte array
	/// </summary>
	public byte[] Data { get; }
	internal override bool IsInvalid { get; set; }

	internal EscapeMessage(EndianBinaryReader r, bool isInvalid)
	{
		IsInvalid = isInvalid;
		VariableLength = Utils.ReadVariableLength(r);
		if (VariableLength == 0)
		{
			Data = Array.Empty<byte>();
		}
		else
		{
			Data = new byte[VariableLength];
			r.ReadBytes(Data);
		}
	}

	/// <summary>
	/// Creates a new Escape Message
	/// </summary>
	/// <param name="data">The Data to use</param>
	/// <exception cref="ArgumentException">If the data is not [0, 0x0FFFFFFF]</exception>
	public EscapeMessage(byte[] data)
	{
		if (!Utils.IsValidVariableLengthValue(data.Length))
		{
			throw new ArgumentException($"{nameof(EscapeMessage)} data length must be [0, 0x0FFFFFFF]");
		}

		Data = data;
		VariableLength = Data.Length;
	}

	internal override byte GetCMDByte()
	{
		return 0xF7;
	}

	internal override void Write(EndianBinaryWriter w)
	{
		Utils.WriteVariableLength(w, Data.Length);
		w.WriteBytes(Data);
	}

	/// <summary>
	/// Outputs a string with the details of the <see cref="EscapeMessage"/>
	/// </summary>
	/// <returns>A string containing details of the <see cref="EscapeMessage"/></returns>
	public override string ToString()
	{
		return $"{nameof(EscapeMessage)} [Length: {Data.Length}]";
	}
}
