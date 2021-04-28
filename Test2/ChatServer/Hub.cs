using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;

namespace ChatServer
{
    public abstract class Hub
    {
        public HubConnectionContext Clients { get; set; }
        public HubCallerContext Context { get; set; }
        public IGroupManager Groups { get; set; }
    }
}
