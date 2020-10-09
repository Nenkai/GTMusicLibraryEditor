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

        public LibraryTrackFormat Format { get; set; }
    }

    public enum LibraryTrackFormat
    {
        ATRAC3 = 0,
        SGB = 2,
        SXDF = 259,
    }
}
