using Shouldly;
using Xunit;

namespace Please.Presentation.UnitTests;

public class CommandLineArgumentsTests
{
    [Fact]
    public void Creates_spaced_task_description_when_parsing_multiple_arguments()
    {
        // Arrange
        var args = new[] { "create", "a", "PowerShell", "script" };

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.TaskDescription.ShouldBe("create a PowerShell script");
        arguments.RawArguments.ShouldBe(args);
        arguments.HasInput.ShouldBeTrue();
    }

    [Fact]
    public void Preserves_single_argument_as_task_description()
    {
        // Arrange
        var args = new[] { "test" };

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.TaskDescription.ShouldBe("test");
        arguments.RawArguments.ShouldBe(args);
        arguments.HasInput.ShouldBeTrue();
    }

    [Fact]
    public void Returns_empty_task_description_when_no_arguments_provided()
    {
        // Arrange
        string[] args = Array.Empty<string>();

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.TaskDescription.ShouldBe("");
        arguments.RawArguments.ShouldBe(args);
        arguments.HasInput.ShouldBeFalse();
    }

    [Fact]
    public void Handles_null_arguments_gracefully()
    {
        // Arrange
        string[]? args = null;

        // Act
        var arguments = CommandLineArguments.Parse(args!);

        // Assert
        arguments.TaskDescription.ShouldBe("");
        arguments.RawArguments.ShouldBeEmpty();
        arguments.HasInput.ShouldBeFalse();
    }

    [Fact]
    public void Preserves_special_characters_in_task_description()
    {
        // Arrange
        var args = new[] { "list", "*.txt", "files", "with", "size", ">", "1MB" };

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.TaskDescription.ShouldBe("list *.txt files with size > 1MB");
        arguments.RawArguments.ShouldBe(args);
        arguments.HasInput.ShouldBeTrue();
    }

    [Fact]
    public void Preserves_whitespace_only_arguments_in_task_description()
    {
        // Arrange
        var args = new[] { " ", "  ", "test" };

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.TaskDescription.ShouldBe("     test");
        arguments.RawArguments.ShouldBe(args);
        arguments.HasInput.ShouldBeTrue();
    }

    [Fact]
    public void Accepts_nonsensical_input_as_valid_task_description()
    {
        // Arrange
        var args = new[] { "???", "!!!", "@#$%", "123", "abc" };

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.TaskDescription.ShouldBe("??? !!! @#$% 123 abc");
        arguments.RawArguments.ShouldBe(args);
        arguments.HasInput.ShouldBeTrue();
    }

    [Theory]
    [InlineData("--history")]
    [InlineData("-r")]
    [InlineData("history")]
    public void Recognizes_history_commands(string command)
    {
        // Arrange
        var args = new[] { command };

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.IsHistoryCommand.ShouldBeTrue();
        arguments.TaskDescription.ShouldBe("");
        arguments.HasInput.ShouldBeFalse();
    }

    [Fact]
    public void Recognizes_history_command_case_insensitively()
    {
        // Arrange
        var args = new[] { "HISTORY" };

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.IsHistoryCommand.ShouldBeTrue();
        arguments.TaskDescription.ShouldBe("");
        arguments.HasInput.ShouldBeFalse();
    }
}
