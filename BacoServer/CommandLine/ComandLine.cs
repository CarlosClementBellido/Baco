using BacoServer.CommandLine.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using static BacoServer.CommandLine.ErrorCodes;

namespace BacoServer.CommandLine
{
    public static class ComandLine
    {

        internal static readonly Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>
        {
            { "help", new Help() },
            { "status", new Status() },
            { "cls", new Clear() },
            { "restart", new Restart() }
        };

        public static void StartCLI()
        {
            int commandReturn = 0;
            string command;

            do
            {

                command = Console.ReadLine();

                if (commands.ContainsKey(command))
                    commandReturn = commands.SingleOrDefault(p => p.Key == command).Value.Run();
                else
                    Printer.Printer.WriteLine($"'{command}' not recognized as command", Printer.Printer.PrintType.Error);


            } while (commandReturn != COMMAND_FATAL_ERROR || commandReturn != COMMAND_EXIT);

        }

    }
}
