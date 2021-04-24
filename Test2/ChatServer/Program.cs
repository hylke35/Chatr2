using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:5000";
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("The Server URL is: {0}", url);
                Console.ReadLine();
            }
        }
    }

    class Startup
    {
        public void Configuration(IAppBuilder MyApp)
        {
            MyApp.MapSignalR();
        }
    }

    public class MyHub : Hub
    {
        public void Send(ChatMessage message)
        {
            Clients.All.broadcastMessage(message);
        }
    }
}