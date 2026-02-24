using Kermalis.EndianBinaryIO;

namespace Kermalis.MIDI;

public abstract class MIDIMessage
{
	internal abstract bool IsInvalid { get; set; }
	internal abstract byte GetCMDByte();

	internal abstract void Write(EndianBinaryWriter w);
}
