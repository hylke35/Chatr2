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
using System.Collections;
using Windows.UI.Core;
using Windows.UI.Core.Preview;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Test2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DBConnect connection = new DBConnect();
        SemaphoreSlim ss = new SemaphoreSlim(2);
        string tempDir;
        string lobbyCode;
        int userID;
        IDictionary<int, string> videos;
        int currentVideo = 0;
        Queue qt;


        public delegate void timerTick();
        DispatcherTimer ticks = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
            tempDir = SetTemporaryDirectory();
            qt = new Queue();
            mediaPlayer.CurrentStateChanged += media_change;
            mediaPlayer.MediaEnded += media_ended;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.testi;
        }

        public async void testi(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            IDictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@code", this.lobbyCode);

            IEnumerable<object> list2 = await connection.DataReader("SELECT * FROM dbo.User_Lobby WHERE lobbyCode = @code", "User_Lobby", p);

            List<object> test2 = list2.ToList();

            // If last one
            if (test2.Count == 1)
            {
                IDictionary<string, object> p3 = new Dictionary<string, object>();
                p3.Add("@code", this.lobbyCode);

                connection.runQueryAsync("DELETE FROM dbo.Lobby WHERE lobbyCode = @code", p3);
            }
            else
            {
                IDictionary<string, object> p2 = new Dictionary<string, object>();
                p2.Add("@userID", userID);
                p2.Add("@code", this.lobbyCode);

                connection.runQueryAsync("DELETE FROM dbo.User_Lobby WHERE userID = @userID AND lobbyCode = @code", p2);
            }



            SqlDependency.Stop(connection.getConnectionString());
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameters = (Params)e.Parameter;
            lobbyCode = parameters.LobbyCode;
            userID = parameters.UserId;

            videos = await getURLs();
            List<string> urls = videos.Values.ToList();

            downloadVideoList(urls);

            changeStateTime();
            changeStatePause();

        }

        public void changeStatePause()
        {
            using (SqlConnection conn = new SqlConnection(connection.getConnectionString()))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand("SELECT isPaused FROM dbo.Video WHERE videoID = '" + videos.Keys.ToArray()[currentVideo] + "'", conn))
                {

                    // Create a dependency and associate it with the SqlCommand.
                    SqlDependency dependency = new SqlDependency(command);
                    // Maintain the reference in a class member.

                    // Subscribe to the SqlDependency event.
                    dependency.OnChange += new OnChangeEventHandler(OnDependencyChangeStatePause);

                    //Execute the command.
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                    }
                }
                conn.Close();
            }
        }

        private async void OnDependencyChangeStatePause(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && e.Info == SqlNotificationInfo.Update)
            {
                IDictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@videoID", videos.Keys.ToArray()[currentVideo]);

                IEnumerable<object> list2 = await connection.DataReader("SELECT * FROM dbo.Video WHERE videoID = @videoID", "Video", p);

                List<object> test2 = list2.ToList();

                foreach (Video l in test2)
                {
                    Debug.WriteLine("Paused: " + l.IsPaused.ToString());
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        if (l.IsPaused == 1)
                        {
                            mediaPlayer.Pause();
                        }
                        else
                        {
                            mediaPlayer.Play();
                        }

                    });

                }

            }
            SqlDependency dependency = sender as SqlDependency;
            changeStatePause();
        }

        public void changeStateTime()
        {
            Debug.WriteLine("Sync Time: " + videos.Keys.ToArray()[currentVideo]);
            using (SqlConnection conn = new SqlConnection(connection.getConnectionString()))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand("SELECT syncTime FROM dbo.Video WHERE videoID = '" + videos.Keys.ToArray()[currentVideo] + "'", conn))
                {

                    // Create a dependency and associate it with the SqlCommand.
                    SqlDependency dependency = new SqlDependency(command);
                    // Maintain the reference in a class member.

                    // Subscribe to the SqlDependency event.
                    dependency.OnChange += new OnChangeEventHandler(OnDependencyChangeStateTime);

                    //Execute the command.
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                    }
                }
                conn.Close();
            }
        }

        private async void OnDependencyChangeStateTime(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && e.Info == SqlNotificationInfo.Update)
            {

                IDictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@videoID", videos.Keys.ToArray()[currentVideo]);

                IEnumerable<object> list2 = await connection.DataReader("SELECT * FROM dbo.Video WHERE videoID = @videoID", "Video", p);

                List<object> test2 = list2.ToList();

                foreach (Video l in test2)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        mediaPlayer.Position = TimeSpan.FromSeconds(l.SyncTime);
                        PositionSlider.Value = l.SyncTime;

                        Binding b = new Binding();
                        b.ElementName = "mediaPlayer";
                        b.Path = new PropertyPath("Position.TotalSeconds");
                        PositionSlider.SetBinding(Slider.ValueProperty, b);
                    });

                }

            }
            SqlDependency dependency = sender as SqlDependency;
            changeStateTime();
        }


        public async Task<IDictionary<int, string>> getURLs()
        {
            IDictionary<int, string> urls = new Dictionary<int, string>();

            IDictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@code", this.lobbyCode);

            IEnumerable<object> list2 = await connection.DataReader("SELECT link, videoID, syncTime, isPaused, title FROM dbo.Video WHERE lobbyCode = @code", "Video", p);

            List<object> test2 = list2.ToList();

            foreach (Video l in test2)
            {
                urls.Add(l.VideoId, l.Link);
            }

            return urls;
        }


        public async void media_ended(object sender, RoutedEventArgs e)
        {
            StopTimer();
            PositionSlider.Value = 0.0;

            File.Delete((string)qt.Peek());

            qt.Dequeue();
            if (qt.Count != 0)
            {
                await setMediaPlayerAsync((string)qt.Peek());
                currentVideo++;
            }
            else
            {
                // Go back to lobby screen
                IDictionary<string, object> f = new Dictionary<string, object>();
                f.Add("@code", lobbyCode);

                connection.runQueryAsync("UPDATE Lobby SET inProgress = 0 WHERE lobbyCode = @code", f);

                var parameters = new Params();
                parameters.LobbyCode = lobbyCode;
                parameters.UserId = userID;
                Frame.Navigate(typeof(VideoLobby), parameters);
            }
        }

        // ------------------

        private DispatcherTimer _timer;
        private bool _sliderpressed = false;

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            PositionSlider.ValueChanged += timelineSlider_ValueChanged;

            PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            PositionSlider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
            PositionSlider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(PositionSlider.StepFrequency);
            StartTimer();
        }

        private void _timer_Tick(object sender, object e)
        {
            if (!_sliderpressed)
            {
                PositionSlider.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        private void StartTimer()
        {
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
            _timer.Tick -= _timer_Tick;
        }


        private double SliderFrequency(TimeSpan timevalue)
        {
            double stepfrequency = -1;

            double absvalue = (int)Math.Round(
                timevalue.TotalSeconds, MidpointRounding.AwayFromZero);

            stepfrequency = (int)(Math.Round(absvalue / 100));

            if (timevalue.TotalMinutes >= 10 && timevalue.TotalMinutes < 30)
            {
                stepfrequency = 10;
            }
            else if (timevalue.TotalMinutes >= 30 && timevalue.TotalMinutes < 60)
            {
                stepfrequency = 30;
            }
            else if (timevalue.TotalHours >= 1)
            {
                stepfrequency = 60;
            }

            if (stepfrequency == 0) stepfrequency += 1;

            if (stepfrequency == 1)
            {
                stepfrequency = absvalue / 100;
            }

            return stepfrequency;
        }



        void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _sliderpressed = true;
        }

        void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            // Update Time in DB
            Slider slider = (Slider)sender;
            IDictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@currentTime", PositionSlider.Value);
            p.Add("@videoID", videos.Keys.ToArray()[currentVideo]);

            connection.runQueryAsync("UPDATE dbo.Video SET syncTime = @currentTime WHERE videoID = @videoID", p);

            mediaPlayer.Position = TimeSpan.FromSeconds(PositionSlider.Value);

            Binding b = new Binding();
            b.ElementName = "mediaPlayer";
            b.Path = new PropertyPath("Position.TotalSeconds");
            slider.SetBinding(Slider.ValueProperty, b);
            _sliderpressed = false;
        }

        void timelineSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!_sliderpressed)
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(e.NewValue);
            }
        }


        private string GetHresultFromErrorMessage(ExceptionRoutedEventArgs e)
        {
            String hr = String.Empty;
            String token = "HRESULT - ";
            const int hrLength = 10;     // eg "0xFFFFFFFF"

            int tokenPos = e.ErrorMessage.IndexOf(token, StringComparison.Ordinal);
            if (tokenPos != -1)
            {
                hr = e.ErrorMessage.Substring(tokenPos + token.Length, hrLength);
            }

            return hr;
        }

        // -----------------------



        public void media_change(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.CurrentState == MediaElementState.Playing)
            {
                if (_sliderpressed)
                {
                    _timer.Stop();
                }
                else
                {
                    _timer.Start();
                }
            }

            if (mediaPlayer.CurrentState == MediaElementState.Paused)
            {
                _timer.Stop();
            }

            if (mediaPlayer.CurrentState == MediaElementState.Stopped)
            {
                _timer.Stop();
                PositionSlider.Value = 0;
            }


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


        public void updateVideoState(string status)
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

            IDictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@isPaused", isPaused);
            p.Add("@videoID", videos.Keys.ToArray()[currentVideo]);
            connection.runQueryAsync("UPDATE dbo.Video SET isPaused = @isPaused WHERE videoID = @videoID", p);

        }



        public string SetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath());
            Directory.CreateDirectory(tempDirectory);
            Debug.WriteLine(tempDirectory);
            return tempDirectory;
        }

        public async Task setMediaPlayerAsync(string uri)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                mediaPlayer.Source = new Uri(@"ms-appx:///" + uri);
                playList.SelectedIndex = currentVideo;
            });

            mediaPlayer.MediaOpened += loaded;

        }


        public void loaded(object sender, RoutedEventArgs e)
        {
            double absvalue = (int)Math.Round(
            mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds,
            MidpointRounding.AwayFromZero);
            PositionSlider.Maximum = absvalue;
            PositionSlider.StepFrequency =
                SliderFrequency(mediaPlayer.NaturalDuration.TimeSpan);
            SetupTimer();

            CheckLoadedState();

            IDictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@userID", userID);
            p.Add("@lobbyCode", lobbyCode);
            connection.runQueryAsync("UPDATE dbo.User_Lobby SET isLoaded = 1 WHERE lobbyCode = @lobbyCode AND userID = @userID", p);


            changeStateTime();
        }

        public void CheckLoadedState()
        {
            using (SqlConnection conn = new SqlConnection(connection.getConnectionString()))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand("SELECT isLoaded FROM dbo.User_Lobby WHERE lobbyCode = '" + this.lobbyCode + "'", conn))
                {

                    // Create a dependency and associate it with the SqlCommand.
                    SqlDependency dependency = new SqlDependency(command);
                    // Maintain the reference in a class member.

                    // Subscribe to the SqlDependency event.
                    dependency.OnChange += new OnChangeEventHandler(OnDependencyCheckLoadedState);

                    //Execute the command.
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                    }
                }
                conn.Close();
            }
        }

        private async void OnDependencyCheckLoadedState(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && e.Info == SqlNotificationInfo.Update)
            {
                IDictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@code", this.lobbyCode);

                IEnumerable<object> list2 = await connection.DataReader("SELECT * FROM dbo.User_Lobby WHERE lobbyCode = @code", "User_Lobby", p);

                List<object> test2 = list2.ToList();

                bool allLoaded = true;
                foreach (UserLobby l in test2)
                {
                    if (l.isLoaded == 0)
                    {
                        allLoaded = false;
                    }
                }

                if (allLoaded)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        this.mediaPlayer.Play();
                    });

                }

            }
            SqlDependency dependency = sender as SqlDependency;
            CheckLoadedState();
        }

        public string downloadVideo(string link)
        {
            // this.ss.Wait();
            Debug.WriteLine("Begninn");
            var youTube = YouTube.Default; // starting point for YouTube actions
            var video = youTube.GetVideo(link); // gets a Video object with info about the video
            var path = tempDir + video.FullName;
            File.WriteAllBytes(path, video.GetBytes());
            Debug.WriteLine("END");
            //Notify the semaphore that there is a space available
            // this.ss.Release();


            return path;
        }

        public async Task<string> downloadVideoAsync(string link)
        {
            return await Task.Run(() => downloadVideo(link));
        }



        public async void downloadVideoList(List<string> urls)
        {

            List<Task<string>> trackedTasks = new List<Task<string>>();
            foreach (var item in urls)
            {
                await ss.WaitAsync().ConfigureAwait(false);
                trackedTasks.Add(Task.Run(() =>
                {

                    try
                    {
                        return downloadVideo(item);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error: " + e.Message);
                        throw;
                    }
                    finally
                    {
                        ss.Release();
                    }
                }));
            }
            var results = await Task.WhenAll(trackedTasks);

            foreach (var r in results)
            {
                qt.Enqueue(r);
            }

            foreach (var q in qt)
            {
                var r = q.ToString();
                var mp4 = r.Substring(r.LastIndexOf('\\') + 1);
                var result = mp4.Substring(0, mp4.Length - 4);
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    playList.Items.Add(result);
                });

            }

            await setMediaPlayerAsync(results[0]);
        }

        private void playList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Debug.WriteLine(playList.SelectedItem);
        }
    }
}