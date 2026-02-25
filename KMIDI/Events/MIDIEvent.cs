namespace Kermalis.MIDI;

/// <summary>
/// The MIDI Event Interface
/// </summary>
public interface IMIDIEvent
{
	/// <summary>The total amount of ticks leading up to this event</summary>
	int Ticks { get; }
	/// <summary>How many ticks are between this event and the previous one. If this is the first event in the track, then it is equal to <see cref="Ticks"/></summary>
	int DeltaTicks => Prev is null ? Ticks : Ticks - Prev.Ticks;

	/// <summary>MIDI Message data</summary>
	MIDIMessage Msg { get; }

	/// <summary>Contains the data of the previous MIDI Event</summary>
	IMIDIEvent? Prev { get; }
	/// <summary>Contains the data of the next MIDI Event</summary>
	IMIDIEvent? Next { get; }
}
/// <summary>
/// A MIDI Event Interface allowing for specifying the MIDI Event type
/// </summary>
/// <typeparam name="T">The type of MIDI Event class to use</typeparam>
public interface IMIDIEvent<T> : IMIDIEvent
	where T : MIDIMessage
{
	new T Msg { get; }
}

internal abstract class MIDIEvent : IMIDIEvent
{
	public int Ticks { get; }
	public MIDIEvent? IPrev { get; set; }
	public MIDIEvent? INext { get; set; }

	public abstract MIDIMessage Msg { get; }

	public IMIDIEvent? Prev => IPrev;
	public IMIDIEvent? Next => INext;

	protected MIDIEvent(int ticks)
	{
		Ticks = ticks;
	}

	public override string ToString()
	{
		return string.Format("@{0} = {1}", Ticks, Msg);
	}
}
internal sealed class MIDIEvent<T> : MIDIEvent, IMIDIEvent<T>
	where T : MIDIMessage
{
	public T IMsg { get; set; }

	public override MIDIMessage Msg => IMsg;
	T IMIDIEvent<T>.Msg => IMsg;

	public MIDIEvent(int ticks, T msg)
		: base(ticks)
	{
		IMsg = msg;
	}
}