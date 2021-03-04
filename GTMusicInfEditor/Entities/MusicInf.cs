using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Syroot.BinaryData;
using GTMusicInfEditor.Utils;

namespace GTMusicInfEditor.Entities
{
    public class MusicInf
    {
        public List<Playlist> Playlists { get; private set; }

        public const string MAGIC_SEQ = "MSEQ";
        public const string MAGIC_ADS = "MADS";

        public PlaylistType Type { get; set; }

        public string Magic { get; set; }

        public static readonly string[] ADSPlaylistNames = new[]
        {
            "start",
            "goal",
            "racing",
            "default_quick",
            "result",
            "torophy_solo",
            "torophy_champ",
            "get_car",
            "replay_theater",
            "license_start",
            "license_goal",
            "license_gold",
            "license_silver",
            "license_bronze",
            "license_failed",
            "license_torophy",
            "autodemo",
            "champ_start",
            "champ_goal",
            "timeattack_start",
            "timeattack_goal",
            "coffee_start",
            "coffee_goal",
            "etc_start",
            "etc_goal",
            "last_start",
            "last_goal",
            "challenge_win",
            "license_quick",
        };

        public static readonly string[] SEQPlaylistNames = new[]
        {
            "home",
            "powerspeed",
            "race",
            "hall",
            "license",
            "mission",
            "gtauto",
            "arcade",
            "map",
            "replaytheater",
        };

        public static MusicInf ReadFromFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            using (var br = new BinaryStream(fs))
            {
                var musicInf = new MusicInf();
                musicInf.Magic = br.ReadString(4);

                if (musicInf.Magic == MAGIC_ADS)
                    musicInf.Type = PlaylistType.ADS;
                else if (musicInf.Magic == MAGIC_SEQ)
                    musicInf.Type = PlaylistType.SEQ;
                else
                    throw new Exception($"Invalid file magic (Need {MAGIC_ADS}, got {musicInf.Magic})");

                br.Position = 0x0C;

                int playlistCount = br.ReadInt32();
                musicInf.Playlists = new List<Playlist>(playlistCount);
                int playlistTreeOffset = br.ReadInt32();

                // Read Playlist

                string[] playlistNames;
                if (musicInf.Type == PlaylistType.ADS)
                    playlistNames = ADSPlaylistNames;
                else
                    playlistNames = SEQPlaylistNames;

                for (int i = 0; i < playlistCount; i++)
                {
                    Playlist playlist = new Playlist();
                    playlist.Name = playlistNames[i];
                    br.Position = 0x14 + (i * 0x08);
                    int dataOffset = br.ReadInt32();
                    int trackCount = br.ReadInt32();
                    
                    for (int j = 0; j < trackCount; j++)
                    {
                        var track = new Track();
                        track.ParentPlaylist = playlist;
                        br.Position = dataOffset + (j * 0x18);
                        int rawNameOffset = br.ReadInt32();
                        int fileNameOffset = br.ReadInt32();
                        int trackNameOffset = br.ReadInt32();
                        int artistNameOffset = br.ReadInt32();
                        int genreNameoffset = br.ReadInt32();
                        track.Index = br.ReadInt32();

                        if (rawNameOffset != 0)
                        {
                            br.Position = rawNameOffset;
                            track.Label = br.ReadString(StringCoding.ZeroTerminated);
                        }

                        if (fileNameOffset != 0)
                        {
                            br.Position = fileNameOffset;
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

                        playlist.AddTrack(track);
                    }

                    musicInf.Playlists.Add(playlist);
                }

                return musicInf;
            }
        }

        public int GetTotalTrackCount()
        {
            int i = 0;
            foreach (var playlist in Playlists)
                i += playlist.Tracks.Count;
            return i;
        }

        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            using (var br = new BinaryStream(fs))
            {
                br.WriteString(Magic, StringCoding.Raw);
                br.WriteInt32(2);
                br.Position += 4; // Write File Size Later
                br.WriteInt32(Playlists.Count);

                int totalTracks = GetTotalTrackCount();
                br.WriteInt32(totalTracks);

                br.Position = 0x14;

                // Step 1: Skip all way to the string table location so we can have proper string offsets

                // - Move to the Playlist Tree for now
                br.Position += 0x08 * Playlists.Count;

                int trackOffsetMap = (int)br.Position;

                // - Go to the end of the Playlist Tree
                br.Position += totalTracks * 0x18;

                // Step 2: We reached the string table location, write the string tables
                OptimizedStringTable trackStringTable = SerializeTrackStringTable();
                trackStringTable.SaveStream(br);

                // Step 3: Populate the trees that reference the string tables
                int lastTrackDataPos = trackOffsetMap;

                for (int i = 0; i < Playlists.Count; i++)
                {
                    for (int j = 0; j < Playlists[i].Tracks.Count; j++)
                    {
                        br.Position = lastTrackDataPos + (j * 0x18);

                        var track = Playlists[i].Tracks[j];
                        br.WriteInt32(trackStringTable.GetStringOffset(track.Label));
                        br.WriteInt32(trackStringTable.GetStringOffset(track.FileName));
                        br.WriteInt32(trackStringTable.GetStringOffset(track.TrackName));
                        br.WriteInt32(trackStringTable.GetStringOffset(track.Artist));
                        br.WriteInt32(trackStringTable.GetStringOffset(track.Genre));
                        if (Type == PlaylistType.SEQ)
                            br.WriteInt32(j);
                    }

                    br.Position = 0x14 + (0x08 * i);
                    br.WriteInt32(lastTrackDataPos);
                    br.WriteInt32(Playlists[i].Tracks.Count);

                    lastTrackDataPos += 0x18 * Playlists[i].Tracks.Count;
                }
            }
        }

        public OptimizedStringTable SerializeTrackStringTable()
        {
            var trackStrTable = new OptimizedStringTable();
            foreach (var playlist in Playlists)
            {
                foreach (var track in playlist.Tracks)
                {
                    trackStrTable.AddString(track.Label);
                    trackStrTable.AddString(track.FileName);
                    trackStrTable.AddString(track.TrackName);
                    trackStrTable.AddString(track.Artist);
                    trackStrTable.AddString(track.Genre);
                }
            }

            return trackStrTable;
        }

        public enum PlaylistType
        {
            ADS,
            SEQ,
        }
    }
}
