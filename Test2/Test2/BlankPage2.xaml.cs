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


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Test2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage2 : Page
    {
        MainPage mainPage;
        DBConnect connection;
        public BlankPage2()
        {
            this.InitializeComponent();
            connection = new DBConnect();
            connection.OpenConnection();
        }
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var link = linkBox.Text;
            string sqlString = "Insert into [Video] (link) VALUES(@link)";
            SqlCommand command = new SqlCommand(sqlString, connection.getCon());


            if (Uri.IsWellFormedUriString(link, UriKind.Absolute)) {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(link);
                request.Method = "HEAD";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.ResponseUri.ToString().Contains("youtube.com") || response.ResponseUri.ToString().Contains("youtu.be"))
                {
                    command.Parameters.AddWithValue("@link", link);
                    command.ExecuteNonQuery();
                    //await mainPage.downloadVideoAsync(linkBox.Text);
                }
                else
                {
                    Debug.WriteLine("didn't work");
                }
            }
            else
            {
                Debug.WriteLine("didn't work");
            }
        }
    }
}
