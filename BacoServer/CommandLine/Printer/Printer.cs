using System;
using System.Timers;

namespace BacoServer.CommandLine.Printer
{
    public static class Printer
    {

        private const int WARNING_ERRORS_PER_SECOND = 2;
        private const int MAX_ERRORS_PER_SECOND = 5;

        public enum PrintType
        {
            Info, Warning, Error
        }

        private static Timer TimerErrorsPerSecond;
        private static int ErrorsPerSecond;

        static Printer()
        {
            TimerErrorsPerSecond = new Timer
            {
                Interval = 1000
            };
            TimerErrorsPerSecond.Elapsed += TimerErrorsPerSecondElapsed;
            TimerErrorsPerSecond.Start();
        }

        private static void TimerErrorsPerSecondElapsed(object sender, ElapsedEventArgs e)
        {
            if (ErrorsPerSecond > MAX_ERRORS_PER_SECOND)
                WriteLine($"\n\t{ErrorsPerSecond} errors in last second", PrintType.Error);
            if (ErrorsPerSecond > WARNING_ERRORS_PER_SECOND)
                WriteLine($"\n\t{ErrorsPerSecond} errors in last second", PrintType.Warning);
            ErrorsPerSecond = 0;
        }

        public static void WriteLine(string text, PrintType printType = PrintType.Info)
        {
            switch (printType)
            {
                case PrintType.Info:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case PrintType.Warning:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case PrintType.Error:
                    ErrorsPerSecond++;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.WriteLine(text);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Write(string text, PrintType printType = PrintType.Info)
        {
            switch (printType)
            {
                case PrintType.Info:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case PrintType.Warning:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case PrintType.Error:
                    ErrorsPerSecond++;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.Write(text);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}