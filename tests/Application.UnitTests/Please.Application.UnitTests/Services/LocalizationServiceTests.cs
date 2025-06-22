using TUnit;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using Please.Application.Services;
using Please.Domain.Interfaces;

namespace Please.Application.UnitTests.Services;

[TestFixture]
public class LocalizationServiceTests
{
    private ILocalizationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new LocalizationService();
    }

    [Test]
    public void get_string_returns_value_when_key_exists()
    {
        var value = _service.GetString("Generated");
        Assert.Equal("Script generated successfully", value);
    }

    [Test]
    public void get_string_returns_key_when_not_found()
    {
        var value = _service.GetString("UnknownKey");
        Assert.Equal("UnknownKey", value);
    }
}
