using Please.Domain.Enums;
using Please.Infrastructure.Repositories;
using Please.TestUtilities.Builders;

namespace Please.Infrastructure.UnitTests.Repositories;

public class ScriptRepositoryTests
{
    private readonly ScriptRepository _repository = new();

    [Fact]
    public async Task SaveScriptAsync_with_valid_script_response_should_save_successfully()
    {
        // Arrange
        var scriptResponse = new ScriptResponseBuilder()
            .WithScript("Get-ChildItem")
            .WithTask("list files")
            .WithProvider(ProviderType.OpenAi)
            .WithModel("gpt-4")
            .WithScriptType(ScriptType.PowerShell)
            .Build();

        // Act
        var result = await _repository.SaveScriptAsync(scriptResponse);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task GetLastScriptAsync_when_no_scripts_exist_should_return_null()
    {
        // Act
        var result = await _repository.GetLastScriptAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public async Task GetLastScriptAsync_when_scripts_exist_should_return_most_recent()
    {
        // Arrange
        var firstScript = new ScriptResponseBuilder()
            .WithScript("Get-Date")
            .WithTask("get current date")
            .Build();

        var secondScript = new ScriptResponseBuilder()
            .WithScript("Get-ChildItem")
            .WithTask("list files")
            .Build();

        await _repository.SaveScriptAsync(firstScript);
        await Task.Delay(10); // Ensure different timestamps
        await _repository.SaveScriptAsync(secondScript);

        // Act
        var result = await _repository.GetLastScriptAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Script.ShouldBe("Get-ChildItem");
        result.Value.TaskDescription.ShouldBe("list files");
    }

    [Fact]
    public async Task GetScriptHistoryAsync_when_no_scripts_exist_should_return_empty_collection()
    {
        // Act
        var result = await _repository.GetScriptHistoryAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetScriptHistoryAsync_when_scripts_exist_should_return_all_scripts_ordered_by_date()
    {
        // Arrange
        var firstScript = new ScriptResponseBuilder()
            .WithScript("Get-Date")
            .WithTask("get current date")
            .Build();

        var secondScript = new ScriptResponseBuilder()
            .WithScript("Get-ChildItem")
            .WithTask("list files")
            .Build();

        await _repository.SaveScriptAsync(firstScript);
        await Task.Delay(10);
        await _repository.SaveScriptAsync(secondScript);

        // Act
        var result = await _repository.GetScriptHistoryAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Count().ShouldBe(2);
        result.Value!.First().Script.ShouldBe("Get-ChildItem"); // Most recent first
        result.Value!.Last().Script.ShouldBe("Get-Date");
    }

    [Fact]
    public async Task GetScriptHistoryAsync_with_count_limit_should_return_limited_results()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var script = new ScriptResponseBuilder()
                .WithScript($"Script {i}")
                .WithTask($"task {i}")
                .Build();
            await _repository.SaveScriptAsync(script);
            await Task.Delay(10);
        }

        // Act
        var result = await _repository.GetScriptHistoryAsync(count: 3);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Count().ShouldBe(3);
    }

    [Fact]
    public async Task ClearHistoryAsync_should_remove_all_scripts()
    {
        // Arrange
        var script = new ScriptResponseBuilder()
            .WithScript("Get-ChildItem")
            .WithTask("list files")
            .Build();
        await _repository.SaveScriptAsync(script);

        // Act
        var result = await _repository.ClearHistoryAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var historyResult = await _repository.GetScriptHistoryAsync();
        historyResult.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task HasHistoryAsync_when_no_scripts_exist_should_return_false()
    {
        // Act
        var result = await _repository.HasHistoryAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeFalse();
    }

    [Fact]
    public async Task HasHistoryAsync_when_scripts_exist_should_return_true()
    {
        // Arrange
        var script = new ScriptResponseBuilder()
            .WithScript("Get-ChildItem")
            .WithTask("list files")
            .Build();
        await _repository.SaveScriptAsync(script);

        // Act
        var result = await _repository.HasHistoryAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }
}
