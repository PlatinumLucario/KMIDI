using Kermalis.EndianBinaryIO;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Kermalis.MIDI;

public sealed partial class MetaMessage : MIDIMessage
{
	/// <summary>
	/// The variable length of the MetaMessage, which is the size length of the MetaMessage's contents
	/// </summary>
	public int VariableLength { get; }
	/// <summary>
	/// The raw data contained in the MetaMessage
	/// </summary>
	public byte[] Data { get; }
	/// <summary>
	/// If this MetaMessage is flagged as invalid, it will be set to <b>true</b>
	/// </summary>
	internal override bool IsInvalid { get; set; }

	/// <summary>
	/// The type of MetaMessage
	/// </summary>
	public MetaMessageType Type { get; }

	internal MetaMessage(EndianBinaryReader r, bool isInvalid)
	{
		IsInvalid = isInvalid;
		long startPos = r.Stream.Position;

		Type = r.ReadEnum<MetaMessageType>();
		if (Type >= MetaMessageType.MAX)
		{
			Utils.ThrowInvalidMessageDataException(nameof(MetaMessage), nameof(Type), startPos, Type);
		}
		int expectedLen = GetExpectedLength(Type);

		VariableLength = Utils.ReadVariableLength(r);
		if (expectedLen != -1 && expectedLen != VariableLength)
		{
			throw new InvalidDataException($"{nameof(MetaMessage)} at 0x{startPos:X} had an invalid length for {Type}: {VariableLength}. Expected {expectedLen}");
		}

		if (VariableLength == 0)
		{
			Data = Array.Empty<byte>();
		}
		else
		{
			Data = new byte[VariableLength];
			r.ReadBytes(Data);
		}
	}

	/// <summary>
	/// Creates a new MetaMessage using raw bytes
	/// </summary>
	/// <param name="type">The type of MetaMessage</param>
	/// <param name="data">Data in raw bytes</param>
	/// <exception cref="ArgumentOutOfRangeException">Occurs if <see cref="MetaMessageType"/> value is more than 128</exception>
	/// <exception cref="ArgumentException">Occurs if the arguments are invalid</exception>
	public MetaMessage(MetaMessageType type, byte[] data)
	{
		if (type >= MetaMessageType.MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(type), type, null);
		}
		int expectedLen = GetExpectedLength(type);

		if (expectedLen == -1)
		{
			if (!Utils.IsValidVariableLengthValue(data.Length))
			{
				throw new ArgumentException($"{nameof(EscapeMessage)} data length must be [0, 0x0FFFFFFF]");
			}
		}
		else if (data.Length != expectedLen)
		{
			throw new ArgumentException($"{nameof(EscapeMessage)} data length must be {expectedLen} for {type}");
		}

		Type = type;
		Data = data;
	}

	internal override byte GetCMDByte()
	{
		return 0xFF;
	}

	internal override void Write(EndianBinaryWriter w)
	{
		w.WriteEnum(Type);
		Utils.WriteVariableLength(w, Data.Length);
		w.WriteBytes(Data);
	}

	private static int GetExpectedLength(MetaMessageType type)
	{
		switch (type)
		{
			case MetaMessageType.SequenceNumber: return 2;
			case MetaMessageType.MIDIChannelPrefix: return 1;
			case MetaMessageType.EndOfTrack: return 0;
			case MetaMessageType.Tempo: return 3;
			case MetaMessageType.SMPTEOffset: return 5;
			case MetaMessageType.TimeSignature: return 4;
			case MetaMessageType.KeySignature: return 2;
		}
		return -1; // Section 3 - Not required to support all types
	}

	public override string ToString()
	{
		string? arg;

		switch (Type)
		{
			case MetaMessageType.SequenceNumber:
				{
					ReadSequenceNumberMessage(out ushort sequenceID);
					arg = sequenceID.ToString();
					break;
				}
			case MetaMessageType.Text:
			case MetaMessageType.Copyright:
			case MetaMessageType.TrackName:
			case MetaMessageType.InstrumentName:
			case MetaMessageType.Lyric:
			case MetaMessageType.Marker:
			case MetaMessageType.CuePoint:
			case MetaMessageType.ProgramName:
			case MetaMessageType.DeviceName:
			case MetaMessageType.Reserved_A:
			case MetaMessageType.Reserved_B:
			case MetaMessageType.Reserved_C:
			case MetaMessageType.Reserved_D:
			case MetaMessageType.Reserved_E:
			case MetaMessageType.Reserved_F:
				{
					ReadTextMessage(out string text);
					arg = '\"' + text + '\"';
					break;
				}
			case MetaMessageType.MIDIChannelPrefix:
				{
					ReadMIDIChannelPrefixMessage(out byte channel);
					arg = channel.ToString();
					break;
				}
			case MetaMessageType.EndOfTrack:
				{
					arg = null;
					break;
				}
			case MetaMessageType.Tempo:
				{
					ReadTempoMessage(out uint microsecondsPerQuarterNote, out decimal beatsPerMinute);
					arg = string.Format("MicrosecondsPerQuarterNote: {0} ({1} bpm)", microsecondsPerQuarterNote, beatsPerMinute);
					break;
				}
			case MetaMessageType.SMPTEOffset:
				{
					ReadSMPTEOffsetMessage(out byte hour, out byte minute, out byte second, out byte frame, out byte fractionalFrame);
					arg = string.Format("Hour: {0}, Minute: {1}, Second: {2}, Frame: {3}, FractionalFrame: {4}", hour, minute, second, frame, fractionalFrame);
					break;
				}
			case MetaMessageType.TimeSignature:
				{
					ReadTimeSignatureMessage(out byte numerator, out byte denominator, out byte clocksPerMetronomeClick, out byte num32ndNotesPerQuarterNote);
					arg = string.Format("{0}/{1}, ClocksPerMetronomeClick: {2}, Num32ndNotesPerQuarterNote: {3}", numerator, denominator, clocksPerMetronomeClick, num32ndNotesPerQuarterNote);
					break;
				}
			case MetaMessageType.KeySignature:
				{
					ReadKeySignatureMessage(out KeySignatureSF sf, out KeySignatureMI mi);
					arg = string.Format("{0} {1}", sf, mi);
					break;
				}
			case MetaMessageType.ProprietaryEvent:
				{
					ReadProprietaryEventMessage(out ushort manufacturer, out ReadOnlySpan<byte> msgData);
					arg = string.Format("Manufacturer: {0}, Length: {1}", manufacturer, msgData.Length);
					break;
				}
			default:
				{
					arg = string.Format("Length: {0}", Data.Length);
					break;
				}
		}

		if (arg is null)
		{
			return string.Format("{0} [{1}]", nameof(MetaMessage), Type);
		}
		return string.Format("{0} [{1}: {2}]", nameof(MetaMessage), Type, arg);
	}
}