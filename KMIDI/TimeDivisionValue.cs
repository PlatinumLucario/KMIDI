namespace Kermalis.MIDI;

// Section 2.1
public readonly struct TimeDivisionValue
{
	/// <summary>
	/// The minimum of Pulses Per Quarter Note
	/// </summary>
	public const int PPQN_MIN_DIVISION = 24;

	/// <summary>
	/// The raw 16-bit value
	/// </summary>
	public readonly ushort RawValue;

	/// <summary>
	/// The Type of Time Division
	/// </summary>
	public TimeDivisionType Type => (TimeDivisionType)(RawValue >> 15);

	/// <summary>
	/// The Ticks Per Quarter Note from the Pulses Per Quarter Note (PPQN)
	/// </summary>
	public ushort PPQN_TicksPerQuarterNote => RawValue; // Type bit is already 0

	/// <summary>
	/// The SMPTE Format type
	/// </summary>
	public SMPTEFormat SMPTE_Format => (SMPTEFormat)(-(sbyte)(RawValue >> 8)); // Upper 8 bits, negated
	/// <summary>
	/// The SMPTE Ticks Per Frame value
	/// </summary>
	public byte SMPTE_TicksPerFrame => (byte)RawValue; // Lower 8 bits

	/// <summary>
	/// Creates a new TimeDivisionValue using a 16-bit rawValue
	/// </summary>
	/// <param name="rawValue">The 16-bit Raw Value</param>
	public TimeDivisionValue(ushort rawValue)
	{
		RawValue = rawValue;
	}

	/// <summary>
	/// Creates a new Time Division Value as a Pulse Per Quarter Note (PPQN)
	/// </summary>
	/// <param name="ticksPerQuarterNote">The number of Ticks Per Quarter Note</param>
	/// <returns>A new Time Division Value as a Pulse Per Quarter Note (PPQN)</returns>
	public static TimeDivisionValue CreatePPQN(ushort ticksPerQuarterNote)
	{
		return new TimeDivisionValue(ticksPerQuarterNote);
	}
	/// <summary>
	/// Creates a new Time Division Value as a SMPTE tick timecode
	/// </summary>
	/// <param name="format">The SMPTE Format to use</param>
	/// <param name="ticksPerFrame">The amount of Ticks Per Frame to use</param>
	/// <returns>A new Time Division Value as a SMPTE timecode</returns>
	public static TimeDivisionValue CreateSMPTE(SMPTEFormat format, byte ticksPerFrame)
	{
		ushort rawValue = (ushort)((-(sbyte)format) << 8);
		rawValue |= ticksPerFrame;

		return new TimeDivisionValue(rawValue);
	}

	/// <summary>
	/// Checks if the Time Division Value is valid
	/// </summary>
	/// <returns><b>true</b> if valid, otherwise <b>false</b></returns>
	public bool IsValid()
	{
		if (Type == TimeDivisionType.PPQN)
		{
			return PPQN_TicksPerQuarterNote >= PPQN_MIN_DIVISION;
		}

		// SMPTE
		return SMPTE_Format is SMPTEFormat.Smpte24 or SMPTEFormat.Smpte25 or SMPTEFormat.Smpte30Drop or SMPTEFormat.Smpte30;
	}

	/// <summary>
	/// Outputs a string containing either the Pulse Per Quarter Note (PPQN) value, SMPTE Format and Ticks Per Frame, or invalid if the type is isn't valid
	/// </summary>
	/// <returns>A string containing either a PPQN value, SMPTE Format and ticks, or an Invalid type</returns>
	public override string ToString()
	{
        return Type switch
        {
            TimeDivisionType.PPQN => string.Format("PPQN [TicksPerQuarterNote: {0}]", PPQN_TicksPerQuarterNote),
            TimeDivisionType.SMPTE => string.Format("SMPTE [Format: {0}, TicksPerFrame: {1}]", SMPTE_Format, SMPTE_TicksPerFrame),
            _ => string.Format("INVALID [0x{0:X4}]", RawValue),
        };
    }
}
