using System.Collections.Generic;
using System.Linq;

namespace ChatServer
{
    class MessageHub : Hub
    {
        static List<User> ConnectedUsers = new List<User>();
        public void Connect(string userName)
        {
            var id = Context.ConnectionId;

            if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {

                ConnectedUsers.Add(new User { ConnectionId = id, UserName = userName });

                // send to caller
                Clients.Caller.onConnected(id, userName, ConnectedUsers);

                // send to all except caller client
                Clients.AllExcept(id).onNewUserConnected(id, userName);
            }

        }
        public void SendMessage(ChatMessage message)
        {
            Clients.All.broadcastMessage(message);
        }

        public void SendPrivateMessage(string toUserId, string message)
        {
            string fromUserId = Context.ConnectionId;

            var toUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == toUserId);
            var fromUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == fromUserId);

            if (toUser != null && fromUser != null)
            {
                // send to
                Clients.Client(toUserId).sendPrivateMessage(fromUserId, fromUser.UserName, message);

                // send to caller user
                Clients.Caller.sendPrivateMessage(toUserId, fromUser.UserName, message);
            }

        }
    }
}
