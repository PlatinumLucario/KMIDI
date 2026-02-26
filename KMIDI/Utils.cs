using Kermalis.EndianBinaryIO;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Kermalis.MIDI;

internal static class Utils
{
	// Checks if it's a Power Of Two
	public static bool IsPowerOfTwo(int value)
	{
		return (value & (value - 1)) == 0;
	}

	// Reads the Variable Length
	public static int ReadVariableLength(EndianBinaryReader r)
	{
		// 28 bits allowed
		// (varlen)0x7F_FF_FF_FF represents (uint)0x0F_FF_FF_FF

		int value = r.ReadByte();
		int numBytesRead = 1;

		if ((value & 0x80) != 0)
		{
			value &= 0x7F;

			while (true)
			{
				if (numBytesRead >= 4)
				{
					throw new InvalidDataException("Variable length value was more than 28 bits");
				}

				byte curByte = r.ReadByte();
				numBytesRead++;

				value = (value << 7) + (curByte & 0x7F);
				if ((curByte & 0x80) == 0)
				{
					break;
				}
			}
		}

		return value;
	}
	// Writes a Variable Length
	public static void WriteVariableLength(EndianBinaryWriter w, int value)
	{
		ValidateVariableLengthValue(value);

		int buffer = value & 0x7F;
		while ((value >>= 7) > 0)
		{
			buffer <<= 8;
			buffer |= 0x80;
			buffer += value & 0x7F;
		}
		while (true)
		{
			w.WriteByte((byte)buffer);
			if ((buffer & 0x80) == 0)
			{
				break;
			}
			buffer >>= 8;
		}
	}

	// Checks if the MIDI Channel is valid
	public static void ValidateMIDIChannel(byte channel)
	{
		if (channel > 15)
		{
			throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
		}
	}
	// Checks if the SMPTE Offset values are valid
	public static void ValidateSMPTEOffset(byte[] data)
	{
		int fpsCap = 30;
		byte hourBits = (byte)(data[0] << 3);
		hourBits >>= 3;
		int frameBits = data[0] >> 5;
		switch (frameBits)
		{
			case 0:
				fpsCap = 24;
				break;
			case 1:
				fpsCap = 25;
				break;
			case 2:
				fpsCap = 29;
				break;
			case 3:
				fpsCap = 30;
				break;
		}

		if (hourBits > 23)
		{
			throw new InvalidDataException($"The hour bits cannot be more than 23, the value specified was {hourBits}");
		}
		if (data[1] > 59)
		{
			throw new InvalidDataException($"The minute value cannot be more than 59, the value specified was {data[1]}");
		}
		if (data[2] > 59)
		{
			throw new InvalidDataException($"The second value cannot be more than 59, the value specified was {data[2]}");
		}
		if (data[3] > fpsCap)
		{
			throw new InvalidDataException($"The frame rate value cannot be more than the frames per second cap value ({fpsCap}), the value specified was {data[3]}");
		}
		if (data[4] > 99)
		{
			throw new InvalidDataException($"The fractional frame value cannot be more than 99, the value specified was {data[4]}");
		}
	}
	// Checks if the Variable Length Value is valid or not
	public static bool IsValidVariableLengthValue(int value)
	{
		return value is >= 0 and <= 0x0FFFFFFF; // Section 1.1
	}
	// Checks if the Variable Length Value is valid, throws an exception if it's invalid
	private static void ValidateVariableLengthValue(int value)
	{
		if (!IsValidVariableLengthValue(value))
		{
			throw new ArgumentOutOfRangeException(nameof(value), value, null);
		}
	}
	// Throws an InvalidDataException if the Message Data is invalid
	[DoesNotReturn]
	public static void ThrowInvalidMessageDataException(string msgType, string msgParam, long pos, object value)
	{
		throw new InvalidDataException(string.Format("Invalid {0} {1} at 0x{2:X} ({3})", msgType, msgParam, pos, value));
	}
}
