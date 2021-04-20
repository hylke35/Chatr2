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


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Test2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoLobby : Page
    {
        MainPage mainPage;
        string lobbyCode;
        DBConnect connection = new DBConnect();

        
        public VideoLobby()
        {
            this.InitializeComponent();
            this.lobbyCode = "h3K5Lr";
            SqlDependency.Start(connection.getConnectionString());
            changedState();
            changedStateUsers();
            
        }

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

        private async void OnDependencyChangeUsers(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && e.Info == SqlNotificationInfo.Insert)
            {

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    this.userList.Items.Clear();

                    IEnumerable<object> list2 = await connection.DataReader("SELECT dbo.Users.username FROM dbo.User_Lobby, dbo.Users WHERE dbo.User_Lobby.lobbyCode = '" + this.lobbyCode + "' AND dbo.User_Lobby.userID = dbo.Users.userID", "Users", null);

                    List<object> test2 = list2.ToList();

                    foreach (User l in test2)
                    {
                        userList.Items.Add(l.Username);
                    }
                }
                );
            }
            SqlDependency dependency = sender as SqlDependency;
            changedStateUsers();
        }

        private async void OnDependencyChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change && e.Info == SqlNotificationInfo.Insert)
            {

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    this.videoList.Items.Clear();

                    IEnumerable<object> list2 = await connection.DataReader("SELECT * FROM Video", "Video", null);

                    List<object> test2 = list2.ToList();

                    foreach (Video l in test2)
                    {
                        videoList.Items.Add(l.Link);
                    }
                }
                );
            }
            SqlDependency dependency = sender as SqlDependency;
            changedState();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var link = linkBox.Text;
            if (Uri.IsWellFormedUriString(link, UriKind.Absolute) && (link.Contains("youtube.com") || link.Contains("youtu.be")))
            {
                IDictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@link", link);

                connection.runQueryAsync("INSERT INTO dbo.Video(link, runTime, currentTime, isPaused) VALUES (@link, '00:00:00', '00:00:00', 1)", p);
            }
        }
    }
}
