using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Syroot.BinaryData;

namespace GTMusicInfEditor.Utils
{
    /// <summary>
    /// Represents a binary string table that Polyphony Digital uses to save space within files.
    /// </summary>
    public class OptimizedStringTable
    {
        /// <summary>
        /// Position within the string table
        /// </summary>
        private int _currentPos;
        public Dictionary<string, int> StringMeta = new Dictionary<string, int>();

        /// <summary>
        /// Adds a string to the string table.
        /// </summary>
        public void AddString(string str)
        {
            if (!StringMeta.ContainsKey(str))
            {
                StringMeta.Add(str, _currentPos);
                _currentPos += Encoding.UTF8.GetByteCount(str) + 1; // Null-terminated
            }
        }

        /// <summary>
        /// Saves the string table into a main stream.
        /// This updates the underlaying table holding the offsets to match the main stream.
        /// </summary>
        public void SaveStream(BinaryStream bs)
        {
            int basePos = (int)bs.Position;
            foreach (var strEntry in StringMeta)
                bs.WriteString(strEntry.Key, StringCoding.ZeroTerminated, Encoding.UTF8);

            // Update the offsets - kinda inefficient way to do it
            foreach (var strEntry in StringMeta.Keys.ToList())
                StringMeta[strEntry] += basePos;
        }

        /// <summary>
        /// Gets the offset of a string within the binary table.
        /// Save stream should've already been called.
        /// </summary>
        public int GetStringOffset(string str)
        {
            return StringMeta[str];
        }
    }
}
