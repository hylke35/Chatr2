using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VideoLibrary;
using System.Security;
using System.Security.Permissions;
using Windows.UI.Popups;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Test2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DBConnect connection;
        public MainPage()
        {
            this.InitializeComponent();
            setMediaPlayer();
            
            
        }

        private bool isPlaying()
        {
            if ( mediaPlayer.CurrentState.ToString() == "Playing") {
                return true;               
            } else {
                return false;
            }
        }

        public string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public async void setMediaPlayer()
        {
            var video = await downloadVideoAsync("https://www.youtube.com/watch?v=QAUzWtLMnU0");
            mediaPlayer.Source = new Uri(@"ms-appx:///" + video);

            mediaPlayer.Play();
        }

        public string downloadVideo(string link)
        {
            var path = GetTemporaryDirectory() + "/test.mp4";
            var youTube = YouTube.Default; // starting point for YouTube actions
            var video = youTube.GetVideo(link); // gets a Video object with info about the video
            
            File.WriteAllBytes(path, video.GetBytes());

            return path;
        }

        public async Task<string> downloadVideoAsync(string link)
        {
            return await Task.Run(() => downloadVideo(link));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(isPlaying());
        }
    }
}
