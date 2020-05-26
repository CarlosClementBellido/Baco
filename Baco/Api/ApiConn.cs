using Baco.Beautifiers.LoadingNotificator;
using Baco.ServerObjects;
using Baco.Windows.RSSWindow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

using static Baco.Beautifiers.EnumerationDescriptor.EnumeratorDescriptor;

namespace Baco.Api
{
    /// <summary>
    /// Gives the response of the API when query is completed
    /// </summary>
    [Serializable]
    public struct ApiResponse
    {
        /// <summary>
        /// Query HTTP status code
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }
        /// <summary>
        /// Query given phrase of the query
        /// </summary>
        public string ReasonPhrase { get; private set; }

        /// <summary>
        /// Creates the understandable response of the API
        /// </summary>
        /// <param name="statusCode">Stores the HTTP query status code</param>
        /// <param name="reasonPhrase">Stores the reason given within the status</param>
        public ApiResponse(HttpStatusCode statusCode, string reasonPhrase)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
        }
    }

    /// <summary>
    /// Makes an standard for communicating inside the application
    /// </summary>
    public static class ApiConn
    {
        private const int INTERVAL_BETWEEN_PETITIONS_MS = 100;

        private static readonly Queue<ServerObject> ServerQueue = new Queue<ServerObject>();
        private static readonly Timer timer;
        private static bool PetitionInProgress = false;

        /// <summary>
        /// Initializes the class subscribing to all its events
        /// </summary>
        static ApiConn()
        {
            // Event-subscription for notificating when API gives a response. This tells us when an API-action is done

            Client.IdRetrieved += IdRetrieved;
            Client.HashCheckingRetrieved += HashCheckingRetrieved;
            Client.UserPostFeedback += UserPostFeedback;
            Client.NickCheckingRetrieved += NickCheckingRetrieved;
            Client.MailAvailabilityRetrieved += MailAvailabilityRetrieved;
            Client.UsersRetrieved += UsersRetrieved;
            Client.RSSSubscriptionsRetrieved += RSSSubscriptionsRetrieved;
            Client.ProfileImageUpdateFeedback += ProfileImageUpdateFeedback;
            Client.GetProfilePicture += GetProfilePicture;
            Client.FriendsRetrieved += FriendsRetrieved;
            Client.GroupsRetrieved += GroupsRetrieved;
            Client.PetitionsRetrieved += PetitionsRetrieved;
            Client.SendPetitionFeedback += SendPetitionFeedback;

            timer = new Timer
            {
                Interval = INTERVAL_BETWEEN_PETITIONS_MS
            };
            timer.Elapsed += TimerIntervalElapsed;
        }

        #region DATA RETRIEVED FROM API - EVENTS

        private static void SendPetitionFeedback(ApiResponse apiResponse)
        {
            DataRetrievedHandler();
        }

        private static void PetitionsRetrieved(List<User> petitioners)
        {
            DataRetrievedHandler();
        }

        private static void GroupsRetrieved(List<Group> groups)
        {
            DataRetrievedHandler();
        }

        private static void FriendsRetrieved(List<User> friends)
        {
            DataRetrievedHandler();
        }

        private static void GetProfilePicture(Image image)
        {
            DataRetrievedHandler();
        }

        private static void ProfileImageUpdateFeedback(ApiResponse apiResponse)
        {
            DataRetrievedHandler();
        }

        private static void RSSSubscriptionsRetrieved(List<RSSChannel> channels)
        {
            DataRetrievedHandler();
        }

        private static void UsersRetrieved(List<User> usersFound)
        {
            DataRetrievedHandler();
        }

        private static void MailAvailabilityRetrieved(bool available)
        {
            DataRetrievedHandler();
        }

        private static void NickCheckingRetrieved(bool success)
        {
            DataRetrievedHandler();
        }

        private static void UserPostFeedback(ApiResponse apiResponse)
        {
            DataRetrievedHandler();
        }

        private static void HashCheckingRetrieved(bool success)
        {
            DataRetrievedHandler();
        }

        private static void IdRetrieved(int id)
        {
            DataRetrievedHandler();
        }

        #region HANDLER

        private static void DataRetrievedHandler()
        {
            PetitionInProgress = false;
            LoadingNotificator.LoadingFinished();
        }

        #endregion HANDLER

        #endregion DATA RETRIEVED FROM API - EVENTS

        /// <summary>
        /// Each time passes <see cref="INTERVAL_BETWEEN_PETITIONS_MS"/> check if there's any petition in progress and send new petition
        /// </summary>
        /// <param name="sender"> - </param>
        /// <param name="e"> - </param>
        private static void TimerIntervalElapsed(object sender, ElapsedEventArgs e)
        {
            if (!PetitionInProgress)
            {
                PetitionInProgress = true;
                if (ServerQueue.Count > 0)
                {
                    ServerObject serverObject = ServerQueue.Dequeue();

                    Debugging.StatusBar.SetStatus(((ApiObject)serverObject.Data).apiFlag.GetDescription());

                    if (ServerQueue.Where(q => ((ApiObject)q.Data).apiFlag == ((ApiObject)serverObject.Data).apiFlag).Count() == 0)
                    {
                        Client.SendToServer(serverObject);
                        if (LoadingNotificator.LoadingPetitions == 0)
                            LoadingNotificator.LoadingInitialized();
                    }
                    else
                        PetitionInProgress = false;
                }
                if (ServerQueue.Count == 0)
                {
                    timer.Stop();
                }

                PetitionInProgress = false;
            }
        }

        /// <summary>
        /// Adds the ServerObject to the queue to be sent
        /// </summary>
        /// <param name="serverObject">Object to be sent</param>
        private static void AddToQueue(ServerObject serverObject)
        {
            ServerQueue.Enqueue(serverObject);

            if (!timer.Enabled)
                timer.Start();
        }

        public static void GetFriends(int id)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.GetFriends, id)));
        }

        public static void GetGroups(int id)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.GetGroups, id)));
        }

        public static void GetSubscriptionsFromApiRest(int id)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.GetRSSSubscriptions, $"subscription/{id}")));
        }

        public static void CheckHashFromApiRest(string nick, string password)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.CheckHash, $"autentication/{nick}?hash={Uri.EscapeDataString(Convert.ToBase64String(GetHash(password)))}")));
        }

        public static void GetIdFromApiRest(string nick)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.GetId, $"id/{nick}")));
        }

        public static void GetProfilePictureFromApiRest(string nick)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.GetProfilePicture, $"image/{nick}")));
        }

        public static void CheckNickAvailability(string nick)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.CheckNickAvailability, $"id/{nick}")));
        }

        public static void CheckMailInUse(string mail)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.CheckMailInUse, $"mail/available/{mail}")));
        }

        public static void ApiRestPostRequest(string nick, string password, string mail)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.PostUser, new User(nick, Convert.ToBase64String(GetHash(password)), mail))));
        }

        public static void AcceptPetition(User user, bool accepted)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.AcceptPetition, $"friends/{user.Id}/{Client.Id}/{accepted.ToString()}")));
        }

        public static void SendPetition(User user)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.SendPetition, $"friends/{Client.Id}/{user.Id}")));
        }

        public static void FindUsers(string nick)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.FindUsers, $"find/{nick}?nick={Uri.EscapeDataString(nick)}")));
        }

        public static void ApiRestProfileUpdateRequestRequest(byte[] image)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.UpdateProfilePicture, new object[] { Client.Nickname, image })));
        }

        public static void GetPetitions(int id)
        {
            AddToQueue(new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.GetPetitions, $"petitions/{id}")));
        }

        private static byte[] GetHash(string password)
        {
            byte[] salt = new byte[16];
            byte[] unhashedBytes = Encoding.Unicode.GetBytes(string.Concat(salt, password));

            SHA256Managed sha256 = new SHA256Managed();
            byte[] hashedBytes = sha256.ComputeHash(unhashedBytes);

            return hashedBytes;
        }
    }
}
