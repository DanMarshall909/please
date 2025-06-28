public class CommandLineArguments
{
    public string TaskDescription { get; }
    public string[] RawArguments { get; }
    public bool HasInput { get; }
    public bool IsInstallCommand { get; }
    public bool IsStatusCommand { get; }
    public bool IsVersionCommand { get; }
    public bool IsHelpCommand { get; }
    public bool IsHistoryCommand { get; }
    public bool IsSearchCommand { get; }
    public string SearchQuery { get; }

    private CommandLineArguments(string[] args)
    {
        RawArguments = args ?? Array.Empty<string>();
        
        // Check for special commands
        var firstArg = args?.FirstOrDefault()?.ToLowerInvariant();
        IsInstallCommand = firstArg == "--install" || firstArg == "-i";
        IsStatusCommand = firstArg == "--status" || firstArg == "-s";
        IsVersionCommand = firstArg == "--version" || firstArg == "-v";
        IsHelpCommand = firstArg == "--help" || firstArg == "-h" || firstArg == "help";
        IsHistoryCommand = firstArg == "--history" || firstArg == "-r" || firstArg == "history";
        IsSearchCommand = firstArg == "--search" || firstArg == "-f" || firstArg == "search" || firstArg == "find";

        // Handle search command with query
        if (IsSearchCommand)
        {
            SearchQuery = args?.Length > 1 ? string.Join(" ", args.Skip(1)) : string.Empty;
            TaskDescription = string.Empty;
            HasInput = false;
        }
        // If it's a special command, don't treat it as a task description
        else if (IsInstallCommand || IsStatusCommand || IsVersionCommand || IsHelpCommand || IsHistoryCommand)
        {
            SearchQuery = string.Empty;
            TaskDescription = string.Empty;
            HasInput = false;
        }
        else
        {
            SearchQuery = string.Empty;
            TaskDescription = string.Join(" ", RawArguments);
            HasInput = RawArguments.Length > 0;
        }
    }

    public static CommandLineArguments Parse(string[] args) => new(args);
}
