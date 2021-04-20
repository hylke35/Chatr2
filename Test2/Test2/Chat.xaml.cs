using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Test2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Chat : Page
    {
        string username = "bob";
        string message;
        DateTime time;

        public Chat()
        {
            this.InitializeComponent();
        }

        static void Connect(string server, string message, int userID, string username, DateTime time)
        {
            try
            {
                Int32 port = 5000;
                TcpClient client = new TcpClient(server, port);
                NetworkStream stream = client.GetStream();

                // Translate the Message into ASCII.
                Byte[] data = Encoding.ASCII.GetBytes(message);
                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Sent: {0}", message);
                Thread.Sleep(2000);
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            Console.Read();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Connecting to server");
            message = sendBox.Text;
            time = DateTime.Now;
            Connect("127.0.0.1", message, 1, username, time);
            Console.WriteLine("Connected");
        }
    }
}
