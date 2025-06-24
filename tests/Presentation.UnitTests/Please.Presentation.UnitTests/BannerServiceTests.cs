using Please.UI.Services;

namespace Please.Presentation.UnitTests;

public class BannerServiceTests
{
    [Fact]
    public void Test_banner_service_generates_please_banner_text()
    {
        // Arrange
        var bannerService = new BannerService();

        // Act
        var bannerText = bannerService.GetBanner();

        // Assert
        bannerText.ShouldNotBeNull();
        bannerText.ShouldNotBeEmpty();
        bannerText.ShouldContain("PLEASE");
    }

    [Fact]
    public void Test_banner_service_generates_multi_line_banner()
    {
        // Arrange
        var bannerService = new BannerService();

        // Act
        var bannerText = bannerService.GetBanner();

        // Assert
        var lines = bannerText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBeGreaterThan(1);
    }

    [Fact]
    public void Test_banner_service_generates_consistent_banner()
    {
        // Arrange
        var bannerService = new BannerService();

        // Act
        var firstCall = bannerService.GetBanner();
        var secondCall = bannerService.GetBanner();

        // Assert
        firstCall.ShouldBe(secondCall);
    }
}
