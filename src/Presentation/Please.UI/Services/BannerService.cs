namespace Please.UI.Services;

public class BannerService
{
    private readonly string _banner = @"██████╗ ██╗     ███████╗ █████╗ ███████╗███████╗
██╔══██╗██║     ██╔════╝██╔══██╗██╔════╝██╔════╝
██████╔╝██║     █████╗  ███████║███████╗█████╗
██╔═══╝ ██║     ██╔══╝  ██╔══██║╚════██║██╔══╝
██║     ███████╗███████╗██║  ██║███████║███████╗
╚═╝     ╚══════╝╚══════╝╚═╝  ╚═╝╚══════╝╚══════╝";

    public string GetBanner()
    {
        var title = "PLEASE - PowerShell Script Generator";
        var subtitle = "Generate scripts with natural language";

        return $"{_banner}\n{title}\n{subtitle}";
    }
}
