using System.Collections.Generic;
using System.Linq;

namespace ChatServer
{
    class MessageHub : Hub
    {
        static List<User> ConnectedUsers = new List<User>();
        // When connecting to the SignalR server, the user is added to the list of users created above this line.
        public void Connect(string user)
        {
            var id = Context.ConnectionId;

            if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {

                ConnectedUsers.Add(new User { ConnectionId = id, UserName = user });

                Clients.Caller.onConnected(id, user, ConnectedUsers);

                Clients.AllExcept(id).onNewUserConnected(id, user);
            }

        }
        // Send a message to all the connected clients.
        public void SendMessage(ChatMessage message)
        {
            Clients.All.broadcastMessage(message);
        }

        // Send a private message to a specific user using their assigned ID.
        public void SendPrivateMessage(string toUserId, string message)
        {
            string fromUserId = Context.ConnectionId;

            var toUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == toUserId);
            var fromUser = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == fromUserId);

            if (toUser != null && fromUser != null)
            {
                Clients.Client(toUserId).sendPrivateMessage(fromUserId, fromUser.UserName, message);

                Clients.Caller.sendPrivateMessage(toUserId, fromUser.UserName, message);
            }

        }
    }
}
