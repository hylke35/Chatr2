using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using VideoLibrary;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Test2.DB;

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
        Queue videoQueue;
        public delegate void timerTick();
        DispatcherTimer ticks = new DispatcherTimer();
        private DispatcherTimer _timer;
        private bool _sliderpressed = false;

        public MainPage()
        {
            this.InitializeComponent();
            tempDir = SetTemporaryDirectory();
            videoQueue = new Queue();
            mediaPlayer.CurrentStateChanged += Media_Change;
            mediaPlayer.MediaEnded += Media_Ended;
            // Add EventHandler to window close Listener
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnClose;
        }

        // Event that gets triggered when closing this page
        /// <summary>
        /// Event that gets triggered when closing this page
        /// </summary>
        public async void OnClose(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            // Gets users from Lobby
            IDictionary<string, object> userLobbySelectDictionary = new Dictionary<string, object>();
            userLobbySelectDictionary.Add("@code", lobbyCode);
            IEnumerable<object> userLobbySelect = await connection.DataReader("SELECT * FROM dbo.User_Lobby WHERE lobbyCode = @code", "User_Lobby", userLobbySelectDictionary);
            List<object> userLobbySelectList = userLobbySelect.ToList();

            // If last user
            if (userLobbySelectList.Count == 1)
            {
                // Delete Lobby from DB
                IDictionary<string, object> deleteLobbyDictionary = new Dictionary<string, object>();
                deleteLobbyDictionary.Add("@code", lobbyCode);
                connection.RunQueryAsync("DELETE FROM dbo.Lobby WHERE lobbyCode = @code", deleteLobbyDictionary);
            }
            else
            {
                // Delete User from Lobby
                IDictionary<string, object> deleteUserLobbyDictionary = new Dictionary<string, object>();
                deleteUserLobbyDictionary.Add("@userID", userID);
                deleteUserLobbyDictionary.Add("@code", lobbyCode);
                connection.RunQueryAsync("DELETE FROM dbo.User_Lobby WHERE userID = @userID AND lobbyCode = @code", deleteUserLobbyDictionary);
            }

            // Delete all videos from Storage
            foreach(string file in videoQueue)
            {
                File.Delete(file);
            }
            SqlDependency.Stop(connection.GetConnectionString());
        }

        // Event that gets triggered when app is navigated to this page
        /// <summary>
        ///  Event that gets triggered when app is navigated to this page
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameters = (Params)e.Parameter;
            lobbyCode = parameters.LobbyCode;
            userID = parameters.UserId;

            // Get URLs from Lobby and save them in videos var
            videos = await GetURLs();
            List<string> urls = videos.Values.ToList();

            // Download all videos
            DownloadVideoList(urls);

            // Initiate listeners
            ChangeStateTime();
            ChangeStatePause();
        }

        // Video pause state Listener
        /// <summary>
        /// Video pause state Listener
        /// </summary>
        public void ChangeStatePause()
        {
            using (SqlConnection conn = new SqlConnection(connection.GetConnectionString()))
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

        // Event that gets triggered when video gets paused 
        /// <summary>
        /// Event that gets triggered when video gets paused 
        /// </summary>
        private async void OnDependencyChangeStatePause(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && e.Info == SqlNotificationInfo.Update)
            {
                // Gets changed video from DB
                IDictionary<string, object> selectVideoDictionary = new Dictionary<string, object>();
                selectVideoDictionary.Add("@videoID", videos.Keys.ToArray()[currentVideo]);
                IEnumerable<object> selectVideo = await connection.DataReader("SELECT * FROM dbo.Video WHERE videoID = @videoID", "Video", selectVideoDictionary);
                List<object> selectVideoList = selectVideo.ToList();

                // Pauses/Plays video depending on DB results
                foreach (DB.Video l in selectVideoList)
                {
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
            ChangeStatePause();
        }

        // Video timeline change
        /// <summary>
        ///  Video timeline change
        /// </summary>
        public void ChangeStateTime()
        {
            Debug.WriteLine("Sync Time: " + videos.Keys.ToArray()[currentVideo]);
            using (SqlConnection conn = new SqlConnection(connection.GetConnectionString()))
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

        // Event that gets triggered when videos timeline changes
        /// <summary>
        ///  Event that gets triggered when videos timeline changes
        /// </summary>
        private async void OnDependencyChangeStateTime(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && e.Info == SqlNotificationInfo.Update)
            {
                // Get changed video
                IDictionary<string, object> selectVideoDictionary = new Dictionary<string, object>();
                selectVideoDictionary.Add("@videoID", videos.Keys.ToArray()[currentVideo]);
                IEnumerable<object> selectVideo = await connection.DataReader("SELECT * FROM dbo.Video WHERE videoID = @videoID", "Video", selectVideoDictionary);
                List<object> selectVideoList = selectVideo.ToList();

                // Synconize local player with DB time
                foreach (DB.Video l in selectVideoList)
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
            ChangeStateTime();
        }

        // Gets video URLS from Lobby
        /// <summary>
        /// Gets videoURLS from Lobby
        /// </summary>
        public async Task<IDictionary<int, string>> GetURLs()
        {
            IDictionary<int, string> urls = new Dictionary<int, string>();
            IDictionary<string, object> getVideoDataDictionary = new Dictionary<string, object>();
            getVideoDataDictionary.Add("@code", this.lobbyCode);
            IEnumerable<object> getVideoData = await connection.DataReader("SELECT link, videoID, syncTime, isPaused, title FROM dbo.Video WHERE lobbyCode = @code", "Video", getVideoDataDictionary);
            List<object> getVideoDataList = getVideoData.ToList();
            foreach (DB.Video l in getVideoDataList)
            {
                urls.Add(l.VideoId, l.Link);
            }
            return urls;
        }

        // Event that gets triggered when video ended
        /// <summary>
        /// Event that gets triggered when video ended
        /// </summary>
        public async void Media_Ended(object sender, RoutedEventArgs e)
        {
            StopTimer();
            PositionSlider.Value = 0.0;

            // Delete video from storage
            File.Delete((string)videoQueue.Peek());
            videoQueue.Dequeue();
            // If there is still a video in queue
            if (videoQueue.Count != 0)
            {
                await SetMediaPlayerAsync((string)videoQueue.Peek());
                currentVideo++;
            }
            else
            {
                // No video in queue
                // Go back to lobby screen
                IDictionary<string, object> updateInProgressDictionary = new Dictionary<string, object>();
                updateInProgressDictionary.Add("@code", lobbyCode);
                connection.RunQueryAsync("UPDATE Lobby SET inProgress = 0 WHERE lobbyCode = @code", updateInProgressDictionary);

                IDictionary<string, object> updateUserReadyState = new Dictionary<string, object>();
                updateUserReadyState.Add("@code", lobbyCode);
                updateUserReadyState.Add("@userID", userID);
                connection.RunQueryAsync("UPDATE User_Lobby SET isReady = 0, isLoaded = 0 WHERE lobbyCode = @code AND userID = @userID", updateUserReadyState);


                // Redirect to Lobby
                var parameters = new Params();
                parameters.LobbyCode = lobbyCode;
                parameters.UserId = userID;
                Frame.Navigate(typeof(VideoLobby), parameters);
            }
        }

        // Event that gets triggered when page is loaded
        /// <summary>
        /// Event that gets triggered when page is loaded
        /// </summary>
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Add EventHandlers to Slider Listeners
            PositionSlider.ValueChanged += TimelineSlider_ValueChanged;
            PointerEventHandler pointerpressedhandler = new PointerEventHandler(Slider_PointerEntered);
            PositionSlider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);
            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(Slider_PointerCaptureLost);
            PositionSlider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        // ------------------ Custom TimeLine --------------------
        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(PositionSlider.StepFrequency);
            StartTimer();
        }

        private void Timer_Tick(object sender, object e)
        {
            if (!_sliderpressed)
            {
                PositionSlider.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        private void StartTimer()
        {
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
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

        void Slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _sliderpressed = true;
        }

        void Slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            // Update Time in DB
            Slider slider = (Slider)sender;
            IDictionary<string, object> syncTimeDictionary = new Dictionary<string, object>();
            syncTimeDictionary.Add("@currentTime", PositionSlider.Value);
            syncTimeDictionary.Add("@videoID", videos.Keys.ToArray()[currentVideo]);

            connection.RunQueryAsync("UPDATE dbo.Video SET syncTime = @currentTime WHERE videoID = @videoID", syncTimeDictionary);
            // Sync TimeLine with Video
            mediaPlayer.Position = TimeSpan.FromSeconds(PositionSlider.Value);

            Binding b = new Binding();
            b.ElementName = "mediaPlayer";
            b.Path = new PropertyPath("Position.TotalSeconds");
            slider.SetBinding(Slider.ValueProperty, b);
            _sliderpressed = false;
        }

        void TimelineSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!_sliderpressed)
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(e.NewValue);
            }
        }
        // ------------------ Custom TimeLine --------------------


        // Event that gets triggered when video changed
        /// <summary>
        /// Event that gets triggered when video changed
        /// </summary>
        public void Media_Change(object sender, RoutedEventArgs e)
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

            // Invoke method when Player paused/played
            switch (mediaPlayer.CurrentState)
            {
                case MediaElementState.Paused:
                    UpdateVideoState("paused");
                    break;
                case MediaElementState.Playing:
                    UpdateVideoState("playing");
                    break;
            }
        }

        // Event that gets triggered when Player gets paused or played
        /// <summary>
        /// Event that gets triggered when Player gets paused or played
        /// </summary>
        public void UpdateVideoState(string status)
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
            //Update state of isPaused in database
            IDictionary<string, object> updateVideoStateDictionary = new Dictionary<string, object>();
            updateVideoStateDictionary.Add("@isPaused", isPaused);
            updateVideoStateDictionary.Add("@videoID", videos.Keys.ToArray()[currentVideo]);
            connection.RunQueryAsync("UPDATE dbo.Video SET isPaused = @isPaused WHERE videoID = @videoID", updateVideoStateDictionary);
        }

        // Create and return temp directory for downloaded videos
        /// <summary>
        /// Create and return temp directory for downloaded videos
        /// </summary>
        public string SetTemporaryDirectory()
        {
            //Sets temp path for download location of videos
            string tempDirectory = Path.Combine(Path.GetTempPath());
            Directory.CreateDirectory(tempDirectory);
            Debug.WriteLine(tempDirectory);
            return tempDirectory;
        }

        // Sets the source of the MediaPlayer to the current video
        /// <summary>
        /// Sets the source of the MediaPlayer to the current video
        /// </summary>
        public async Task SetMediaPlayerAsync(string uri)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                mediaPlayer.Source = new Uri(@"ms-appx:///" + uri);
                playList.SelectedIndex = currentVideo;
            });
            mediaPlayer.MediaOpened += Media_Loaded;
        }

        // Event that gets triggered when video is loaded
        /// <summary>
        /// Event that gets triggered when video is loaded
        /// </summary>
        public void Media_Loaded(object sender, RoutedEventArgs e)
        {
            // Update Position of Slider
            double absvalue = (int)Math.Round(
            mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds,
            MidpointRounding.AwayFromZero);
            PositionSlider.Maximum = absvalue;
            PositionSlider.StepFrequency =
            SliderFrequency(mediaPlayer.NaturalDuration.TimeSpan);
            SetupTimer();
            // Initiate Listener
            CheckLoadedState();
            // Change isLoaded column from user in Lobby
            IDictionary<string, object> setIsLoadedDictionary = new Dictionary<string, object>();
            setIsLoadedDictionary.Add("@userID", userID);
            setIsLoadedDictionary.Add("@lobbyCode", lobbyCode);
            connection.RunQueryAsync("UPDATE dbo.User_Lobby SET isLoaded = 1 WHERE lobbyCode = @lobbyCode AND userID = @userID", setIsLoadedDictionary);
            ChangeStateTime();
        }

        // isLoaded DB Listener
        /// <summary>
        /// EisLoaded DB Listener
        /// </summary>
        public void CheckLoadedState()
        {
            using (SqlConnection conn = new SqlConnection(connection.GetConnectionString()))
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

        // Event that is triggered when isLoaded changed for a user
        /// <summary>
        /// Event that is triggered when isLoaded changed for a user
        /// </summary>
        private async void OnDependencyCheckLoadedState(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && e.Info == SqlNotificationInfo.Update)
            {
                // Gets users in Lobby
                IDictionary<string, object> selectUserLobbyDictionary = new Dictionary<string, object>();
                selectUserLobbyDictionary.Add("@code", this.lobbyCode);
                IEnumerable<object> selectUserLobby = await connection.DataReader("SELECT * FROM dbo.User_Lobby WHERE lobbyCode = @code", "User_Lobby", selectUserLobbyDictionary);
                List<object> selectUserLobbyList = selectUserLobby.ToList();

                bool allLoaded = true;
                foreach (UserLobby l in selectUserLobbyList)
                {
                    if (l.isLoaded == 0)
                    {
                        allLoaded = false;
                    }
                }

                // If everyone is loaded, play the video for everyone
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

        // Downloads specified Video
        /// <summary>
        /// Downloads specified Video and returns saved path
        /// </summary>
        public string DownloadVideo(string link)
        {
            Debug.WriteLine("Begnin"); // Semaphore Debugging Test
            var youTube = YouTube.Default; // starting point for YouTube actions
            var video = youTube.GetVideo(link); // gets a Video object with info about the video
            var path = tempDir + video.FullName;
            File.WriteAllBytes(path, video.GetBytes()); // Saves video
            Debug.WriteLine("END"); // Semaphore Debugging Test
            return path;
        }

        // Downloads specified videoList with a semaphore
        /// <summary>
        /// Downloads specified videoList with a semaphore that limits concurrent download of videos
        /// </summary>
        public async void DownloadVideoList(List<string> urls)
        {
            //Download videos
            List<Task<string>> trackedTasks = new List<Task<string>>();
            foreach (var item in urls)
            {
                // Ask semaphore for entrance
                await ss.WaitAsync().ConfigureAwait(false);
                trackedTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        return DownloadVideo(item);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error: " + e.Message);
                        throw;
                    }
                    finally
                    {
                        // Release thread/task from semaphore
                        ss.Release();
                    }
                }));
            }

            // When all tasks are done return into variable
            var results = await Task.WhenAll(trackedTasks);

            // Put all video paths into a queue
            foreach (var r in results)
            {
                videoQueue.Enqueue(r);
            }

            //Add each video into playlist
            foreach (var q in videoQueue)
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

            // Set MediaPlayers source to first video
            await SetMediaPlayerAsync(results[0]);
        }
    }
}