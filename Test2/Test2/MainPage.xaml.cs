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
using System.Linq;
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

        public MainPage()
        {
            this.InitializeComponent();

            setMediaPlayer();

            mediaPlayer.CurrentStateChanged += media_change;
            mediaPlayer.MediaEnded += media_ended;
            //updateVideoState();
        }

        public void media_ended(object sender, RoutedEventArgs e)
        {
            // Next video
        }

        public void media_change(object sender, RoutedEventArgs e)
        {

            switch (mediaPlayer.CurrentState)
            {
                case MediaElementState.Buffering:
                    break;
                case MediaElementState.Closed:
                    Debug.WriteLine("Closed");
                    break;
                case MediaElementState.Opening:
                    break;
                case MediaElementState.Paused:
                    updateVideoState("paused");
                    break;
                case MediaElementState.Playing:
                    updateVideoState("playing");
                    break;
                case MediaElementState.Stopped:
                    Debug.WriteLine("stopped");
                    break;
                default:
                    break;
            }
        }


        public async void updateVideoState(string status)
        {

            var asyncConnectionString = new SqlConnectionStringBuilder(@"Data Source=DESKTOP-4TP64DH;Initial Catalog=ChatrTestDB;Integrated Security=True").ToString();


            using (SqlConnection connection = new SqlConnection(asyncConnectionString))
            {
                await connection.OpenAsync();
                SqlCommand command = connection.CreateCommand();

                try
                {
                    var isPaused = 1;
                    if (status == "playing")
                    {
                        isPaused = 0;
                    }
                    else if (status == "paused")
                    {
                        isPaused = 1;
                    }
                    command.CommandText =
                        String.Format("UPDATE Video SET isPaused = {0} WHERE videoID = 2", isPaused);
                    await command.ExecuteNonQueryAsync();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Something went wrong: {0}", ex.Message);
                }
            }
        }

        private bool isPlaying()
        {
            if (mediaPlayer.CurrentState.ToString() == "Playing")
            {
                return true;
            }
            else
            {
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

        }


    }
}