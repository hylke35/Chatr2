using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

namespace Server
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool isRunning { get; set; }
        private int usersConnected { get; set; }
        private TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
        private TcpClient client;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void UpdateLog(string m)
        {
            logBox.Text += m;
        }

        public void StartServer()
        {
            listener.Start();
            isRunning = true;
            UpdateLog("Server started at " + listener.LocalEndpoint);
            UpdateLog("Waiting for Clients");
            StartListener();
            Debug.WriteLine("connected to: " + listener.LocalEndpoint);

        }

        public void StartListener()
        {
            try
            {
                while (true)
                {
                    Debug.WriteLine("j");
                    TcpClient client = listener.AcceptTcpClient();
                    Debug.WriteLine("a");
                    UpdateLog("Client connected!");
                    Thread t = new Thread(new ParameterizedThreadStart(ReceiveMessage));
                    Debug.WriteLine("b");
                    t.Start(client);
                    
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                listener.Stop();
            }
        }

        public void ReceiveMessage(Object obj)
        {
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();
            Byte[] bytes = new Byte[256];
            int i;
            try
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    string data = Encoding.ASCII.GetString(bytes, 0, i);
                    UpdateLog(data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
                client.Close();
            }
        }

        private void startButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (isRunning == true)
            {
                UpdateLog("Server is already running!");
            }
            else
            {
                StartServer();
                Debug.WriteLine("yes");
            }

        }

        private void stopButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (isRunning == false)
            {
                UpdateLog("Server is not running!");
            }
            else
            {
                listener.Stop();
                isRunning = false;
                UpdateLog("Server Stopped");
            }

        }
    }
}
