using Microsoft.Owin.Hosting;
using Owin;
using System;

namespace ChatServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            string url = "http://51.116.224.130:5000";
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("The Server URL is: {0}", url);
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder MyApp)
        {
            MyApp.MapSignalR();
        }
    }
}