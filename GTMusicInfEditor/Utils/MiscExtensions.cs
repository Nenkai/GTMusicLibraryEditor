using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
namespace GTMusicInfEditor.Utils
{
    public static class MiscExtensions
    {
        public static void AlignStream(this Stream ms, int padSize)
            => ms.Position += ms.Position % padSize;
    }
}
