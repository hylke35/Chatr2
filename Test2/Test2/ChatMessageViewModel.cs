using System.Collections.ObjectModel;

namespace Test2
{
    public class ChatMessageViewModel
    {
        public ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
    }
}
