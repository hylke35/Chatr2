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
using Windows.UI.Core;
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
        TcpClient _client;
        byte[] _buffer = new byte[4096];

        public Chat()
        {
            this.InitializeComponent();
            _client = new TcpClient();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Connect to the remote server. The IP address and port # could be
            // picked up from a settings file.
            _client.Connect("127.0.0.1", 54000);

            // Start reading the socket and receive any incoming messages
            _client.GetStream().BeginRead(_buffer, 0, _buffer.Length, Server_MessageReceived, null);
        }

        private async void Server_MessageReceived(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                // End the stream read
                var bytesIn = _client.GetStream().EndRead(ar);
                if (bytesIn > 0)
                {
                    // Create a string from the received data. For this server 
                    // our data is in the form of a simple string, but it could be
                    // binary data or a JSON object. Payload is your choice.
                    var tmp = new byte[bytesIn];
                    Array.Copy(_buffer, 0, tmp, 0, bytesIn);
                    var str = Encoding.ASCII.GetString(tmp);

                    // Any actions that involve interacting with the UI must be done
                    // on the main thread. This method is being called on a worker
                    // thread so using the form's BeginInvoke() method is vital to
                    // ensure that the action is performed on the main thread.

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        listBox.Items.Add(str);
                        listBox.SelectedIndex = listBox.Items.Count - 1;
                    }
                    );
                }

                // Clear the buffer and start listening again
                Array.Clear(_buffer, 0, _buffer.Length);
                _client.GetStream().BeginRead(_buffer, 0, _buffer.Length, Server_MessageReceived, null);
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            // Encode the message and send it out to the server.
            var msg = Encoding.ASCII.GetBytes(sendBox.Text);
            _client.GetStream().Write(msg, 0, msg.Length);

            // Clear the text box and set it's focus
            sendBox.Text = "";
        }
    }
}
