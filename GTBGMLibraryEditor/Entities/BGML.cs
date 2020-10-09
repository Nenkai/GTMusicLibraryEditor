using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Syroot.BinaryData;
using GTBGMLibraryEditor.Utils;

namespace GTBGMLibraryEditor.Entities
{
    public class BGML
    {
        public List<BGMLTrack> Tracks { get; private set; }
        public List<BGML_Playlist> Playlists { get; private set; }

        public const string MAGIC = "BGML";

        public void RemoveTrack(BGMLTrack track)
        {
            int id = track.Index;
            Tracks.Remove(track);

            // Resort playlists
            foreach (var playlist in Playlists)
            {
                if (playlist.TrackIndexes.Contains(id))
                    playlist.TrackIndexes.Remove(id);

                for (int i = 0; i < playlist.TrackIndexes.Count; i++)
                {
                    if (playlist.TrackIndexes[i] >= id)
                        playlist.TrackIndexes[i]--;
                }
            }

            // And Tracks
            for (int i = 0; i < Tracks.Count; i++)
                Tracks[i].Index = i;
        }

        public static BGML ReadFromFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            using (var br = new BinaryStream(fs))
            {

                var magic = br.ReadString(4);
                if (magic != MAGIC)
                    throw new Exception($"Invalid file magic (Need {MAGIC}, got {magic})");

                br.BaseStream.Position += 4;
                uint fileSize = br.ReadUInt32();
                if (fileSize != br.Length)
                    throw new Exception($"Invalid file size in file (File says {fileSize}b, but is actually {br.Length}b)");

                br.Position += 4;

                BGML bgml = new BGML();

                int trackCount = br.ReadInt32();
                bgml.Tracks = new List<BGMLTrack>(trackCount);
                int trackTreeOffset = br.ReadInt32();

                int playlistCount = br.ReadInt32();
                bgml.Playlists = new List<BGML_Playlist>(playlistCount);
                int playlistTreeOffset = br.ReadInt32();

                // Read Tracks
                for (int i = 0; i < trackCount; i++)
                {
                    BGMLTrack track = new BGMLTrack();
                    br.Position = trackTreeOffset + (i * 0x30);
                    int fileNameOffset = br.ReadInt32();
                    int format = br.ReadInt32();
                    int idStringOffset = br.ReadInt32();
                    br.Position += 8;
                    int trackNameOffset = br.ReadInt32();
                    int artistNameOffset = br.ReadInt32();
                    int genreNameoffset = br.ReadInt32();

                    if (fileNameOffset != 0)
                    {
                        br.Position = fileNameOffset;
                        track.Label = br.ReadString(StringCoding.ZeroTerminated);
                    }

                    if (idStringOffset != 0)
                    {
                        br.Position = idStringOffset;
                        track.FileName = br.ReadString(StringCoding.ZeroTerminated);
                    }

                    if (trackNameOffset != 0)
                    {
                        br.Position = trackNameOffset;
                        track.TrackName = br.ReadString(StringCoding.ZeroTerminated);
                    }

                    if (artistNameOffset != 0)
                    {
                        br.Position = artistNameOffset;
                        track.Artist = br.ReadString(StringCoding.ZeroTerminated);
                    }

                    if (genreNameoffset != 0)
                    {
                        br.Position = genreNameoffset;
                        track.Genre = br.ReadString(StringCoding.ZeroTerminated);
                    }

                    bgml.Tracks.Add(track);
                    track.Index = i;
                }

                // Read Playlists
                for (int i = 0; i < playlistCount; i++)
                {
                    BGML_Playlist playlist = new BGML_Playlist();
                    br.Position = playlistTreeOffset + (i * 0x04);
                    br.Position = br.ReadInt32();

                    int nameOffset = br.ReadInt32();
                    br.Position += 12;
                    int pTrackCount = br.ReadInt32();

                    List<int> trackIndexes = new List<int>();
                    for (int j = 0; j < pTrackCount; j++)
                        trackIndexes.Add(br.ReadInt32());

                    playlist.TrackIndexes = trackIndexes;

                    br.Position = nameOffset;
                    playlist.Name = br.ReadString(StringCoding.ZeroTerminated, Encoding.UTF8);

                    bgml.Playlists.Add(playlist);
                }

                return bgml;
            }
        }

        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            using (var br = new BinaryStream(fs))
            {
                br.WriteString(BGML.MAGIC, StringCoding.Raw);
                br.Position += 4;
                br.Position += 4; // Write File Size Later
                br.Position += 4;
                br.WriteInt32(Tracks.Count);
                br.WriteInt32(0x30); // We know its always at 0x30.. so just write anyway
                br.WriteInt32(Playlists.Count);
                br.Position += 4; // Offset write later

                br.Position = 0x30;

                // Step 1: Skip all way to the string table location so we can have proper string offsets

                // - Move to the Playlist Tree for now
                br.Position += 0x30 * Tracks.Count;

                // - Go to the end of the Playlist Tree
                br.Position += Playlists.Count * sizeof(uint);

                // - Playlist Tree is aligned
                br.Align(0x10, true);
                

                int[] playlistDataOffsets = new int[Playlists.Count]; // Will be used later on to write the tree
                // - Write Playlist Data along the way
                for (int i = 0; i < Playlists.Count; i++)
                {
                    playlistDataOffsets[i] = (int)br.Position;
                    br.Position += 0x10; // We're skipping the first int which is the string offset
                    br.WriteInt32(Playlists[i].TrackIndexes.Count);
                    foreach (var index in Playlists[i].TrackIndexes)
                        br.WriteInt32(index);
                    br.Align(0x10, true);
                }

                // Step 2: We reached the string table location, write the string tables
                OptimizedStringTable trackStringTable = SerializeTrackStringTable();
                trackStringTable.SaveStream(br);

                OptimizedStringTable playlistStringTable = SerializePlaylistStringTable();
                playlistStringTable.SaveStream(br);
                
                // Step 3: Populate the trees that reference the string tables
                for (int i = 0; i < Tracks.Count; i++)
                {
                    br.Position = 0x30 + (i * 0x30);
                    br.WriteInt32(trackStringTable.GetStringOffset(Tracks[i].Label));
                    br.WriteInt32(2);
                    br.WriteInt32(trackStringTable.GetStringOffset(Tracks[i].FileName));
                    br.Position += 8;
                    br.WriteInt32(trackStringTable.GetStringOffset(Tracks[i].TrackName));
                    br.WriteInt32(trackStringTable.GetStringOffset(Tracks[i].Artist));
                    br.WriteInt32(trackStringTable.GetStringOffset(Tracks[i].Genre));
                    br.Position += 0x10;
                }

                // Write the playlist data pointers
                int playlistTreeOffset = (int)br.Position;
                for (int i = 0; i < playlistDataOffsets.Length; i++)
                    br.WriteInt32(playlistDataOffsets[i]);
                br.Align(0x10, true);

                // Write Playlist Label offsets
                for (int i = 0; i < Playlists.Count; i++)
                {
                    br.Position = playlistDataOffsets[i]; // Using shortcut by using the precalculated offsets from earlier
                    br.WriteInt32(playlistStringTable.GetStringOffset(Playlists[i].Name));
                }

                // Finish up the header
                br.Position = 0x08;
                br.WriteInt32((int)br.Length); // File Size
                br.Position = 0x1C;
                br.WriteInt32(playlistTreeOffset);
            }
        }

        public OptimizedStringTable SerializeTrackStringTable()
        {
            var trackStrTable = new OptimizedStringTable();
            foreach (var track in Tracks)
            {
                trackStrTable.AddString(track.Label);
                trackStrTable.AddString(track.FileName);
                trackStrTable.AddString(track.TrackName);
                trackStrTable.AddString(track.Artist);
                trackStrTable.AddString(track.Genre);
            }

            return trackStrTable;
        }

        public OptimizedStringTable SerializePlaylistStringTable()
        {
            var playlistStrTable = new OptimizedStringTable();
            foreach (var playlist in Playlists)
                playlistStrTable.AddString(playlist.Name);

            return playlistStrTable;
        }
    }
}
