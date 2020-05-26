using Service.Model;
using System.Collections.Generic;

namespace Service.Services.BacoService
{

    public struct Response
    {
        public bool Success { get; private set; }
        public object Message { get; private set; }

        public Response(bool success, object message)
        {
            Success = success;
            Message = message;
        }
    }

    public interface IBacoService
    {

        IEnumerable<dynamic> GetFriendsById(int? id);

        bool CheckHash(string nick, string password);

        int? GetId(string nick);

        Response PostUser(Users user);

        IEnumerable<Users> GetAllUsers();
        Response DeleteUser(int id);
        Response PutUser(int id, Users newData);
        bool MailAvailability(string mail);
        IEnumerable<dynamic> FindUser(string nick);
        IEnumerable<dynamic> GetSubscriptionsById(int id);
        IEnumerable<dynamic> GetGroups();
        IEnumerable<dynamic> GetGroups(int id);
        Response PostFriendPetition(int idPetitioner, int idRequested);
        IEnumerable<dynamic> GetFriendPetitions(int id);
        Response PutFriendPetition(int idPetitioner, int idRequested, bool accepted);
    }
}
