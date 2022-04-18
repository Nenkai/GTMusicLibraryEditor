using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTBGMLibraryEditor.Entities
{
    public class BGMLTrack
    {
        public int Index { get; set; }

        public string Label { get; set; }
        public string FileName { get; set; }
        public string TrackName { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }

        public LibraryTrackFormat Format { get; set; } = LibraryTrackFormat.SGX;
        public bool HasHeader { get; set; } = false;

    }

    // GT6 1.22 EU - FUN_0091a9e8
    public enum LibraryTrackFormat
    {
        ATRAC3PLUS = 0,
        MP3 = 1,
        SGX = 2, // sgb/sgh
        SNDX = 3,
    }
}
