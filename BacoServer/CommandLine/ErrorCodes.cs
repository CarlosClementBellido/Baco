namespace BacoServer.CommandLine
{
    public static class ErrorCodes
    {
        /// <summary>
        /// Successfull execution of the command
        /// </summary>
        public const int COMMAND_EXECUTION_SUCCESS = 1;
        /// <summary>
        /// Successfull execution finalizer command
        /// </summary>
        public const int COMMAND_EXIT = 0;
        /// <summary>
        /// Command fatal error. Abort program
        /// </summary>
        public const int COMMAND_FATAL_ERROR = -1;
        /// <summary>
        /// Command error
        /// </summary>
        public const int COMMAND_INTERNAL_ERROR = -2;

    }
}
