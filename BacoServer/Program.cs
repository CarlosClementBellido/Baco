using BacoServer.CommandLine.Printer;
using System;

namespace BacoServer
{
    class Program
    {
        public static object MesageBox { get; private set; }

        static void Main(string[] args)
        {
            GetApiRestUrl();
            Server.Server.StartServer();
            CommandLine.ComandLine.StartCLI();
        }

        private static void GetApiRestUrl()
        {
            bool connectionSucceded = false;
            while (!connectionSucceded)
            {
                Printer.Write($"API REST URL (leave it blank for '{Server.Server.apiResrBaseUrl}'): ");
                string url = Console.ReadLine();
                if (string.IsNullOrEmpty(url))
                    url = Server.Server.API_REST_BASE_URL;
                else
                    Server.Server.apiResrBaseUrl = url;
                try
                {
                    Api.ApiConn.GetGroups();
                }
                catch
                {
                    Printer.WriteLine($"Not valid url ({url})", Printer.PrintType.Error);
                    Server.Server.apiResrBaseUrl = Server.Server.API_REST_BASE_URL;
                    continue;
                }
                connectionSucceded = true;
                Server.Server.apiResrBaseUrl = url;
            }
        }
    }
}
