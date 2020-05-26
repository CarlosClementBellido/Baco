using System.Collections.Generic;
using static BacoServer.CommandLine.ComandLine;
using static BacoServer.CommandLine.ErrorCodes;

namespace BacoServer.CommandLine.Commands
{
    class Help : ICommand
    {

        private const string DESCRIPTION = "Shows help about baco-server commands";

        public string Description { get => DESCRIPTION; }

        public int Run()
        {

            foreach (KeyValuePair<string, ICommand> keyValuePair in commands)
                Printer.Printer.WriteLine($"    - {keyValuePair.Key}: {keyValuePair.Value.Description}");

            return COMMAND_EXECUTION_SUCCESS;
        }
    }
}
