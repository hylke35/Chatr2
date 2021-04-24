using EasyEncryption;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Test2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        DBConnect connection = new DBConnect();
        public Login()
        {
            this.InitializeComponent();
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = userBox.Text;
            string password = passBox.Password.ToString();
            string hashedPassword = HashPassword(password);

            IDictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@username", username);
            p.Add("@password", hashedPassword);
            IEnumerable<object> selectUserList1 = await connection.DataReader("SELECT * FROM dbo.Users WHERE username = @username AND password = @password", "Users", p);

            List<object> list1 = selectUserList1.ToList();

            string databasePassword = null;
            int userID = 0;
            foreach (User l in list1)
            {
                databasePassword = l.Password;
                userID = l.UserId;
            }

            Debug.WriteLine(databasePassword);
            if (PasswordsMatch(password, databasePassword))
            {
                statusBox.Text = "Success, you will be redirected to the chat";

                var parameters = new LoginParams();
                parameters.UserId = userID;
                Debug.WriteLine(parameters.UserId);
                Frame.Navigate(typeof(VideoLobby), parameters);
            }
            else
            {
                statusBox.Text = "Failure, the username and password don't match!";
            }
        }

        private async void registerButton_Click(object sender, RoutedEventArgs e)
        {
            string username = userBox.Text;
            string password = passBox.Password.ToString();
            string hashedPassword = HashPassword(password);
            Debug.WriteLine(username + password);

            IDictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@username", username);
            IEnumerable<object> selectUserList1 = await connection.DataReader("SELECT * FROM dbo.Users WHERE username = @username", "Users", p);

            List<object> list1 = selectUserList1.ToList();

            string checkUsername = null;

            foreach (User l in list1)
            {
                checkUsername = l.Username;
            }


            if (checkUsername != username)
            {
                IDictionary<string, object> c = new Dictionary<string, object>();
                c.Add("@username", username);
                c.Add("@password", hashedPassword);
                connection.runQueryAsync("INSERT INTO dbo.Users (username, password) VALUES (@username, @password)", c);
                statusBox.Text = "Success, account with this username has been created!";

                IDictionary<string, object> b = new Dictionary<string, object>();
                b.Add("@username", username);
                IEnumerable<object> selectUserList2 = await connection.DataReader("SELECT * FROM dbo.Users WHERE username = @username", "Users", b);

                List<object> list2 = selectUserList2.ToList();

                int userID = 0;

                foreach (User l in list2)
                {
                    userID = l.UserId;
                }

                var parameters = new LoginParams();
                parameters.UserId = userID;
                Debug.WriteLine(parameters.UserId);
                Frame.Navigate(typeof(VideoLobby), parameters);
            }
            else
            {
                statusBox.Text = "Failure, account with this username already exists!";
            }

        }
        private static string HashPassword(string input)
        {
            return SHA.ComputeSHA256Hash(input);
        }

        private static bool PasswordsMatch(string userInput, string passwordString)
        {
            //Debug.WriteLine(userInput);
            string hashedInput = HashPassword(userInput);
            bool doPasswordsMatch = string.Equals(hashedInput, passwordString);
            return doPasswordsMatch;
        }
    }

    public class LoginParams
    {
        public LoginParams() { }
        public string LobbyCode { get; set; }
        public int UserId { get; set; }
    }
}
