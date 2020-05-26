using static BacoServer.CommandLine.ErrorCodes;
using static BacoServer.Server.Server;

namespace BacoServer.CommandLine.Commands
{
    class Restart : ICommand
    {

        private const string DESCRIPTION = "Kicks all connected users and restart the listener";

        public string Description { get => DESCRIPTION; }

        public int Run()
        {
            int connected = connectedUsers.Count;
            connectedUsers.Clear();
            Printer.Printer.WriteLine($"{connected} users kicked");
            tcpListener.Stop();
            tcpListener.Start();
            return COMMAND_EXECUTION_SUCCESS;
        }
    }
}
