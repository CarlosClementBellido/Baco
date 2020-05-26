using Microsoft.EntityFrameworkCore;
using Service.Model;
using System.Collections.Generic;
using System.Linq;

namespace Service.Services.BacoService
{
    public class BacoService : IBacoService
    {

        private readonly BacoContext bacoContext = new BacoContext();

        public bool CheckHash(string nick, string password) => bacoContext.Users.AsNoTracking()
                                                                                .SingleOrDefault(u => u.Nick == nick && u.PassHash == password) != null;

        public int? GetId(string nick) => bacoContext.Users.AsNoTracking().SingleOrDefault(u => u.Nick == nick)?.Id;

        public IEnumerable<Users> GetAllUsers() => bacoContext.Users.AsNoTracking().ToList();

        public Response PostUser(Users user)
        {
            if (bacoContext.Users.AsNoTracking().Any(u => u.Nick == user.Nick))
                return new Response(false, $"Nick '{user.Nick}' already exists");
            else if (bacoContext.Users.AsNoTracking().Any(u => u.Mail == user.Mail))
                return new Response(false, $"Mail '{user.Mail}' already exists");

            bacoContext.Users.Add(user);
            bacoContext.SaveChanges();

            return new Response(true, $"Nick '{user.Nick}' ({user.Mail}) created");
        }

        public Response DeleteUser(int id)
        {
            Users user = bacoContext.Users.SingleOrDefault(u => u.Id == id);
            if (user == null)
                return new Response(false, $"Id '{id}' not found");
            else
            {
                bacoContext.Users.Remove(user);
                bacoContext.SaveChanges();
                return new Response(true, $"Id '{id}' deleted");
            }
        }

        public Response PutUser(int id, Users newData)
        {
            Users user = bacoContext.Users.SingleOrDefault(u => u.Id == id);
            if (user == null)
                return new Response(false, $"Id '{id}' not found");
            else if (newData.Id != 0 && newData.Id != id)
                return new Response(false, $"Id requested '{id}' different from new data '{newData.Id}'");
            else if (bacoContext.Users.AsNoTracking().Any(u => u.Nick == newData.Nick && id != u.Id))
                return new Response(false, $"New nick '{newData.Nick}' alredy exists");
            else if (bacoContext.Users.AsNoTracking().Any(u => u.Mail == newData.Mail && id != u.Id))
                return new Response(false, $"New mail '{newData.Mail}' is beign used");
            else
            {
                if (newData.Nick != null)
                    user.Nick = newData.Nick;
                if (newData.Mail != null)
                    user.Mail = newData.Mail;
                if (newData.PassHash != null)
                    user.PassHash = newData.PassHash;
                bacoContext.Users.Update(user);
                bacoContext.SaveChanges();
                return new Response(true, $"Id '{id}' updated");
            }
        }

        public bool MailAvailability(string mail) => !bacoContext.Users.AsNoTracking().Any(u => u.Mail == mail);

        public IEnumerable<dynamic> FindUser(string nick) => bacoContext.Users.AsNoTracking().Where(u => u.Nick.Contains(nick)).Select(u => new { u.Id, u.Nick, u.PassHash, u.Mail });


        public IEnumerable<dynamic> GetSubscriptionsById(int id) => bacoContext.RsschannelSubscriptions.AsNoTracking().Where(r => r.IdUser == id)
                                                                                                                        .Select(r => bacoContext.Rsschannels.AsNoTracking()
                                                                                                                            .Where(c => r.IdRsschannel == c.Id)
                                                                                                                        .Select(r2 => new { r2.Rss, r2.Name }))
                                                                                                                        .Select(o => o.FirstOrDefault());

        public IEnumerable<dynamic> GetGroups()
        {
            return bacoContext.GroupsRelations.AsNoTracking().Select(g => new { groupId = g.IdGroup, user = g.IdUserNavigation })
                .ToList()
                .GroupBy(g => g.groupId)
                .Select(g => new
                {
                    id = g.Key,
                    name = bacoContext.Groups.AsNoTracking().SingleOrDefault(n => n.Id == g.Key).Name,
                    users = g.Select(u => new { u.user.Id, u.user.Nick, u.user.PassHash, u.user.Mail })
                });
        }

        public IEnumerable<dynamic> GetGroups(int id)
        {
            return bacoContext.GroupsRelations.AsNoTracking().Select(g => new { groupId = g.IdGroup, user = g.IdUserNavigation })
                .ToList()
                .GroupBy(g => g.groupId)
                .Select(g => new
                {
                    id = g.Key,
                    name = bacoContext.Groups.AsNoTracking().SingleOrDefault(n => n.Id == g.Key).Name,
                    users = g.Select(u => new { u.user.Id, u.user.Nick, u.user.PassHash, u.user.Mail })
                })
                .Where(g => g.users.Any(u => u.Id == id));
        }

        public IEnumerable<dynamic> GetFriendsById(int? id)
        {
            return bacoContext.Friends.AsNoTracking().Where(f => f.Accepted == true)
                .Select(f => new { petitioner = f.IdPetitionerNavigation, acceptor = f.IdAcceptorNavigation })
                .Where(f => f.petitioner.Id == id || f.acceptor.Id == id)
                .Select(f => f.petitioner.Id == id ? f.acceptor : f.petitioner)
                .Select(f => new { f.Id, f.Nick, f.PassHash, f.Mail });
        }

        public Response PostFriendPetition(int idPetitioner, int idRequested)
        {
            if (bacoContext.Friends.AsNoTracking()
                .FirstOrDefault(f => ((f.IdAcceptor == idRequested) || (f.IdAcceptor == idPetitioner)) && ((f.IdPetitioner == idRequested) || (f.IdPetitioner == idPetitioner))) != null)
                return new Response(false, $"Already friends / already sent");

            Friends newFriend = new Friends
            {
                IdAcceptor = idRequested,
                IdPetitioner = idPetitioner,
                Accepted = null
            };

            bacoContext.Friends.Add(newFriend);
            bacoContext.SaveChanges();

            return new Response(true, $"Petition successfull");
        }

        public IEnumerable<dynamic> GetFriendPetitions(int id)
        {
            return bacoContext.Friends.AsNoTracking().Where(f => f.Accepted == null && f.IdPetitioner != id)
                .Select(f => new { petitioner = f.IdPetitionerNavigation, acceptor = f.IdAcceptorNavigation })
                .Where(f => f.petitioner.Id == id || f.acceptor.Id == id)
                .Select(f => f.petitioner.Id == id ? f.acceptor : f.petitioner)
                .Select(f => new { f.Id, f.Nick, f.PassHash, f.Mail });
        }

        public Response PutFriendPetition(int idPetitioner, int idRequested, bool accepted)
        {
            Friends friend = bacoContext.Friends.AsNoTracking().FirstOrDefault(f => f.Accepted == null && f.IdPetitioner == idPetitioner && f.IdAcceptor == idRequested);

            if (friend == null)
                return new Response(false, "Petition not found");

            friend.Accepted = accepted;

            bacoContext.Friends.Update(friend);
            bacoContext.SaveChanges();
            return new Response(true, $"Petition {(accepted ? "accepted" : "rejected")}");
        }
    }
}
