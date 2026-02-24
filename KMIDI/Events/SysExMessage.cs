using Kermalis.EndianBinaryIO;
using System;
using System.IO;

namespace Kermalis.MIDI;

public sealed class SysExMessage : MIDIMessage, ISysExMessage
{
	/// <summary>
	/// The Variable Length of the data
	/// </summary>
	public int VariableLength { get; }
	/// <summary>
	/// The raw Data in a byte array
	/// </summary>
	public byte[] Data { get; }
	internal override bool IsInvalid { get; set; }

	/// <summary>
	/// To check if the SysExMessage has completed
	/// </summary>
	public bool IsComplete => Data[^1] == 0xF7;

	internal SysExMessage(EndianBinaryReader r, bool isInvalid)
	{
		IsInvalid = isInvalid;
		long offset = r.Stream.Position;

		VariableLength = Utils.ReadVariableLength(r);
		if (VariableLength == 0)
		{
			throw new InvalidDataException($"{nameof(SysExMessage)} at 0x{offset:X} was empty");
		}

		Data = new byte[VariableLength];
		r.ReadBytes(Data);
	}

	/// <summary>
	/// Creates a new System Exclusive Message
	/// </summary>
	/// <param name="data">The Data byte array to insert</param>
	/// <exception cref="ArgumentException">If the Data length is 0 or the variable length is invalid</exception>
	public SysExMessage(byte[] data)
	{
		if (data.Length == 0 || !Utils.IsValidVariableLengthValue(data.Length))
		{
			throw new ArgumentException($"{nameof(SysExMessage)} data length must be [1, 0x0FFFFFFF]");
		}

		Data = data;
		VariableLength = Data.Length;
	}

	internal override byte GetCMDByte()
	{
		return 0xF0;
	}

	internal override void Write(EndianBinaryWriter w)
	{
		Utils.WriteVariableLength(w, Data.Length);
		w.WriteBytes(Data);
	}

	/// <summary>
	/// Outputs a string with the details of the <see cref="SysExMessage"/>
	/// </summary>
	/// <returns>A string containing details of the <see cref="SysExMessage"/></returns>
	public override string ToString()
	{
		return $"{nameof(SysExMessage)} [Length: {Data.Length}"
			+ $", {nameof(IsComplete)}: {IsComplete}"
			+ ']';
	}
}
