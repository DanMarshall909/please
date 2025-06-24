using Please.UI.Services;

namespace Please.UI;

class Program
{
    static void Main(string[] args)
    {
        var bannerService = new BannerService();
        var colorService = new ColorService();

        // Display the banner
        Console.WriteLine(bannerService.GetBanner());
        Console.WriteLine();

        // Demonstrate colors
        Console.WriteLine("Color Demonstration:");
        var colors = new[] { "Red", "Green", "Blue", "Yellow", "Cyan", "Purple" };

        foreach (var colorName in colors)
        {
            var colorCode = colorService.GetColor(colorName);
            var resetCode = colorService.GetResetColor();
            Console.WriteLine($"{colorCode}● {colorName} colored text{resetCode}");
        }

        Console.WriteLine();
        Console.WriteLine("UI Walking Skeleton Demo Complete!");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
