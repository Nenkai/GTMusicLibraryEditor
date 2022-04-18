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
    /// Interaction logic for BGMLTrackEditWindow.xaml
    /// </summary>
    public partial class BGMLTrackEditWindow : Window
    {
        private BGMLTrack _track;
        public bool Saved { get; set; }
        public BGMLTrackEditWindow(BGMLTrack track)
        {
            _track = track;
            InitializeComponent();
            this.DataContext = track;

            cb_TrackFormatType.SelectedIndex = (int)track.Format;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            _track.Label = tb_Label.Text;
            _track.FileName = tb_FileName.Text;
            _track.Artist = tb_Artist.Text;
            _track.TrackName = tb_TrackName.Text;
            _track.Genre = tb_Genre.Text;
            _track.Format = (LibraryTrackFormat)cb_TrackFormatType.SelectedIndex;

            Saved = true;
            Close();
        }
    }
}
