using Kermalis.EndianBinaryIO;
using System;
using System.IO;

namespace Kermalis.MIDI;

public sealed class SysExContinuationMessage : MIDIMessage, ISysExMessage
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
	/// To check if the SysExContinuationMessage has finished
	/// </summary>
	public bool IsFinished => Data[^1] == 0xF7;

	internal SysExContinuationMessage(EndianBinaryReader r, bool isInvalid)
	{
		IsInvalid = isInvalid;
		long offset = r.Stream.Position;

		VariableLength = Utils.ReadVariableLength(r);
		if (VariableLength == 0)
		{
			throw new InvalidDataException($"{nameof(SysExContinuationMessage)} at 0x{offset:X} was empty");
		}

		Data = new byte[VariableLength];
		r.ReadBytes(Data);
	}

	/// <summary>
	/// Creates a new System Exclusive Continuation Message
	/// </summary>
	/// <param name="data">The Data byte array to insert</param>
	/// <exception cref="ArgumentException">If the Data length is 0 or the variable length is invalid</exception>
	public SysExContinuationMessage(byte[] data)
	{
		if (data.Length == 0 || !Utils.IsValidVariableLengthValue(data.Length))
		{
			throw new ArgumentException($"{nameof(SysExContinuationMessage)} data length must be [1, 0x0FFFFFFF]");
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
	/// Outputs a string with the details of the <see cref="SysExContinuationMessage"/>
	/// </summary>
	/// <returns>A string containing details of the <see cref="SysExContinuationMessage"/></returns>
	public override string ToString()
	{
		return $"{nameof(SysExContinuationMessage)} [Length: {Data.Length}"
			+ $", {nameof(IsFinished)}: {IsFinished}"
			+ ']';
	}
}
