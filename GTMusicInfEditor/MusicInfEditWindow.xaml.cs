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

using GTMusicInfEditor.Entities;

namespace GTMusicInfEditor
{
    /// <summary>
    /// Interaction logic for TrackEditWindow.xaml
    /// </summary>
    public partial class TrackEditWindow : Window
    {
        private Track _track;
        public bool Saved { get; set; }
        public TrackEditWindow(Track track, bool isSequence = false)
        {
            _track = track;
            InitializeComponent();
            this.DataContext = track;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            _track.Label = tb_Label.Text;
            _track.FileName = tb_FileName.Text;
            _track.Artist = tb_Artist.Text;
            _track.TrackName = tb_TrackName.Text;
            _track.Genre = tb_Genre.Text;

            Saved = true;
            Close();
        }
    }
}
