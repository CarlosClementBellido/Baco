using static BacoServer.CommandLine.ErrorCodes;
using static BacoServer.Server.Server;

namespace BacoServer.CommandLine.Commands
{
    class Status : ICommand
    {

        private const string DESCRIPTION = "Displays data available in listener and connected users with response time";

        public string Description { get => DESCRIPTION; }

        public int Run()
        {
            int countConnectedUsers = connectedUsers.Count;
            Printer.Printer.WriteLine($"Connected users: {countConnectedUsers}");
            if (countConnectedUsers != 0)
                connectedUsers.ForEach(u => Printer.Printer.WriteLine($"\t{u.Id} IPStatus: {u.Ping()}ms"));

            return COMMAND_EXECUTION_SUCCESS;
        }
    }
}
