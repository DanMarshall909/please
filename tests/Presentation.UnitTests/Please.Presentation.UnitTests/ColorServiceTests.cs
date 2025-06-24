using Please.UI.Services;

namespace Please.Presentation.UnitTests;

public class ColorServiceTests
{
    [Fact]
    public void Test_color_service_provides_reset_color()
    {
        // Arrange
        var colorService = new ColorService();

        // Act
        var resetColor = colorService.GetResetColor();

        // Assert
        resetColor.ShouldNotBeNull();
        resetColor.ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData("Red")]
    [InlineData("Green")]
    [InlineData("Blue")]
    [InlineData("Yellow")]
    [InlineData("Cyan")]
    [InlineData("Purple")]
    public void Test_color_service_provides_named_colors(string colorName)
    {
        // Arrange
        var colorService = new ColorService();

        // Act
        var color = colorService.GetColor(colorName);

        // Assert
        color.ShouldNotBeNull();
        color.ShouldNotBeEmpty();
    }

    [Fact]
    public void Test_color_service_handles_unknown_color_gracefully()
    {
        // Arrange
        var colorService = new ColorService();

        // Act
        var color = colorService.GetColor("UnknownColor");

        // Assert
        color.ShouldBe(string.Empty);
    }
}
