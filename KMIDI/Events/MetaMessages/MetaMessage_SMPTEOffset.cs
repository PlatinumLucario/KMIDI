using Kermalis.EndianBinaryIO;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Kermalis.MIDI;

public sealed partial class MetaMessage : MIDIMessage
{
	/// <summary>
	/// Creates a new <see cref="MetaMessage"/> as a SMPTE Offset type
	/// </summary>
	/// <param name="hour">The number of hours</param>
	/// <param name="minute">The number of minutes</param>
	/// <param name="second">The number of seconds</param>
	/// <param name="frame">The number of frames per second</param>
	/// <param name="fractionalFrame">The number of fractional frames</param>
	/// <returns>A new <see cref="MetaMessage"/> as a SMPTE Offset type with the specified hours, minutes, seconds, frames and fractional frames</returns>
	public static MetaMessage CreateSMPTEOffsetMessage(byte hour, byte minute, byte second, byte frame, byte fractionalFrame)
	{
		byte[] data = [hour, minute, second, frame, fractionalFrame];
		Utils.ValidateSMPTEOffset(data);
		return new MetaMessage(MetaMessageType.SMPTEOffset, data);
	}
	/// <summary>
	/// Reads and outputs the SMPTE Offset values of the <see cref="MetaMessage"/>
	/// </summary>
	/// <param name="hour">The number of hours to output</param>
	/// <param name="minute">The number of minutes to output</param>
	/// <param name="second">The number of seconds to output</param>
	/// <param name="frame">The number of frames per second to output</param>
	/// <param name="fractionalFrame">The number of fractional frames to output</param>
	/// <exception cref="InvalidDataException">If the type isn't a SMPTE Output Meta Message type</exception>
	public void ReadSMPTEOffsetMessage(out byte hour, out byte minute, out byte second, out byte frame, out byte fractionalFrame)
	{
		if (Type is not MetaMessageType.SMPTEOffset)
		{
			throw new InvalidDataException("This Meta Message is not a SMPTE Offset Meta Message");
		}
		Utils.ValidateSMPTEOffset(Data);
		hour = Data[0];
		minute = Data[1];
		second = Data[2];
		frame = Data[3];
		fractionalFrame = Data[4];
	}
}
