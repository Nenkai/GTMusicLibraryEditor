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

using GTBGMLibraryEditor.Entities;

namespace GTBGMLibraryEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BGML Library { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenLib_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "Gran Turismo BGM Library (*.lib)|*.lib";
            openDialog.CheckFileExists = true;
            openDialog.CheckPathExists = true;

            if (openDialog.ShowDialog() == true)
            {
                BGML bgml;
                try
                {
                    bgml = BGML.ReadFromFile(openDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error occured while loading file: {ex.Message}", "A not so friendly prompt", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                Library = bgml;

                UpdateTrackList();
                UpdatePlaylistsList();

                if (bgml.Format == LibraryTrackFormat.SXDF)
                {
                    MessageBox.Show($"This library uses SXDF headers (GTSP), and contains track metadata in it. It cannot be saved yet.", "Cannot save", MessageBoxButton.OK, MessageBoxImage.Information);
                    menuItem_Save.IsEnabled = false;
                }
                else if (bgml.HasExtraTrackMetadata)
                {
                    MessageBox.Show($"This library contains track metadata for each track. It cannot be saved yet.", "Cannot save", MessageBoxButton.OK, MessageBoxImage.Information);
                    menuItem_Save.IsEnabled = false;
                }
                else
                {
                    menuItem_Save.IsEnabled = true;
                }
            }
        }

        private void SaveLib_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Gran Turismo BGM Library (*.lib)|*.lib";

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    Library.Save(saveDialog.FileName);
                    MessageBox.Show($"Library file saved successfuly as {saveDialog.FileName} ({Library.Tracks.Count} track(s))", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (Library is null)
                return;

            var entry = new BGMLTrack();
            var dialog = new BGMLTrackEditWindow(entry);
            dialog.ShowDialog();
            if (dialog.Saved)
            {
                entry.Index = Library.Tracks.Count;
                Library.Tracks.Add(entry);
                UpdateTrackList();
            }
        }

        private void lvContextTrack_Edit(object sender, RoutedEventArgs e)
        {
            if (Library is null || lvTracks.SelectedIndex == -1)
                return;

            var dialog = new BGMLTrackEditWindow((BGMLTrack)lvTracks.SelectedItem);
            dialog.ShowDialog();
            if (dialog.Saved)
                UpdateTrackList();
        }

        private void lvContextTrack_Remove(object sender, RoutedEventArgs e)
        {
            if (Library is null || lvTracks.SelectedIndex == -1)
                return;

            var track = (BGMLTrack)lvTracks.SelectedItem;
            var result = MessageBox.Show($"Are you sure that you want to remove \"{track.TrackName}\"?", "A friendly prompt", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Library.RemoveTrack(track);
                UpdateTrackList();
                UpdatePlaylistsList();
            }
        }

        private void lvContextPlaylist_AddNew(object sender, RoutedEventArgs e)
        {
            if (Library is null)
                return;

            var entry = new BGML_Playlist();
            entry.Name = $"playlist_{Library.Playlists.Count}";

            var dialog = new PlaylistEditWindow(Library, entry);
            dialog.Edited = true;
            dialog.ShowDialog();
            if (dialog.Edited)
            {
                Library.Playlists.Add(entry);
                UpdatePlaylistsList();

                lvPlaylists.SelectedItem = entry;
                lvPlaylists.ScrollIntoView(lvPlaylists.SelectedItem);
            }
        }

        private void lvContextPlaylist_Edit(object sender, RoutedEventArgs e)
        {
            if (Library is null || lvPlaylists.SelectedIndex == -1)
                return;

            var dialog = new PlaylistEditWindow(Library, (BGML_Playlist)lvPlaylists.SelectedItem);
            dialog.ShowDialog();
            if (dialog.Edited)
                UpdatePlaylistsList();
        }

        private void lvContextPlaylist_Remove(object sender, RoutedEventArgs e)
        {
            if (Library is null || lvPlaylists.SelectedIndex == -1)
                return;

            var playlist = (BGML_Playlist)lvPlaylists.SelectedItem;
            var result = MessageBox.Show($"Are you sure that you want to remove playlist \"{playlist.Name}\"?", "A friendly prompt", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Library.RemovePlaylist(playlist);
                UpdateTrackList();
                UpdatePlaylistsList();
            }
        }

        public void UpdateTrackList()
        {
            lvTracks.ItemsSource = Library.Tracks;
            lvTracks.Items.Refresh();
        }

        public void UpdatePlaylistsList()
        {
            lvPlaylists.ItemsSource = Library.Playlists;
            lvPlaylists.Items.Refresh();
        }

        private void lvPlaylists_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Library is null || lvPlaylists.SelectedIndex == -1)
                return;

            var dialog = new PlaylistEditWindow(Library, (BGML_Playlist)lvPlaylists.SelectedItem);
            dialog.ShowDialog();
            if (dialog.Edited)
                UpdatePlaylistsList();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Credits:\n" +
                "- Nenkai#9075 - GT BGM Library Editor Tool Creator & Research\n" +
                "- TheAdmiester - Initial Research for the file format", "About", MessageBoxButton.OK);
        }
    }
}
