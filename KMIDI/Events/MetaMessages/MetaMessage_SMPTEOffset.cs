using System.IO;

namespace Kermalis.MIDI;

public sealed partial class MetaMessage : MIDIMessage
{
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a SMPTE Offset type using rateAndHour byte, minute, second, frame and fractionalFrame values
	/// </summary>
	/// <param name="rateAndHour">The value of the rate shifted by 6, plus the number of hours</param>
	/// <param name="minute">The number of minutes</param>
	/// <param name="second">The number of seconds</param>
	/// <param name="frame">The number of frames per second</param>
	/// <param name="fractionalFrame">The number of fractional frames</param>
	/// <returns>A new <see cref="MetaMessage"/> as a SMPTE Offset type with the specified hours, minutes, seconds, frames and fractional frames</returns>
	public static MetaMessage CreateSMPTEOffsetMessage(byte rateAndHour, byte minute, byte second, byte frame, byte fractionalFrame)
	{
		byte[] data = [rateAndHour, minute, second, frame, fractionalFrame];
		Utils.ValidateSMPTEOffset(data);
		return new MetaMessage(MetaMessageType.SMPTEOffset, data);
	}
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a SMPTE Offset type using rate, hour, minute, second, frame and fractionalFrame values
	/// </summary>
	/// <param name="format">The SMPTE Format to use</param>
	/// <param name="hour">The number of hours</param>
	/// <param name="minute">The number of minutes</param>
	/// <param name="second">The number of seconds</param>
	/// <param name="frame">The number of frames per second</param>
	/// <param name="fractionalFrame">The number of fractional frames</param>
	/// <returns>A new <see cref="MetaMessage"/> as a SMPTE Offset type with the specified hours, minutes, seconds, frames and fractional frames</returns>
	public static MetaMessage CreateSMPTEOffsetMessage(SMPTEFormat format, byte hour, byte minute, byte second, byte frame, byte fractionalFrame)
	{
		byte rv = (byte)((byte)format << 5);
		byte rateAndHour = (byte)(rv + hour);
		byte[] data = [rateAndHour, minute, second, frame, fractionalFrame];
		Utils.ValidateSMPTEOffset(data);
		return new MetaMessage(MetaMessageType.SMPTEOffset, data);
	}
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a SMPTE Offset type using rate, hour, minute, second, frame and fractionalFrame values
	/// </summary>
	/// <param name="rate">The frame rate value, it will accept either the frame rate type or an actual frame rate value</param>
	/// <param name="hour">The number of hours</param>
	/// <param name="minute">The number of minutes</param>
	/// <param name="second">The number of seconds</param>
	/// <param name="frame">The number of frames per second</param>
	/// <param name="fractionalFrame">The number of fractional frames</param>
	/// <returns>A new <see cref="MetaMessage"/> as a SMPTE Offset type with the specified hours, minutes, seconds, frames and fractional frames</returns>
	public static MetaMessage CreateSMPTEOffsetMessage(float rate, byte hour, byte minute, byte second, byte frame, byte fractionalFrame)
	{
		byte rv = 3;
		if (rate > 3)
		{
			if (rate <= 24)
			{
				rv = 0;
			}
			else if (rate == 25)
			{
				rv = 1;
			}
			else if (rate <= 29.97f)
			{
				rv = 2;
			}
			else
			{
				rv = 3;
			}
		}
		rv <<= 5;
		byte rateAndHour = (byte)(rv + hour);
		byte[] data = [rateAndHour, minute, second, frame, fractionalFrame];
		Utils.ValidateSMPTEOffset(data);
		return new MetaMessage(MetaMessageType.SMPTEOffset, data);
	}
	/// <summary>
	/// Reads and outputs the SMPTE Offset values of the <see cref="MetaMessage"/>
	/// </summary>
	/// <param name="rateAndHour">The output value of the rate shifted by 6, plus the number of hours</param>
	/// <param name="minute">The number of minutes to output</param>
	/// <param name="second">The number of seconds to output</param>
	/// <param name="frame">The number of frames per second to output</param>
	/// <param name="fractionalFrame">The number of fractional frames to output</param>
	/// <exception cref="InvalidDataException">If the type isn't a SMPTE Output Meta Message type</exception>
	public void ReadSMPTEOffsetMessage(out byte rateAndHour, out byte minute, out byte second, out byte frame, out byte fractionalFrame)
	{
		if (Type is not MetaMessageType.SMPTEOffset)
		{
			throw new InvalidDataException("This Meta Message is not a SMPTE Offset Meta Message");
		}
		Utils.ValidateSMPTEOffset(Data);
		rateAndHour = Data[0];
		minute = Data[1];
		second = Data[2];
		frame = Data[3];
		fractionalFrame = Data[4];
	}
	/// <summary>
	/// Reads and outputs the SMPTE Offset values of the <see cref="MetaMessage"/>, plus splitting the rate and hour byte
	/// </summary>
	/// <param name="rate">The frame rate value, can either be a 0 (24 fps), 1 (25 fps), 2 (29.97 fps) or 3 (30 fps)</param>
	/// <param name="hour">The number of hours to output</param>
	/// <param name="minute">The number of minutes to output</param>
	/// <param name="second">The number of seconds to output</param>
	/// <param name="frame">The number of frames per second to output</param>
	/// <param name="fractionalFrame">The number of fractional frames to output</param>
	public void ReadSMPTEOffsetMessage(out float rate, out byte hour, out byte minute, out byte second, out byte frame, out byte fractionalFrame)
	{
		ReadSMPTEOffsetMessage(out byte _, out byte m, out byte s, out byte f, out byte ff);
		rate = 0;
		int rateVal = (byte)(Data[0] >> 5);
		switch (rateVal)
		{
			case 0:
				rate = 24;
				break;
			case 1:
				rate = 25;
				break;
			case 2:
				rate = 29.97f;
				break;
			case 3:
				rate = 30;
				break;
		}
		hour = (byte)(Data[0] << 3);
		hour >>= 3;
		minute = m;
		second = s;
		frame = f;
		fractionalFrame = ff;
	}
}
