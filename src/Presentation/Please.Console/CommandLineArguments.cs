public class CommandLineArguments
{
    public string TaskDescription { get; }
    public string[] RawArguments { get; }
    public bool HasInput { get; }

    private CommandLineArguments(string[] args)
    {
        RawArguments = args ?? Array.Empty<string>();
        TaskDescription = string.Join(" ", RawArguments);
        HasInput = RawArguments.Length > 0;
    }

    public static CommandLineArguments Parse(string[] args) => new(args);
}
