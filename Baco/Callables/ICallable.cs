using Baco.Messaging;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Baco.ServerObjects
{
    public interface ICallable
    {

        string Name { get; set; }
        string Descriptor { get; set; }
        int Id { get; set; }
        Image Picture { get; set; }

        void Call();
        void SendMessage(string message);
        ObservableCollection<Message> GetChat();

    }
}
