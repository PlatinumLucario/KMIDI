using Kermalis.EndianBinaryIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Kermalis.MIDI;

// https://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html
// http://www.somascape.org/midi/tech/mfile.html
// TODO: Is a second headerchunk valid?
public sealed class MIDIFile
{
	/// <summary>
	/// The header chunk of the MIDI file
	/// </summary>
	public MIDIHeaderChunk HeaderChunk { get; }
	/// <summary>
	/// This contains the MIDI file stream data, which can be used for working with the MIDI file data manually<br></br>
	/// <br></br>
	/// Note: If a new MIDI file is being made, this property will remain <b>null</b> until the "Save()" method is used
	/// </summary>
	public Stream? Stream { get; private set; }
	/// <summary>
	/// If the MIDI has errors, this will be set to <b>true</b>
	/// </summary>
	public bool HasErrors { get; internal set; }
	internal static bool SkipErrors;
	private readonly List<MIDIChunk> _nonHeaderChunks;
	/// <summary>
	/// Creates a new MIDI file
	/// </summary>
	/// <param name="format">The type of MIDI format to use</param>
	/// <param name="timeDivision">The Time Division Value of the MIDI</param>
	/// <param name="tracksInitialCapacity">The amount of tracks that the MIDI can carry</param>
	/// <exception cref="ArgumentException">Occurs when MIDI Format0 doesn't have 1 track</exception>
	public MIDIFile(MIDIFormat format, TimeDivisionValue timeDivision, int tracksInitialCapacity)
	{
		if (format == MIDIFormat.Format0 && tracksInitialCapacity != 1)
		{
			throw new ArgumentException("Format 0 must have 1 track", nameof(tracksInitialCapacity));
		}

		HeaderChunk = new MIDIHeaderChunk(format, timeDivision); // timeDivision validated here
		_nonHeaderChunks = new List<MIDIChunk>(tracksInitialCapacity);
	}
	/// <summary>
	/// Opens an existing MIDI data stream
	/// </summary>
	/// <param name="stream">The MIDI data stream that will be used</param>
	/// <param name="skipErrors">Stops if there's an error if set to <b>false</b>, proceeds and skips non-critical errors if set to <b>true</b></param>
	/// <exception cref="InvalidDataException">Thrown when the MIDI data stream is invalid</exception>
	public MIDIFile(Stream stream, bool skipErrors = false)
	{
		SkipErrors = skipErrors;
		Stream = stream;
		var r = new EndianBinaryReader(stream, endianness: Endianness.BigEndian, ascii: true);
		string chunkName = r.ReadString_Count(4);
		if (chunkName != MIDIHeaderChunk.EXPECTED_NAME)
		{
			HasErrors = true;
			InvalidDataException ex = new("MIDI header was not at the start of the file");
			if (!SkipErrors)
			{
				throw ex;
			}
			else
			{
				Debug.WriteLine(ex.Message);
				Debug.WriteLine("Attempting to find MIDI header in the entire file");
				while (chunkName != MIDIHeaderChunk.EXPECTED_NAME && stream.Position < stream.Length)
				{
					long pos = r.Stream.Position;
					chunkName = r.ReadString_Count(4);
					r.Stream.Position = pos;
					if (stream.Position < stream.Length - 1)
					{
						r.Stream.Position++;
					}
				}
				Debug.WriteLine($"Found MIDI header at offset 0x{stream.Position:X}");
			}
		}

		HeaderChunk = (MIDIHeaderChunk)ReadChunk(r, alreadyReadName: chunkName);
		_nonHeaderChunks = new List<MIDIChunk>(HeaderChunk.NumTracks);

		while (stream.Position < stream.Length)
		{
			MIDIChunk c = ReadChunk(r);
			_nonHeaderChunks.Add(c);
		}

		int trackCount = CountTrackChunks();
		if (trackCount != HeaderChunk.NumTracks)
		{
			HasErrors = true;
			InvalidDataException ex = new($"Unexpected track count: (Expected {HeaderChunk.NumTracks} but found {trackCount}");
			if (!SkipErrors)
			{
				throw ex;
			}
			else
			{
				Debug.WriteLine(ex.Message);
			}
		}

		// Check each track for errors
		foreach (var trackChunk in EnumerateTrackChunks())
		{
			if (trackChunk.HasErrors)
			{
				HasErrors = true;
			}
		}

		if (HasErrors)
		{
			Debug.WriteLine("This MIDI contains errors, it might not work correctly in some applications");
		}
	}

	private static MIDIChunk ReadChunk(EndianBinaryReader r, string? alreadyReadName = null)
	{
		string chunkName = alreadyReadName ?? r.ReadString_Count(4);
		uint chunkSize = r.ReadUInt32();
		switch (chunkName)
		{
			case MIDIHeaderChunk.EXPECTED_NAME: return new MIDIHeaderChunk(chunkSize, r);
			case MIDITrackChunk.EXPECTED_NAME: return new MIDITrackChunk(chunkSize, r);
			default: return new MIDIUnsupportedChunk(chunkName, chunkSize, r);
		}
	}
	/// <summary>
	/// Adds a chunk to the MIDI file
	/// </summary>
	/// <param name="c">The MIDI chunk to add</param>
	public void AddChunk(MIDIChunk c)
	{
		_nonHeaderChunks.Add(c);
		if (c is MIDITrackChunk)
		{
			HeaderChunk.NumTracks++;
		}
	}
	/// <summary>
	/// Removes a chunk from the MIDI file
	/// </summary>
	/// <param name="c">The MIDI chunk to remove</param>
	/// <returns><b>true</b> if successful, otherwise <b>false</b></returns>
	public bool RemoveChunk(MIDIChunk c)
	{
		bool success = _nonHeaderChunks.Remove(c);
		if (success && c is MIDITrackChunk)
		{
			HeaderChunk.NumTracks--;
		}
		return success;
	}
	/// <summary>
	/// Enumerates the chunks in the MIDI file, to make it easier to read
	/// </summary>
	/// <param name="includeHeaderChunk">If this is set to <b>true</b>, it will include the header chunk as well</param>
	/// <returns></returns>
	public IEnumerable<MIDIChunk> EnumerateChunks(bool includeHeaderChunk)
	{
		if (includeHeaderChunk)
		{
			yield return HeaderChunk;
		}
		foreach (MIDIChunk c in _nonHeaderChunks)
		{
			yield return c;
		}
	}
	/// <summary>
	/// Enumerates all the track chunks, to read each track chunk more easily
	/// </summary>
	/// <returns>The track chunks as an IEnumerable</returns>
	public IEnumerable<MIDITrackChunk> EnumerateTrackChunks()
	{
		foreach (MIDIChunk c in _nonHeaderChunks)
		{
			if (c is MIDITrackChunk tc)
			{
				yield return tc;
			}
		}
	}
	/// <summary>
	/// Counts the total number of tracks in the MIDI file
	/// </summary>
	/// <returns>The number of MIDI tracks</returns>
	public int CountTrackChunks()
	{
		int count = 0;
		foreach (MIDIChunk c in _nonHeaderChunks)
		{
			if (c is MIDITrackChunk tc)
			{
				count++;
			}
		}
		return count;
	}
	public void SetNonHeaderChunks(IEnumerable<MIDIChunk> nonHeaderChunks)
	{
		_nonHeaderChunks.Clear();
		_nonHeaderChunks.AddRange(nonHeaderChunks);
	}
	/// <summary>
	/// Saves the MIDI file stream
	/// </summary>
	/// <param name="stream">The MIDI file stream</param>
	public void Save(Stream stream)
	{
		var w = new EndianBinaryWriter(stream, endianness: Endianness.BigEndian, ascii: true);

		HeaderChunk.Write(w);

		foreach (MIDIChunk c in _nonHeaderChunks)
		{
			c.Write(w);
		}

		Stream = stream;
	}

	/// <summary>
	/// Don't put this as "ToString()" since it can be slow
	/// </summary>
	/// <returns>A string of the MIDI data hierarchy</returns>
	public string GetHierarchy()
	{
		var str = new StringBuilder();
		foreach (MIDIChunk c in EnumerateChunks(true))
		{
			str.AppendLine(c.ToString());
		}
		return str.ToString();
	}
}
