namespace Kermalis.MIDI;

/// <summary>
/// Interface for Channel Messages
/// </summary>
public interface IMIDIChannelMessage
{
	byte Channel { get; }
}
/// <summary>
/// Interface for System Exclusive Messages
/// </summary>
public interface ISysExMessage
{
	byte[] Data { get; }
}