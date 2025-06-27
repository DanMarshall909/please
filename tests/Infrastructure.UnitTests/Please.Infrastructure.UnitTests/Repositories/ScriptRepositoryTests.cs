using Please.Infrastructure.Repositories;
using Please.TestUtilities.Builders;
using Please.Domain.Services;
using NSubstitute;

namespace Please.Infrastructure.UnitTests.Repositories;

public class ScriptRepositoryTests
{
    private static IPlatformService CreateMockPlatformService()
    {
        var mock = Substitute.For<IPlatformService>();
        mock.GetDataDirectory().Returns(Path.GetTempPath());
        return mock;
    }
    [Fact]
    public async Task SaveScriptAsync_with_valid_script_returns_success()
    {
        // Arrange
        var repository = new ScriptRepository(CreateMockPlatformService());
        var script = ScriptResponseBuilder.Create()
            .WithScript("Get-ChildItem")
            .Build();

        // Act
        var result = await repository.SaveScriptAsync(script);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task SaveScriptAsync_with_null_script_returns_failure()
    {
        // Arrange
        var repository = new ScriptRepository(CreateMockPlatformService());

        // Act
        var result = await repository.SaveScriptAsync(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Message.ShouldBe("Script response cannot be null");
    }

    [Fact]
    public async Task GetLastScriptAsync_with_existing_scripts_returns_most_recent()
    {
        // Arrange
        var repository = new ScriptRepository(CreateMockPlatformService());
        var script1 = ScriptResponseBuilder.Create()
            .WithScript("Get-Process")
            .Build();
        var script2 = ScriptResponseBuilder.Create()
            .WithScript("Get-Service")
            .Build();

        await repository.SaveScriptAsync(script1);
        await Task.Delay(10); // Ensure different timestamps
        await repository.SaveScriptAsync(script2);

        // Act
        var result = await repository.GetLastScriptAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Script.ShouldBe("Get-Service");
    }

    [Fact]
    public async Task GetLastScriptAsync_with_no_scripts_returns_null()
    {
        // Arrange
        var repository = new ScriptRepository(CreateMockPlatformService());

        // Act
        var result = await repository.GetLastScriptAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public async Task GetScriptHistoryAsync_returns_scripts_in_reverse_chronological_order()
    {
        // Arrange
        var repository = new ScriptRepository(CreateMockPlatformService());
        var script1 = ScriptResponseBuilder.Create()
            .WithScript("Script1")
            .Build();
        var script2 = ScriptResponseBuilder.Create()
            .WithScript("Script2")
            .Build();

        await repository.SaveScriptAsync(script1);
        await Task.Delay(10); // Ensure different timestamps
        await repository.SaveScriptAsync(script2);

        // Act
        var result = await repository.GetScriptHistoryAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Count().ShouldBe(2);
        result.Value!.First().Script.ShouldBe("Script2"); // Most recent first
        result.Value!.Last().Script.ShouldBe("Script1");
    }

    [Fact]
    public async Task GetScriptHistoryAsync_with_count_limit_returns_correct_number()
    {
        // Arrange
        var repository = new ScriptRepository(CreateMockPlatformService());
        for (var i = 0; i < 5; i++)
        {
            var script = ScriptResponseBuilder.Create()
                .WithScript($"Script{i}")
                .Build();
            await repository.SaveScriptAsync(script);
        }

        // Act
        var result = await repository.GetScriptHistoryAsync(3);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Count().ShouldBe(3);
    }

    [Fact]
    public async Task GetScriptHistoryAsync_with_since_filter_returns_scripts_after_date()
    {
        // Arrange
        var repository = new ScriptRepository(CreateMockPlatformService());
        var cutoffDate = DateTime.UtcNow.AddMinutes(-1);

        var oldScript = ScriptResponseBuilder.Create()
            .WithScript("OldScript")
            .WithCreatedAt(DateTime.UtcNow.AddMinutes(-2))
            .Build();
        var newScript = ScriptResponseBuilder.Create()
            .WithScript("NewScript")
            .WithCreatedAt(DateTime.UtcNow)
            .Build();

        await repository.SaveScriptAsync(oldScript);
        await repository.SaveScriptAsync(newScript);

        // Act
        var result = await repository.GetScriptHistoryAsync(since: cutoffDate);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Count().ShouldBe(1);
        result.Value!.First().Script.ShouldBe("NewScript");
    }

    [Fact]
    public async Task HasHistoryAsync_with_no_scripts_returns_false()
    {
        // Arrange
        var repository = new ScriptRepository(CreateMockPlatformService());

        // Act
        var result = await repository.HasHistoryAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeFalse();
    }

    [Fact]
    public async Task HasHistoryAsync_with_scripts_returns_true()
    {
        // Arrange
        var repository = new ScriptRepository(CreateMockPlatformService());
        var script = ScriptResponseBuilder.Create()
            .WithScript("Get-Process")
            .Build();
        await repository.SaveScriptAsync(script);

        // Act
        var result = await repository.HasHistoryAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task ClearHistoryAsync_removes_all_scripts()
    {
        // Arrange
        var repository = new ScriptRepository(CreateMockPlatformService());
        var script1 = ScriptResponseBuilder.Create().WithScript("Script1").Build();
        var script2 = ScriptResponseBuilder.Create().WithScript("Script2").Build();
        await repository.SaveScriptAsync(script1);
        await repository.SaveScriptAsync(script2);

        // Act
        var clearResult = await repository.ClearHistoryAsync();
        var hasHistoryResult = await repository.HasHistoryAsync();

        // Assert
        clearResult.IsSuccess.ShouldBeTrue();
        hasHistoryResult.IsSuccess.ShouldBeTrue();
        hasHistoryResult.Value.ShouldBeFalse();
    }
}
