using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Test2.DB;
using Test2.VideoParty.ClassHelper;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;


// TODO: Hemran
// Check if inProgress is 0 after done with VideoParty [ ]
// Clean up code (VideoLobby, MainPage, Chat) [ ]

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Test2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoLobby : Page
    {
        int userID;
        string lobbyCode;
        int ready = 0;
        DBConnect connection = new DBConnect();

        public VideoLobby()
        {
            this.InitializeComponent();
            // Starts SqlDependency for specific connection
            SqlDependency.Start(connection.getConnectionString());
        }

        // Event that gets triggered when navigated to this page
        /// <summary>
        /// Event that gets triggered when navigated to this page 
        /// </summary>
        /// <param name="e">Event variable</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Maps the passed parameters to the page
            var parameters = (Params)e.Parameter;
            userID = parameters.UserId;
            lobbyCode = parameters.LobbyCode;
            // Event Handler for Closing the Window
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.onClose;
            // Initialize Lists and Listeners
            ListInit();
            changedState();
            changedStateUsers();
            changedStateReady();
            lobbyCodeLabel.Text = this.lobbyCode;
        }

        // Event that gets triggered when windows is closed
        /// <summary>
        /// Event that gets triggered when windows is closed
        /// </summary>
        public async void onClose(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            // Get all users within specified lobby
            IDictionary<string, object> userLobbyCheckDictionary = new Dictionary<string, object>();
            userLobbyCheckDictionary.Add("@code", lobbyCode);
            IEnumerable<object> userLobbyCheck = await connection.DataReader("SELECT * FROM dbo.User_Lobby WHERE lobbyCode = @code", "User_Lobby", userLobbyCheckDictionary);
            List<object> userLobbyCheckList = userLobbyCheck.ToList();

            // If the user is the last user remaining, then continue
            if (userLobbyCheckList.Count == 1)
            {
                // Delete Lobby from DB
                IDictionary<string, object> deleteLobbyDictionary = new Dictionary<string, object>();
                deleteLobbyDictionary.Add("@code", lobbyCode);
                connection.runQueryAsync("DELETE FROM dbo.Lobby WHERE lobbyCode = @code", deleteLobbyDictionary);
            }
            else
            {
                // Delete User from Lobby
                IDictionary<string, object> deleteUserLobbyDictionary = new Dictionary<string, object>();
                deleteUserLobbyDictionary.Add("@userID", userID);
                deleteUserLobbyDictionary.Add("@code", lobbyCode);
                connection.runQueryAsync("DELETE FROM dbo.User_Lobby WHERE userID = @userID AND lobbyCode = @code", deleteUserLobbyDictionary);
            }



            SqlDependency.Stop(connection.getConnectionString());
        }

        // Gets and puts Lobby information into User interface
        /// <summary>
        /// Gets and puts Lobby information into User interface
        /// </summary>
        public async void ListInit()
        {
            // Gets all the Users from this Lobby
            IDictionary<string, object> userLobbyDictionary = new Dictionary<string, object>();
            userLobbyDictionary.Add("@code", this.lobbyCode);
            IEnumerable<object> userLobby = await connection.DataReader("SELECT * FROM User_Lobby WHERE lobbyCode = @code", "User_Lobby", userLobbyDictionary);
            List<object> userLobbyList = userLobby.ToList();

            // Clears Items in LostBox
            userList.Items.Clear();

            foreach (UserLobby ul in userLobbyList)
            {
                // Selects User information
                IDictionary<string, object> userDictionary = new Dictionary<string, object>();
                userDictionary.Add("@userID", ul.userId);
                IEnumerable<object> userInLobby = await connection.DataReader("SELECT * FROM Users WHERE userID = @userID", "Users", userDictionary);
                List<object> userInLobbyList = userInLobby.ToList();

                // Gets first result
                User u = (User)userInLobbyList.First();
                // Checks is User is ready
                if (ul.isReady == 1)
                {
                    userList.Items.Add(u.Username + " - Ready");
                }
                else
                {
                    userList.Items.Add(u.Username);
                }
            }

            // Gets all Videos from this Lobby
            IDictionary<string, object> videoLobbyDictionary = new Dictionary<string, object>();
            videoLobbyDictionary.Add("@code", this.lobbyCode);
            IEnumerable<object> videoLobby = await connection.DataReader("SELECT * FROM dbo.Video WHERE lobbyCode = @code", "Video", videoLobbyDictionary);
            List<object> videoLobbyList = videoLobby.ToList();

            Dictionary<string, string> videos = new Dictionary<string, string>();
            foreach (Video v in videoLobbyList)
            {
                videos.Add(v.Link, v.Title);
            }
            // Create key/value pair to identify videos
            videoList.ItemsSource = videos;
            videoList.SelectedValuePath = "Key";
            videoList.DisplayMemberPath = "Value";

        }

        // DB Listener for Videos in Lobby
        /// <summary>
        /// DB Listener for Videos in Lobby.
        /// </summary>
        public void changedState()
        {
            using (SqlConnection conn = new SqlConnection(connection.getConnectionString()))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand("SELECT link FROM dbo.Video WHERE link LIKE 'http%' AND lobbyCode = '" + this.lobbyCode + "'", conn))
                {

                    // Create a dependency and associate it with the SqlCommand.
                    SqlDependency dependency = new SqlDependency(command);
                    // Maintain the reference in a class member.

                    // Subscribe to the SqlDependency event.
                    dependency.OnChange += new OnChangeEventHandler(OnDependencyChange);

                    //Execute the command.
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Process the DataReader.
                    }
                }
                conn.Close();
            }
        }

        // DB Listener for Users in Lobby
        /// <summary>
        /// DB Listener for Users in Lobby
        /// </summary>
        public void changedStateUsers()
        {
            using (SqlConnection conn = new SqlConnection(connection.getConnectionString()))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand("SELECT userID FROM dbo.User_Lobby WHERE lobbyCode = '" + this.lobbyCode + "'", conn))
                {

                    // Create a dependency and associate it with the SqlCommand.
                    SqlDependency dependency = new SqlDependency(command);
                    // Maintain the reference in a class member.

                    // Subscribe to the SqlDependency event.
                    dependency.OnChange += new OnChangeEventHandler(OnDependencyChangeUsers);

                    //Execute the command.
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                    }
                }
                conn.Close();
            }
        }

        // DB Listener for checking if ready state changed
        /// <summary>
        /// DB Listener for checking if ready state changed
        /// </summary>
        public void changedStateReady()
        {
            using (SqlConnection conn = new SqlConnection(connection.getConnectionString()))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand("SELECT isReady FROM dbo.User_Lobby WHERE lobbyCode = '" + this.lobbyCode + "'", conn))
                {

                    // Create a dependency and associate it with the SqlCommand.
                    SqlDependency dependency = new SqlDependency(command);
                    // Maintain the reference in a class member.

                    // Subscribe to the SqlDependency event.
                    dependency.OnChange += new OnChangeEventHandler(OnDependencyChangeReady);

                    //Execute the command.
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                    }
                }
                conn.Close();
            }
        }

        // Event, that gets triggerd when isReady value changes in DB
        /// <summary>
        /// Event, that gets triggerd when isReady value changes in DB
        /// </summary>
        private async void OnDependencyChangeReady(object sender, SqlNotificationEventArgs e)
        {
            // If data updated
            if (e.Type == SqlNotificationType.Change && e.Info == SqlNotificationInfo.Update)
            {
                // Gets users from Lobby
                IDictionary<string, object> userLobbyReadyDictionary = new Dictionary<string, object>();
                userLobbyReadyDictionary.Add("@code", this.lobbyCode);
                IEnumerable<object> userLobbyReady = await connection.DataReader("SELECT * FROM User_Lobby WHERE lobbyCode = @code", "User_Lobby", userLobbyReadyDictionary);
                List<object> userLobbyReadyList = userLobbyReady.ToList();

                // Clears the userList ListBox
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>{ userList.Items.Clear(); }).AsTask();

                bool allReady = true;
                foreach (UserLobby ul in userLobbyReadyList)
                {
                    // Gets infromation of user in lobby
                    IDictionary<string, object> userReadyDictionary = new Dictionary<string, object>();
                    userReadyDictionary.Add("@userID", ul.userId);
                    IEnumerable<object> userReady = await connection.DataReader("SELECT * FROM Users WHERE userID = @userID", "Users", userReadyDictionary);
                    List<object> userReadyList = userReady.ToList();

                    User u = (User)userReadyList.First();
                    // Check if user is ready
                    if (ul.isReady == 0)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            userList.Items.Add(u.Username);
                        }).AsTask();

                        allReady = false;
                    }
                    else
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            userList.Items.Add(u.Username + " - Ready");
                        }).AsTask();

                    }
                }

                if (allReady)
                {
                    if (videoList.Items.Count > 0)
                    {
                        // Get Lobbt with specific Lobby code
                        IDictionary<string, object> lobbyDictionary = new Dictionary<string, object>();
                        lobbyDictionary.Add("@code", this.lobbyCode);
                        IEnumerable<object> lobby = await connection.DataReader("SELECT * FROM Lobby WHERE lobbyCode = @code", "Lobby", lobbyDictionary);
                        Lobby lobbyObject = (Lobby)lobby.ToList().First();
                        // If Lobby is in not in Progress, then continue
                        if (lobbyObject.InProgress != 1)
                        {
                            // Update inProgress column from Lobby
                            IDictionary<string, object> inProgressUpdateDictionary = new Dictionary<string, object>();
                            inProgressUpdateDictionary.Add("@code", lobbyCode);
                            connection.runQueryAsync("UPDATE Lobby SET inProgress = 1 WHERE lobbyCode = @code", inProgressUpdateDictionary);
                        }

                        // Redirect to VideoParty
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            var parameters = new Params();
                            parameters.LobbyCode = this.lobbyCode;
                            parameters.UserId = this.userID;
                            Frame.Navigate(typeof(MainPage), parameters);
                        }).AsTask();
                    }
                    else
                    {
                        // Video List is empty, cant proceed
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        async () =>
                        {
                            var dialog = new MessageDialog("Video list is empty!", "Error");
                            await dialog.ShowAsync();

                        });
                    }
                }
            }
            SqlDependency dependency = sender as SqlDependency;
            changedStateReady();
        }

        // Event, that gets triggered when change in User_Lobby table
        /// <summary>
        /// Event, that gets triggered when change in User_Lobby table
        /// </summary>
        private async void OnDependencyChangeUsers(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && (e.Info == SqlNotificationInfo.Insert || e.Info == SqlNotificationInfo.Delete))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    // Clears userList Listbox
                    this.userList.Items.Clear();

                    // Gets users in Lobby
                    IDictionary<string, object> userInLobbyDictionary = new Dictionary<string, object>();
                    userInLobbyDictionary.Add("@code", lobbyCode);
                    IEnumerable<object> userInLobby = await connection.DataReader("SELECT dbo.Users.* FROM dbo.User_Lobby, dbo.Users WHERE dbo.User_Lobby.lobbyCode = @code AND dbo.User_Lobby.userID = dbo.Users.userID", "Users", userInLobbyDictionary);
                    List<object> userInLobbyList = userInLobby.ToList();

                    foreach (User u in userInLobbyList)
                    {
                        // Adds all users from Lobby in ListBox
                        userList.Items.Add(u.Username);
                    }
                }
                ).AsTask();
            }
            SqlDependency dependency = sender as SqlDependency;
            changedStateUsers();
        }

        // Event, that gets triggered when change in Video table
        /// <summary>
        ///Event, that gets triggered when change in Video table
        /// </summary>
        private async void OnDependencyChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && (e.Info == SqlNotificationInfo.Insert || e.Info == SqlNotificationInfo.Delete))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    // Clears videoList Listbox
                    videoList.ItemsSource = null;

                    // Gets all videos from Lobby
                    IDictionary<string, object> selectVideoDictionary = new Dictionary<string, object>();
                    selectVideoDictionary.Add("@code", this.lobbyCode);
                    IEnumerable<object> selectVideo = await connection.DataReader("SELECT * FROM Video WHERE lobbyCode = @code", "Video", selectVideoDictionary);
                    List<object> selectVideoList = selectVideo.ToList();

                    Dictionary<string, string> videos = new Dictionary<string, string>();
                    foreach (Video l in selectVideoList)
                    {
                        videos.Add(l.Link, l.Title);
                    }

                    videoList.ItemsSource = videos;
                    videoList.SelectedValuePath = "Key";
                    videoList.DisplayMemberPath = "Value";
                }).AsTask();

            }
            SqlDependency dependency = sender as SqlDependency;
            changedState();
        }

        // Event, that gets triggered when the Add button is clicked
        /// <summary>
        /// Event, that gets triggered when the Add button is clicked
        /// </summary>
        private async void addButton_Click(object sender, RoutedEventArgs e)
        {
            // Gets all video from Lobby
            IDictionary<string, object> addVideoDictionary = new Dictionary<string, object>();
            addVideoDictionary.Add("@link", linkBox.Text);
            IEnumerable<object> addVideo = await connection.DataReader("SELECT * FROM dbo.Video WHERE link = @link", "Video", addVideoDictionary);
            List<object> addVideoList = addVideo.ToList();

            if (addVideoList.Count == 0)
            {
                var link = "https://www.youtube.com/oembed?format=json&url=" + linkBox.Text;
                // Checks if URL is YouTube
                if (Uri.IsWellFormedUriString(link, UriKind.Absolute) && (link.Contains("youtube.com") || link.Contains("youtu.be")))
                {
                    var request = (HttpWebRequest)WebRequest.CreateHttp(link);
                    using (var response = (HttpWebResponse)request.GetResponseWithoutException())
                    {
                        // Checks if there is a 404
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var title = "";
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                                String responseString = reader.ReadToEnd();
                                // Gets the title of the Video from repsonse
                                title = JsonConvert.DeserializeObject<YouTubeJSON>(responseString).title;
                            }
                            // Inserts information to Video Table
                            IDictionary<string, object> insertVideoDictionary = new Dictionary<string, object>();
                            insertVideoDictionary.Add("@link", linkBox.Text);
                            insertVideoDictionary.Add("@code", this.lobbyCode);
                            insertVideoDictionary.Add("@title", title);
                            connection.runQueryAsync("INSERT INTO dbo.Video(lobbyCode, title, link, syncTime, isPaused) VALUES (@code, @title, @link, 0, 1)", insertVideoDictionary);
                        }
                        else
                        {
                            // Youtube video doesnt exist (404)
                            var dialog = new MessageDialog("YouTube video does not exist!", "Error");
                            await dialog.ShowAsync();
                        }
                    }
                }
                else
                {
                    // Text is not a youtube link
                    var dialog = new MessageDialog("Not a YouTube link!", "Error");
                    await dialog.ShowAsync();
                }
            }
            else
            {
                // Video is already in list
                var dialog = new MessageDialog("Video is already in the list!", "Error");
                await dialog.ShowAsync();
            }
            linkBox.Text = "";
        }

        // Event, that gets triggered when the Ready button is clicked
        /// <summary>
        /// Event, that gets triggered when the Ready button is clicked
        /// </summary>
        private void readyButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            if (ready == 1)
            {
                ready = 0;
                b.Content = "Not ready";
            }
            else
            {
                ready = 1;
                b.Content = "Ready";
            }

            // Change Ready state from user in Lobby
            IDictionary<string, object> readyUserLobbyDictionary = new Dictionary<string, object>();
            readyUserLobbyDictionary.Add("@userID", userID);
            readyUserLobbyDictionary.Add("@lobbyCode", lobbyCode);
            readyUserLobbyDictionary.Add("@ready", ready);
            connection.runQueryAsync("UPDATE User_Lobby SET isReady = @ready WHERE userID = @userID AND lobbyCode = @lobbyCode", readyUserLobbyDictionary);
        }

        // Event, that gets triggered when an Item in videoList is getting double clicked
        /// <summary>
        /// Event, that gets triggered when an Item in videoList is getting double clicked
        /// </summary>
        private void videoList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var link = videoList.SelectedValue;
            // Deletes video from Lobby
            IDictionary<string, object> doubleTappedDictionary = new Dictionary<string, object>();
            doubleTappedDictionary.Add("@link", link);
            connection.runQueryAsync("DELETE FROM dbo.Video WHERE link = @link", doubleTappedDictionary);
        }

        // Event, that gets triggered when the leave button is clicked
        /// <summary>
        /// Event, that gets triggered when the leave button is clicked
        /// </summary>
        private async void leaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Gets all users from Lobbt
            IDictionary<string, object> userLobbyDictionary = new Dictionary<string, object>();
            userLobbyDictionary.Add("@code", this.lobbyCode);
            IEnumerable<object> userLobby = await connection.DataReader("SELECT * FROM dbo.User_Lobby WHERE lobbyCode = @code", "User_Lobby", userLobbyDictionary);
            List<object> userLobbyList = userLobby.ToList();

            // If last user
            if (userLobbyList.Count == 1)
            {
                // Deletes Lobby from DB
                IDictionary<string, object> deleteLobbyDictionary = new Dictionary<string, object>();
                deleteLobbyDictionary.Add("@code", lobbyCode);
                connection.runQueryAsync("DELETE FROM dbo.Lobby WHERE lobbyCode = @code", deleteLobbyDictionary);
            }
            else
            {
                // Deletes User From Lobby
                IDictionary<string, object> deleteUserLobbyDictionary = new Dictionary<string, object>();
                deleteUserLobbyDictionary.Add("@userID", userID);
                deleteUserLobbyDictionary.Add("@code", lobbyCode);
                connection.runQueryAsync("DELETE FROM dbo.User_Lobby WHERE userID = @userID AND lobbyCode = @code", deleteUserLobbyDictionary);
            }
            SqlDependency.Stop(connection.getConnectionString());
            // Closes Window
            Window.Current.Close();
        }
    }
}

