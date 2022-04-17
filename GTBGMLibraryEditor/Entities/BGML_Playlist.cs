using System.Collections.Generic;
using System.Linq;

namespace GTBGMLibraryEditor.Entities
{
    public class BGML_Playlist
    {
        public string Name { get; set; }
        public List<int> TrackIndexes { get; set; } = new List<int>();
        public string TrackIndexesNames
        {
            get
            {
                if (TrackIndexes.Count == 0)
                    return $"<WARNING> : No tracks in playlist!";
                else if (TrackIndexes.Count == 1)
                    return "1 track";
                else
                    return $"{TrackIndexes.Count} tracks";

            }
        }

        public void AddTrack(BGMLTrack track)
            => TrackIndexes.Add(track.Index);

        public void RemoveTrack(BGMLTrack track)
            => TrackIndexes.Remove(track.Index);
    }
}
