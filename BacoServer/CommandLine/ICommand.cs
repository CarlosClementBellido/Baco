namespace BacoServer.CommandLine
{
    interface ICommand
    {
        string Description { get; }

        int Run();

    }
}
