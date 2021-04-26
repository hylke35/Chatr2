using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using System.Net.Http.Headers;
using Windows.UI.Popups;
using System.Net.Http;
using Windows.UI.WebUI;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using System.ComponentModel;
using Windows.UI.Core.Preview;



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
            SqlDependency.Start(connection.getConnectionString());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameters = (Params)e.Parameter;
            userID = parameters.UserId;
            lobbyCode = parameters.LobbyCode;

            // TODO: Hemran
            // Lobby Code Label bigger [X]
            // Check if youtube link is valid[X]
            // If player closes lobby -> remove from table [X]
            // Ready state next to user in userlist [X]
            // Video Name in listbox next to mediaPlayer [X]
            // disable fullscreen button [X]
            // Delete lobby from database if you are the last user to close lobby [X]
            // Show video name in both lists [X]
            // CHeck if link is already in list [X]
            // Create/Join lobby [X]
            // Leave lobby [X]
            // Only create or join lobby if user is not in User_Lobby [X]
            // Open lobby via new window [X]
            // When leaving MainPage, delete videos [ ]
            // implement Design [X]
            // Clean up code (VideoLobby, MainPage, Chat) [ ]

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.testi;
            ListInit();
            changedState();
            changedStateUsers();
            changedStateReady();

            lobbyCodeLabel.Text = this.lobbyCode;
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
            } else
            {
                IDictionary<string, object> p2 = new Dictionary<string, object>();
                p2.Add("@userID", userID);
                p2.Add("@code", this.lobbyCode);

                connection.runQueryAsync("DELETE FROM dbo.User_Lobby WHERE userID = @userID AND lobbyCode = @code", p2);
            }



            SqlDependency.Stop(connection.getConnectionString());
        }

        public async void ListInit()
        {
            IDictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@code", this.lobbyCode);

            IEnumerable<object> list2 = await connection.DataReader("SELECT * FROM User_Lobby WHERE lobbyCode = @code", "User_Lobby", p);

            List<object> test2 = list2.ToList();

            userList.Items.Clear();

            foreach (UserLobby l in test2)
            {

                IDictionary<string, object> p4 = new Dictionary<string, object>();
                p4.Add("@userID", l.userId);

                IEnumerable<object> list4 = await connection.DataReader("SELECT * FROM Users WHERE userID = @userID", "Users", p4);

                List<object> test4 = list4.ToList();
                User u = (User)test4.First();

                if (l.isReady == 1)
                {
                    userList.Items.Add(u.Username + " - Ready");
                }
                else
                {
                    userList.Items.Add(u.Username);
                }

            }

            IDictionary<string, object> p2 = new Dictionary<string, object>();


            p2.Add("@code", this.lobbyCode);

            IEnumerable<object> list3 = await connection.DataReader("SELECT * FROM dbo.Video WHERE lobbyCode = @code", "Video", p2);

            List<object> test3 = list3.ToList();

            Dictionary<string, string> videos = new Dictionary<string, string>();
            foreach (Video l in test3)
            {
                videos.Add(l.Link, l.Title);
            }

            videoList.ItemsSource = videos;
            videoList.SelectedValuePath = "Key";
            videoList.DisplayMemberPath = "Value";

        }

        public void changedState()
        {
            Debug.WriteLine(lobbyCode);
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



        private async void OnDependencyChangeReady(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && e.Info == SqlNotificationInfo.Update)
            {

                IDictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@code", this.lobbyCode);

                IEnumerable<object> list2 = await connection.DataReader("SELECT * FROM User_Lobby WHERE lobbyCode = @code", "User_Lobby", p);

                List<object> test2 = list2.ToList();
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    userList.Items.Clear();
                }).AsTask();


                bool allReady = true;
                foreach (UserLobby l in test2)
                {

                    IDictionary<string, object> p2 = new Dictionary<string, object>();
                    p2.Add("@userID", l.userId);

                    IEnumerable<object> list3 = await connection.DataReader("SELECT * FROM Users WHERE userID = @userID", "Users", p2);

                    List<object> test3 = list3.ToList();
                    User u = (User)test3.First();

                    if (l.isReady == 0)
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


                        IDictionary<string, object> p5 = new Dictionary<string, object>();
                        p5.Add("@code", this.lobbyCode);

                        IEnumerable<object> list5 = await connection.DataReader("SELECT * FROM Lobby WHERE lobbyCode = @code", "Lobby", p5);

                        Lobby test5 = (Lobby) list5.ToList().First();

                        if (test5.InProgress != 1)
                        {
                            IDictionary<string, object> f = new Dictionary<string, object>();
                            f.Add("@code", lobbyCode);

                            connection.runQueryAsync("UPDATE Lobby SET inProgress = 1 WHERE lobbyCode = @code", f);
                        }

                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            var parameters = new Params();
                            parameters.LobbyCode = this.lobbyCode;
                            parameters.UserId = this.userID;
                            Frame.Navigate(typeof(MainPage), parameters);
                        }).AsTask();
                    } else
                    {
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

        private async void OnDependencyChangeUsers(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && (e.Info == SqlNotificationInfo.Insert || e.Info == SqlNotificationInfo.Delete))
            {

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    this.userList.Items.Clear();

                    IDictionary<string, object> p = new Dictionary<string, object>();
                    p.Add("@code", this.lobbyCode);

                    IEnumerable<object> list2 = await connection.DataReader("SELECT dbo.Users.* FROM dbo.User_Lobby, dbo.Users WHERE dbo.User_Lobby.lobbyCode = @code AND dbo.User_Lobby.userID = dbo.Users.userID", "Users", p);

                    List<object> test2 = list2.ToList();

                    foreach (User l in test2)
                    {
                        userList.Items.Add(l.Username);
                    }
                }
                ).AsTask();
            }
            SqlDependency dependency = sender as SqlDependency;
            changedStateUsers();
        }

        private async void OnDependencyChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && (e.Info == SqlNotificationInfo.Insert || e.Info == SqlNotificationInfo.Delete))
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    this.videoList.ItemsSource = null;

                    this.videoList.ItemsSource = null;


                    IDictionary<string, object> p = new Dictionary<string, object>();
                    p.Add("@code", this.lobbyCode);

                    IEnumerable<object> list2 = await connection.DataReader("SELECT * FROM Video WHERE lobbyCode = @code", "Video", p);

                    List<object> test2 = list2.ToList();

                    Dictionary<string, string> videos = new Dictionary<string, string>();
                    foreach (Video l in test2)
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

        private async void addButton_Click(object sender, RoutedEventArgs e)
        {
            IDictionary<string, object> p2 = new Dictionary<string, object>();
            p2.Add("@link", linkBox.Text);
            IEnumerable<object> list3 = await connection.DataReader("SELECT * FROM dbo.Video WHERE link = @link", "Video", p2);

            List<object> test3 = list3.ToList();

            if (test3.Count == 0)
            {
                var link = "https://www.youtube.com/oembed?format=json&url=" + linkBox.Text;
                if (Uri.IsWellFormedUriString(link, UriKind.Absolute) && (link.Contains("youtube.com") || link.Contains("youtu.be")))
                {
                    var request = (HttpWebRequest)WebRequest.CreateHttp(link);
                    //request.Method = "HEAD";

                    using (var response = (HttpWebResponse)request.GetResponseWithoutException())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var title = "";
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                                String responseString = reader.ReadToEnd();

                                title = JsonConvert.DeserializeObject<YouTubeJSON>(responseString).title;
                            }

                            //Debug.WriteLine(response.GetResponseStream().ToString());
                            IDictionary<string, object> p = new Dictionary<string, object>();
                            p.Add("@link", linkBox.Text);
                            p.Add("@code", this.lobbyCode);
                            p.Add("@title", title);

                            connection.runQueryAsync("INSERT INTO dbo.Video(lobbyCode, title, link, syncTime, isPaused) VALUES (@code, @title, @link, 0, 1)", p);
                        }
                        else
                        {
                            var dialog = new MessageDialog("YouTube video does not exist!", "Error");
                            await dialog.ShowAsync();
                        }
                    }



                }
                else
                {
                    var dialog = new MessageDialog("Not a YouTube link!", "Error");
                    await dialog.ShowAsync();
                }
            }
            else
            {
                var dialog = new MessageDialog("Video is already in the list!", "Error");
                await dialog.ShowAsync();
            }

            linkBox.Text = "";

        }

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

            IDictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@userID", userID);
            p.Add("@lobbyCode", lobbyCode);
            p.Add("@ready", ready);

            connection.runQueryAsync("UPDATE User_Lobby SET isReady = @ready WHERE userID = @userID AND lobbyCode = @lobbyCode", p);
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void videoList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var link = videoList.SelectedValue;

            IDictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@link", link);
            connection.runQueryAsync("DELETE FROM dbo.Video WHERE link = @link", p);
        }

        private async void leaveButton_Click(object sender, RoutedEventArgs e)
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
            } else
            {
                IDictionary<string, object> p2 = new Dictionary<string, object>();
                p2.Add("@userID", userID);
                p2.Add("@code", this.lobbyCode);

                connection.runQueryAsync("DELETE FROM dbo.User_Lobby WHERE userID = @userID AND lobbyCode = @code", p2);
            }



            SqlDependency.Stop(connection.getConnectionString());


           
            Window.Current.Close();
  
            
        }
    }

    public class YouTubeJSON
    {
        public string title { get; set; }
    }

    public static class WebRequestExtensions
    {
        public static WebResponse GetResponseWithoutException(this WebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            try
            {
                return request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response == null)
                {
                    throw;
                }

                return e.Response;
            }
        }
    }

}

