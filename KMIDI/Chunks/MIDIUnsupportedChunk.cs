using Kermalis.EndianBinaryIO;
using System;
using System.Diagnostics;
using System.IO;

namespace Kermalis.MIDI;

public sealed class MIDIUnsupportedChunk : MIDIChunk
{
	/// <summary>Length 4</summary>
	public string ChunkName { get; }
	public byte[] Data { get; }

	/// <summary>
	/// If the unsupported chunk has errors, this will be set to <b>true</b>
	/// </summary>
	public override bool HasErrors { get; internal set; }

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

	public override void Write(EndianBinaryWriter w)
	{
		w.WriteChars_Count(ChunkName, 4);
		w.WriteUInt32((uint)Data.Length);

		w.WriteBytes(Data);
	}

	public override string ToString()
	{
		return $"<{ChunkName}> [{Data.Length} bytes]";
	}
}
