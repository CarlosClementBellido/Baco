using Baco.Api;
using Baco.ServerObjects;
using BacoServer.CommandLine.Printer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static Baco.Utils.WindowsUtils;
using static BacoServer.Api.ApiConn;
using static BacoServer.Server.Actions;

namespace BacoServer.Server
{
    public static class Server
    {

        private const int BUFFER_SIZE = 4096;

        internal static readonly IFormatter formatter = new BinaryFormatter();
        internal static TcpListener tcpListener;

        internal static Dictionary<int, List<SenderObjectRelation>> QueuedMessages = new Dictionary<int, List<SenderObjectRelation>>();

        internal static List<UserConnection> connectedUsers = new List<UserConnection>();
        public static List<Baco.ServerObjects.Group> groups = new List<Baco.ServerObjects.Group>();

        private const int portListen = 7854;
        private const int DEFAULT_SLEEP_MS = 1000;
        internal const string API_REST_BASE_URL = "127.0.0.1";

        internal static string apiResrBaseUrl = API_REST_BASE_URL;

        public static void StartServer()
        {
            GetGroups();
            IPAddress localAddr = IPAddress.Parse(GetLocalIPAddress());

            tcpListener = new TcpListener(localAddr, portListen);
            tcpListener.Start();

            Task.Run(() => ListenInfoAsync());
        }

        private static async void ListenInfoAsync()
        {
            Printer.WriteLine("Running");
            while (true)
            {
                while (tcpListener.Pending())
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    ServerObject data;
                    try
                    {
                        data = (ServerObject)formatter.Deserialize(tcpClient.GetStream());
                    }
                    catch (IOException e)
                    {
                        ErrorEventHandler(e, tcpClient);
                        continue;
                    }

                    CheckServerFlag(data, tcpClient);

                    _ = Task.Run(() => ListenConnectedUser(tcpClient));
                }
                await Task.Delay(DEFAULT_SLEEP_MS);
            }
        }

        private static void ListenConnectedUser(TcpClient tcpClient)
        {
            tcpClient.ReceiveBufferSize = BUFFER_SIZE;
            tcpClient.SendBufferSize = BUFFER_SIZE;

            while (tcpClient.Connected)
            {
                try
                {
                    ServerObject data = (ServerObject)formatter.Deserialize(tcpClient.GetStream());

                    Task.Run(() => CheckServerFlag(data, tcpClient));
                }
                catch (IOException e)
                {
                    ErrorEventHandler(e, tcpClient);
                }
                catch (SerializationException e)
                {
                    ErrorEventHandler(e, tcpClient);
                }
                catch (DecoderFallbackException e)
                {
                    ErrorEventHandler(e, tcpClient);
                }
                catch (OutOfMemoryException e)
                {
                    ErrorEventHandler(e, tcpClient);
                }
                finally
                {
                    if (tcpClient.Connected)
                    {
                        tcpClient.GetStream().Flush();
                        byte[] buffer = new byte[BUFFER_SIZE];
                        while (tcpClient.GetStream().DataAvailable)
                            tcpClient.GetStream().Read(buffer, 0, buffer.Length);
                    }
                }
            }

            connectedUsers.SingleOrDefault(u => u.Client.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint)?.CheckConnection();
        }

        private static void ErrorEventHandler(Exception e, TcpClient tcpClient)
        {
            Printer.WriteLine($"\n\t{e.GetType().Name}: {e.Message}\n", Printer.PrintType.Warning);
            connectedUsers.SingleOrDefault(u => u.Client.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint)?.CheckConnection();
        }

        private static void CheckServerFlag(ServerObject data, TcpClient tcpClient)
        {
            if (data.DataIsCorrect())
                switch (data.ServerFlag)
                {
                    case ServerFlag.Connection:
                        Task.Run(() => Connection(data, tcpClient));
                        break;
                    case ServerFlag.CheckStoredMessages:
                        Task.Run(() => CheckStoredMessages((int)data.Data));
                        break;
                    case ServerFlag.ExitFromGroup:
                        Task.Run(() => ExitFromGroup(data));
                        break;
                    case ServerFlag.SendMessage:
                        Task.Run(() => SendMessage(data));
                        break;
                    case ServerFlag.CallTo:
                        Task.Run(() => CallTo(data, tcpClient));
                        break;
                    case ServerFlag.AddToGroup:
                        Task.Run(() => AddToGroup(data));
                        break;
                    case ServerFlag.DeclineCall:
                        Task.Run(() => CallDeclined(data));
                        break;
                    case ServerFlag.SendingData:
                        Task.Run(() => SendingData(data));
                        break;
                    case ServerFlag.FlipFlopMute:
                        Task.Run(() => FlipFlopMute(data));
                        break;
                    case ServerFlag.FlipFlopReceivingScreen:
                        Task.Run(() => FlipFlopReceivingScreen(data));
                        break;
                    case ServerFlag.ConnectionChecking:
                        Task.Run(() => ReceivingConnectionCheck(data));
                        break;
                    case ServerFlag.ApiConnection:
                        Task.Run(() => ApiRequest((ApiObject)data.Data, tcpClient));
                        break;
                    default:
                        break;
                }
        }

    }
}
