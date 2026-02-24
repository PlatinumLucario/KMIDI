using Kermalis.EndianBinaryIO;
using System.IO;

namespace Kermalis.MIDI;

public abstract class MIDIChunk
{
	/// <summary>
	/// If the chunk has errors, this will be set to <b>true</b>
	/// </summary>
	public abstract bool HasErrors { get; internal set; }
	protected static long GetEndOffset(EndianBinaryReader r, uint size)
	{
		return r.Stream.Position + size;
	}
	protected static void EatRemainingBytes(EndianBinaryReader r, long endOffset, string chunkName, uint size)
	{
		if (r.Stream.Position > endOffset)
		{
			throw new InvalidDataException($"Chunk was too short ({chunkName} = {size})");
		}
		r.Stream.Position = endOffset;
	}

	public abstract void Write(EndianBinaryWriter w);
}
