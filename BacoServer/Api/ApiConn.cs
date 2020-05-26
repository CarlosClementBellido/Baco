using Baco.Api;
using Baco.ServerObjects;
using Baco.Windows.RSSWindow;
using BacoServer.CommandLine.Printer;
using BacoServer.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

using System.Text;
using System.Threading.Tasks;
using static Baco.ServerObjects.User;
using static BacoServer.Server.Server;
using Group = Baco.ServerObjects.Group;

namespace BacoServer.Api
{
    public static class ApiConn
    {

        private static readonly HttpClient client = new HttpClient();

        public static async void ManageClientRequest(ApiObject data, TcpClient tcpClient)
        {

            Printer.WriteLine(data.apiFlag.ToString());
            switch (data.apiFlag)
            {
                case ApiFlag.Error:
                    break;
                case ApiFlag.CheckHash:
                    ManagePetition(tcpClient.GetStream(), ApiFlag.CheckHash, CheckHashAndMail((string)data.data));
                    break;
                case ApiFlag.GetId:
                    ManagePetition(tcpClient.GetStream(), ApiFlag.GetId, GetId((string)data.data));
                    break;
                case ApiFlag.CheckNickAvailability:
                    ManagePetition(tcpClient.GetStream(), ApiFlag.CheckNickAvailability, GetId((string)data.data) == -1);
                    break;
                case ApiFlag.CheckMailInUse:
                    ManagePetition(tcpClient.GetStream(), ApiFlag.CheckMailInUse, CheckHashAndMail((string)data.data));
                    break;
                case ApiFlag.FindUsers:
                    ManagePetition(tcpClient.GetStream(), ApiFlag.FindUsers, FindUser((string)data.data));
                    break;
                case ApiFlag.GetRSSSubscriptions:
                    ManagePetition(tcpClient.GetStream(), ApiFlag.GetRSSSubscriptions, GetRSSSubscriptions((string)data.data));
                    break;
                case ApiFlag.GetProfilePicture:
                    ManagePetition(tcpClient.GetStream(), ApiFlag.GetProfilePicture, GetProfilePicture((string)data.data));
                    break;
                case ApiFlag.GetFriends:
                    ManagePetition(tcpClient.GetStream(), ApiFlag.GetFriends, GetFriendsAndNotifyThem((int)data.data, ConnectionState.Connected));
                    break;
                case ApiFlag.GetGroups:
                    ManagePetition(tcpClient.GetStream(), ApiFlag.GetGroups, GetGroups((int)data.data));
                    break;
                case ApiFlag.GetPetitions:
                    ManagePetition(tcpClient.GetStream(), ApiFlag.GetPetitions, GetPetitions((string)data.data));
                    break;
                case ApiFlag.PostUser:
                    formatter.Serialize(tcpClient.GetStream(), new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.PostUser, await PostUserAsync((User)data.data))));
                    break;
                case ApiFlag.UpdateProfilePicture:
                    formatter.Serialize(tcpClient.GetStream(), new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.UpdateProfilePicture, await UpdateProfilePictureAsync((object[])data.data))));
                    break;
                case ApiFlag.AcceptPetition:
                    formatter.Serialize(tcpClient.GetStream(), new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.AcceptPetition, await AcceptPetitionAsync((string)data.data))));
                    break;
                case ApiFlag.SendPetition:
                    formatter.Serialize(tcpClient.GetStream(), new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.SendPetition, await SendPetitionAsync((string)data.data))));
                    break;
            }
        }

        private static async void ManagePetition(NetworkStream networkStream, ApiFlag flag, object result)
        {
            await Task.Delay(100);
            lock(networkStream)
            {
                formatter.Serialize(networkStream, new ServerObject(ServerFlag.ApiConnection, new ApiObject(flag, result)));
            }
        }

        #region CLIENT REQUESTS

        private static async Task<ApiResponse> SendPetitionAsync(string url)
        {
            HttpResponseMessage httpResponseMessage = await client.PostAsync(apiResrBaseUrl + url, null);
            return new ApiResponse(httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
        }

        private static async Task<ApiResponse> AcceptPetitionAsync(string url)
        {
            string[] urlParts = url.Split('/');
            bool accepted = bool.Parse(urlParts[3]);
            int idAcceptor = int.Parse(urlParts[1]);
            int idPetitioner = int.Parse(urlParts[2]);
            if (accepted)   // Notify both of them
            {
                UserConnection petitioner = connectedUsers.FirstOrDefault(u => u.Id == idPetitioner);
                if (petitioner != null)
                    formatter.Serialize(petitioner.Client.GetStream(), new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.GetFriends, GetFriendsAndNotifyThem(idPetitioner,
                        petitioner.ConnectionState.Value))));
                UserConnection acceptor = connectedUsers.FirstOrDefault(u => u.Id == idAcceptor);
                if (acceptor != null)
                    formatter.Serialize(acceptor.Client.GetStream(), new ServerObject(ServerFlag.ApiConnection, new ApiObject(ApiFlag.GetFriends, GetFriendsAndNotifyThem(idAcceptor,
                        acceptor.ConnectionState.Value))));

            }

            HttpResponseMessage httpResponseMessage = await client.PutAsync(apiResrBaseUrl + url, null);
            return new ApiResponse(httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
        }

        private static List<User> GetPetitions(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiResrBaseUrl + url);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return JsonConvert.DeserializeObject<List<User>>(reader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Printer.WriteLine(e.Message, Printer.PrintType.Error);
                return new List<User>();
            }
        }

        private static System.Drawing.Image GetProfilePicture(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiResrBaseUrl + url);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    System.Drawing.Image image = System.Drawing.Image.FromStream(stream);

                    return image;
                }
            }
            catch (Exception e)
            {
                Printer.WriteLine(e.Message, Printer.PrintType.Error);
            }
            return null;
        }

        private static async Task<ApiResponse> UpdateProfilePictureAsync(object[] data)
        {
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
            multipartFormDataContent.Add(new ByteArrayContent((byte[])data[1]), "image", "image");
            HttpResponseMessage httpResponseMessage = await client.PostAsync(apiResrBaseUrl + $"image/{(string)data[0]}", multipartFormDataContent);
            return new ApiResponse(httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
        }

        private static List<RSSChannel> GetRSSSubscriptions(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiResrBaseUrl + url);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return JsonConvert.DeserializeObject<List<RSSChannel>>(reader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Printer.WriteLine(e.Message, Printer.PrintType.Error);
                return new List<RSSChannel>();
            }
        }

        private static bool CheckHashAndMail(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiResrBaseUrl + url);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    bool result = bool.TryParse(reader.ReadToEnd(), out bool success);
                    return result && success;
                }
            }
            catch (Exception e)
            {
                Printer.WriteLine(e.Message, Printer.PrintType.Error);
            }
            return false;
        }

        private static int GetId(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiResrBaseUrl + url);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    //return int.TryParse(reader.ReadToEnd(), out int res) ? res : -1;
                    bool ok = int.TryParse(reader.ReadToEnd(), out int reading);
                    return reading == 0 ? -1 : reading;
                }
            }
            catch (Exception e)
            {
                Printer.WriteLine(e.Message, Printer.PrintType.Error);
            }
            return -2;
        }

        private static async Task<ApiResponse> PostUserAsync(User user)
        {
            HttpResponseMessage httpResponseMessage = await client.PostAsync(apiResrBaseUrl, new StringContent(user.ToString(), Encoding.UTF8, "application/json"));
            return new ApiResponse(httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
        }

        private static List<User> FindUser(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiResrBaseUrl + url);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return JsonConvert.DeserializeObject<List<User>>(reader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Printer.WriteLine(e.Message, Printer.PrintType.Error);
                return new List<User>();
            }
        }

        #endregion CLIENT REQUESTS

        #region CLIENT & SERVER REQUESTS

        public static List<User> GetFriendsAndNotifyThem(int id, ConnectionState connectionState)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiResrBaseUrl + $"friends/{id}");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                List<User> friends = JsonConvert.DeserializeObject<List<User>>(reader.ReadToEnd());
                friends.Select(f => new { user = connectedUsers.SingleOrDefault(u => u.Id == f.Id), friend = f })
                    .Where(f =>
                    {
                        bool connected = f.user != null;

                        if (connected)
                            f.friend.State = f.user.ConnectionState;

                        return connected;
                    })
                    .ToList()
                    .ForEach(u => formatter.Serialize(u.user.Client.GetStream(), new ServerObject(ServerFlag.ConnectionState, new KeyValuePair<ConnectionState, int>(connectionState, id))));

                return friends;
            }
        }

        internal static List<Group> GetGroups(int? data = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiResrBaseUrl + $"groups{(data == null ? "" : $"/user/{data}")}");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    List<Group> groups = JsonConvert.DeserializeObject<List<Group>>(reader.ReadToEnd());
                    if (data == null)
                        Server.Server.groups = groups;
                    return groups;
                }
            }
            catch (Exception e)
            {
                Printer.WriteLine(e.Message, Printer.PrintType.Error);
                throw e;
            }
        }

        #endregion CLIENT & SERVER REQUESTS

    }
}
