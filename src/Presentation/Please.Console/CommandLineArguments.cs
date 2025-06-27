public class CommandLineArguments
{
    public string TaskDescription { get; }
    public string[] RawArguments { get; }
    public bool HasInput { get; }
    public bool IsInstallCommand { get; }
    public bool IsStatusCommand { get; }
    public bool IsVersionCommand { get; }
    public bool IsHelpCommand { get; }

    private CommandLineArguments(string[] args)
    {
        RawArguments = args ?? Array.Empty<string>();
        
        // Check for special commands
        var firstArg = args?.FirstOrDefault()?.ToLowerInvariant();
        IsInstallCommand = firstArg == "--install" || firstArg == "-i";
        IsStatusCommand = firstArg == "--status" || firstArg == "-s";
        IsVersionCommand = firstArg == "--version" || firstArg == "-v";
        IsHelpCommand = firstArg == "--help" || firstArg == "-h" || firstArg == "help";

        // If it's a special command, don't treat it as a task description
        if (IsInstallCommand || IsStatusCommand || IsVersionCommand || IsHelpCommand)
        {
            TaskDescription = string.Empty;
            HasInput = false;
        }
        else
        {
            TaskDescription = string.Join(" ", RawArguments);
            HasInput = RawArguments.Length > 0;
        }
    }

    public static CommandLineArguments Parse(string[] args) => new(args);
}
