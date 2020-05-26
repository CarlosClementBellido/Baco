using Baco.ServerObjects;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Baco.Dialogs.GroupCreationDialog
{
    class GroupCreationDialogVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string SearchBox { get; set; }
        public string GroupName { get; set; }
        public ObservableCollection<User> FoundFriends { get; set; }
        public ObservableCollection<User> SelectedFriends { get; set; }

        public User SelectedFriend { get; set; }

        public GroupCreationDialogVM()
        {
            FoundFriends = new ObservableCollection<User>();
            SelectedFriends = new ObservableCollection<User>();
        }

        internal void SearchBoxTextChanged()
        {
            FoundFriends = new ObservableCollection<User>(Client.Friends.Where(f => !SelectedFriends.Contains(f) && f.Nickname.Contains(SearchBox)));
        }

        internal void SelectedItem()
        {
            if (SelectedFriend != null)
            {
                SelectedFriends.Add(SelectedFriend);
                SearchBox = "";
                SearchBoxTextChanged();
            }
        }

        internal void CreateGroup()
        {
            Group.groups.Add(new Group(SelectedFriends.ToList(), GroupName));
        }
    }
}
