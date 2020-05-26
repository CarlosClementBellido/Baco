using System;
using static BacoServer.CommandLine.ErrorCodes;

namespace BacoServer.CommandLine.Commands
{
    class Clear : ICommand
    {

        private const string DESCRIPTION = "Clear the console window";

        public string Description { get => DESCRIPTION; }

        public int Run()
        {
            Console.Clear();
            return COMMAND_EXECUTION_SUCCESS;
        }
    }
}
