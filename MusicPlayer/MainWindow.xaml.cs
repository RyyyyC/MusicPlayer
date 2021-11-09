using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MusicPlayer
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaPlayer _MediaPlayer = new MediaPlayer();

        public TimeSpan MediaTotal
        {
            get { if (_MediaPlayer.NaturalDuration.HasTimeSpan) { return _MediaPlayer.NaturalDuration.TimeSpan; } else { return TimeSpan.FromSeconds(0); }; }
        }
        public Duration Duration
        {
            get { return MediaTotal; }
        }
        public bool MediaHasTimeSpan
        {
            get { return _MediaPlayer.NaturalDuration.HasTimeSpan; }
        }
        public TimeSpan MediaPosition
        {
            get { return _MediaPlayer.Position; }
        }
        public DoubleAnimation DoubleAnimationProgessBar
        {
            get { return new DoubleAnimation(MediaPosition.TotalSeconds, MediaTotal.TotalSeconds + MediaPosition.TotalSeconds, Duration); }
        }

        string[] Files;
        int Index;

        List<string> Paths = new List<string>();
        List<string> SongNames = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }
        private void previous_Button(object sender, RoutedEventArgs e)
        {
            Index--;
            if (Index < 0)
            {
                Index = Paths.Count - 1;
                SongsList.SelectedIndex = Index;
            }
            else
                SongsList.SelectedIndex = Index;
        }
        private void skip_Button(object sender, RoutedEventArgs e)
        {
            Index++;
            if (Index == Paths.Count)
            {
                Index = 0;
                SongsList.SelectedIndex = Index;
            }
            else
                SongsList.SelectedIndex = Index;
        }
        private void play_Button(object sender, RoutedEventArgs e)
        {

            if (MediaHasTimeSpan)
            {
                try
                {
                    DoubleAnimation doubleAnimation = new DoubleAnimation(MediaPosition.TotalSeconds, MediaTotal.TotalSeconds, Duration);
                    beginBar(doubleAnimation);
                }
                catch (System.InvalidOperationException)
                {
                    MessageBox.Show("File Error.");
                }
            }
            _MediaPlayer.Play();
        }
        private void pause_Button(object sender, RoutedEventArgs e)
        {
            if (MediaHasTimeSpan)
            {
                try
                {
                    SongProgressBar.BeginAnimation(ProgressBar.ValueProperty, null);

                    SongProgressBar.Maximum = MediaTotal.TotalSeconds;
                    SongProgressBar.SetCurrentValue(ProgressBar.ValueProperty, MediaPosition.TotalSeconds);
                }
                catch (System.InvalidOperationException)
                {
                    MessageBox.Show("File Error.");
                }
            }
            _MediaPlayer.Pause();
        }
        private void choose_File(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Opf = new OpenFileDialog(){Multiselect = true, Filter = "Audio files (*.mp3)|*.mp3" };
            if(Opf.ShowDialog() == true)
            {
                Files = Opf.FileNames;
                foreach (string File in Files)
                {
                    Paths.Add(File);
                    SongNames.Add(File.Substring(File.LastIndexOf(@"\") + 1, File.Length - File.LastIndexOf(@"\") - 1));
                }
                SongsList.ItemsSource = SongNames;
                SongsList.Items.Refresh();
            }
        }

        private void SongsList_Changed(object sender, SelectionChangedEventArgs e)
        {

            Index = SongsList.SelectedIndex;
            if(Index != -1)
            {
                ActualSong.Text = SongNames[SongsList.SelectedIndex];
                _MediaPlayer.Open(new Uri(Paths[Index]));
                _MediaPlayer.MediaOpened += _MediaPlayer_MediaOpened;

            } 
        }

        private void _MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            _MediaPlayer.Stop();
            if (MediaHasTimeSpan)
            {
                _MediaPlayer.Play();
                progressBar_Process();
            }
        }
        
        void progressBar_Process()
        {
            SongProgressBar.Maximum = MediaTotal.TotalSeconds;
            try
            {
                beginBar(DoubleAnimationProgessBar);
                _MediaPlayer.Play();
            }
            catch (System.InvalidOperationException)
            {
                MessageBox.Show("File Error.");
            }
        }
        private void SongProgressBar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SongProgressBar.Maximum = MediaTotal.TotalSeconds;
            try
            {
                _MediaPlayer.Open(new Uri(Path.GetFullPath(Paths[Index])));
            }
            catch (System.ArgumentOutOfRangeException)
            {
                MessageBox.Show("Load file.");
            }
            finally 
            {
                if (MediaHasTimeSpan)
                {
                    _MediaPlayer.Position = TimeSpan.FromSeconds(MediaTotal.TotalSeconds * (e.MouseDevice.GetPosition(sender as UIElement).X * 100 / 540) / 100);
                    beginBar(DoubleAnimationProgessBar);
                }
            }
        }
        void beginBar(DoubleAnimation doubleAnimation)
        {
            SongProgressBar.BeginAnimation(ProgressBar.ValueProperty, null);
            SongProgressBar.BeginAnimation(ProgressBar.ValueProperty, doubleAnimation);
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _MediaPlayer.Volume = SongVolume.Value;
            volumeValue.Content = Math.Round(SongVolume.Value * 100, 0).ToString();
        }
        private void SongProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentTime.Text = MediaPosition.ToString(@"mm\:ss");
            HowLong.Text = MediaTotal.ToString(@"mm\:ss");
            _MediaPlayer.MediaEnded += _MediaPlayer_MediaEnded;
        }

        private void _MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            Index++;
            if (Index == Paths.Count)
            {
                Index = 0;
                SongsList.SelectedIndex = Index;
            }
            else
                SongsList.SelectedIndex = Index;
        }

        private void stop_Button(object sender, RoutedEventArgs e)
        {
            _MediaPlayer.Pause();
            _MediaPlayer.Close();
            SongProgressBar.BeginAnimation(ProgressBar.ValueProperty, null);
            CurrentTime.Text = "0:00";
            HowLong.Text = "0:00";
            Index = 0;
            ActualSong.Text = "";
            SongsList.SelectedItem = null;
        }
    }
}
