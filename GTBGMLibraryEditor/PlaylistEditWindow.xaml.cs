using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using GTBGMLibraryEditor.Entities;

namespace GTBGMLibraryEditor
{
    /// <summary>
    /// Interaction logic for PlaylistEditWindow.xaml
    /// </summary>
    public partial class PlaylistEditWindow : Window
    {
        private BGML_Playlist _playlist;
        private BGML _library;

        private List<BGMLTrack> _selectableTracks { get; set; } = new List<BGMLTrack>();
        public bool Edited { get; set; }
        public PlaylistEditWindow(BGML library, BGML_Playlist playlist)
        {
            InitializeComponent();
            _library = library;
            _playlist = playlist;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PopulatePlaylistTracks();
            PopulateSelectableTrackList();

            Title = $"Editing Playlist \"{_playlist.Name}\"";
        }

        public void PopulatePlaylistTracks()
        {
            lv_PlaylistTracks.ItemsSource = null;
            var playlistTracks = new List<BGMLTrack>(); 
            foreach (var trackIndex in _playlist.TrackIndexes)
                playlistTracks.Add(_library.Tracks[trackIndex]);
            lv_PlaylistTracks.ItemsSource = playlistTracks;
        }

        public void PopulateSelectableTrackList()
        {
            cb_TrackPicker.Items.Clear();
            _selectableTracks.Clear();
            foreach (var track in _library.Tracks)
            {
                if (!_playlist.TrackIndexes.Contains(track.Index))
                {
                    cb_TrackPicker.Items.Add($"{track.TrackName} ({track.Label})");
                    _selectableTracks.Add(track);
                }
            }
        }

        private void btn_AddTrack_Click(object sender, RoutedEventArgs e)
        {
            if (cb_TrackPicker.SelectedIndex == -1)
                return;

            var selectedTrack = _selectableTracks[cb_TrackPicker.SelectedIndex];
            _playlist.AddTrack(selectedTrack);

            PopulatePlaylistTracks();
            PopulateSelectableTrackList();

            Edited = true;
        }

        public void lvContextTrackPlaylist_Remove(object sender, RoutedEventArgs e)
        {
            if (lv_PlaylistTracks.SelectedIndex == -1)
                return;

            var track = (BGMLTrack)lv_PlaylistTracks.SelectedItem;
            _playlist.RemoveTrack(track);

            PopulatePlaylistTracks();
            PopulateSelectableTrackList();

            Edited = true;
        }
    }
}
