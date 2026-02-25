using Kermalis.EndianBinaryIO;
using System;
using System.Diagnostics;
using System.IO;

namespace Kermalis.MIDI;

public sealed class MIDIUnsupportedChunk : MIDIChunk
{
	/// <summary>Length 4</summary>
	public string ChunkName { get; }
	/// <summary>The Data byte array containing the values</summary>
	public byte[] Data { get; }

	/// <summary>
	/// If the unsupported chunk has errors, this will be set to <b>true</b>
	/// </summary>
	public override bool HasErrors { get; internal set; }

	/// <summary>
	/// Creates a new MIDI Unsupported Chunk
	/// </summary>
	/// <param name="chunkName">The name of the chunk</param>
	/// <param name="data">The data byte array to insert</param>
	/// <exception cref="ArgumentOutOfRangeException">If the chunk name length is not equal to 4</exception>
	public MIDIUnsupportedChunk(string chunkName, byte[] data)
	{
		if (chunkName.Length != 4)
		{
			throw new ArgumentOutOfRangeException(nameof(chunkName), chunkName, null);
		}

		ChunkName = chunkName;
		Data = data;
	}
	internal MIDIUnsupportedChunk(string chunkName, uint size, EndianBinaryReader r)
	{
		ChunkName = chunkName;
		Data = new byte[size];
		if (!MIDIFile.SkipErrors)
		{
			r.ReadBytes(Data);
		}
		else
		{
			if (r.Stream.Position + Data.Length < r.Stream.Length)
			{
				r.ReadBytes(Data);
			}
			else
			{
				HasErrors = true;
				EndOfStreamException ex = new("The size of this chunk exceeds the size of the MIDI file");
				Debug.WriteLine(ex.Message);
			}
		}
	}

	/// <summary>
	/// Writes the Unsupported MIDI Chunk to memory
	/// </summary>
	/// <param name="w">The EndianBinaryWriter stream to use</param>
	public override void Write(EndianBinaryWriter w)
	{
		w.WriteChars_Count(ChunkName, 4);
		w.WriteUInt32((uint)Data.Length);

		w.WriteBytes(Data);
	}

	/// <summary>
	/// Outputs the ChunkName and Data byte array length as a string
	/// </summary>
	/// <returns>A string containing the ChunkName and Data byte array length</returns>
	public override string ToString()
	{
		return $"<{ChunkName}> [{Data.Length} bytes]";
	}
}
