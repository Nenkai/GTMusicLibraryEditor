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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

using GTMusicInfEditor.Entities;

namespace GTMusicInfEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MusicInf Library { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenLib_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "Gran Turismo 4 BGM Library (*.inf)|*.inf";
            openDialog.CheckFileExists = true;
            openDialog.CheckPathExists = true;

            if (openDialog.ShowDialog() == true)
            {
                MusicInf bgml;
                try
                {
                    bgml = MusicInf.ReadFromFile(openDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error occured while loading file: {ex.Message}", "A not so friendly prompt", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                Library = bgml;

                UpdatePlaylistsList();

                menuItem_Save.IsEnabled = true;
            }
        }

        private void SaveLib_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Gran Turismo 4 BGM Library (*.inf)|*.inf";

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    Library.Save(saveDialog.FileName);
                    MessageBox.Show($"Library file saved successfuly as {saveDialog.FileName} ({Library.Playlists.Count} playlist(s))", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error occured while saving file: {ex.Message}", "A not so friendly prompt", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void lvContextTrack_AddNew(object sender, RoutedEventArgs e)
        {
            if (Library is null || lvTracks.SelectedIndex == -1)
                return;

            var entry = new Track();
            var dialog = new TrackEditWindow(entry, isSequence: Library.Type == MusicInf.PlaylistType.SEQ);
            dialog.ShowDialog();
            if (dialog.Saved)
            {
                var currentPlaylist = ((Track)lvTracks.SelectedItem).ParentPlaylist;
                currentPlaylist.AddTrack(entry);
                entry.ParentPlaylist = currentPlaylist;
                UpdatePlaylistsList();
            }
        }

        private void lvContextTrack_Edit(object sender, RoutedEventArgs e)
        {
            if (Library is null || lvTracks.SelectedIndex == -1)
                return;

            var dialog = new TrackEditWindow((Track)lvTracks.SelectedItem);
            dialog.ShowDialog();
        }

        private void lvContextTrack_Remove(object sender, RoutedEventArgs e)
        {
            if (Library is null || lvTracks.SelectedIndex == -1)
                return;

            var track = (Track)lvTracks.SelectedItem;
            var result = MessageBox.Show($"Are you sure that you want to remove \"{track.TrackName}\"?", "A friendly prompt", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                track.ParentPlaylist.RemoveTrack(track);
                UpdatePlaylistsList();
            }
        }

        private void lvContextTrack_MoveUp(object sender, RoutedEventArgs e)
        {
            if (Library is null || lvTracks.SelectedIndex == -1)
                return;

            var trackSelected = (Track)lvTracks.SelectedItem;
            Playlist parentPlaylist = trackSelected.ParentPlaylist;
            int trackIndex = parentPlaylist.Tracks.IndexOf(trackSelected);
            parentPlaylist.Tracks.Remove(trackSelected);
            parentPlaylist.Tracks.Insert(trackIndex - 1, trackSelected);
            UpdatePlaylistsList();
        }

        private void lvContextTrack_MoveDown(object sender, RoutedEventArgs e)
        {
            if (Library is null || lvTracks.SelectedIndex == -1)
                return;

            var trackSelected = (Track)lvTracks.SelectedItem;
            Playlist parentPlaylist = trackSelected.ParentPlaylist;
            int trackIndex = parentPlaylist.Tracks.IndexOf(trackSelected);
            parentPlaylist.Tracks.Remove(trackSelected);
            parentPlaylist.Tracks.Insert(trackIndex + 1, trackSelected);
            UpdatePlaylistsList();
        }

        private void lvTracks_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (Library.Type == MusicInf.PlaylistType.SEQ)
            {
                var trackSelected = (Track)lvTracks.SelectedItem;
                Playlist parentPlaylist = trackSelected.ParentPlaylist;
                int trackIndex = parentPlaylist.Tracks.IndexOf(trackSelected);

                lvContextTrack_MoveUpItem.IsEnabled = parentPlaylist.Tracks.Count > 1 && trackIndex != 0;
                lvContextTrack_MoveDownItem.IsEnabled = trackIndex < parentPlaylist.Tracks.Count - 1;
            }
            else
            {
                lvContextTrack_MoveUpItem.IsEnabled = false;
                lvContextTrack_MoveDownItem.IsEnabled = false;
            }
        }

        public void UpdatePlaylistsList()
        {
            var trackList = new List<Track>();
            foreach (var playlist in Library.Playlists)
                trackList.AddRange(playlist.Tracks);
            lvTracks.ItemsSource = trackList;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvTracks.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("ParentPlaylist");
            view.GroupDescriptions.Add(groupDescription);

            lvTracks.Items.Refresh();
        }

        private void lvSFX_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            lvContextTrack_Edit(sender, e);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Credits:\n" +
                "- Nenkai#9075 - GT BGM Library Editor Tool Creator & Research\n" +
                "- TheAdmiester - Initial Research for the file format", "About", MessageBoxButton.OK);
        }
    }
}
