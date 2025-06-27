using Shouldly;
using Xunit;

namespace Please.Presentation.UnitTests;

public class CommandLineArgumentsTests
{
    [Fact]
    public void Parse_with_multiple_arguments_creates_spaced_task_description()
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
    public void Parse_with_single_argument_returns_same_as_task_description()
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
    public void Parse_with_empty_arguments_returns_empty_task_description()
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
    public void Parse_with_null_arguments_handles_gracefully()
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
    public void Parse_with_special_characters_preserves_them()
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
    public void Parse_with_whitespace_only_arguments_preserves_them()
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
    public void Parse_with_various_nonsensical_input_accepts_everything()
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

    [Fact]
    public void Parse_with_auto_execute_flag_sets_IsAutoExecuteCommand()
    {
        // Arrange
        var args = new[] { "say", "hello", "--auto-execute" };

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.IsAutoExecuteCommand.ShouldBeTrue();
        arguments.TaskDescription.ShouldBe("say hello");
        arguments.HasInput.ShouldBeTrue();
    }

    [Fact]
    public void Parse_with_auto_execute_short_flag_sets_IsAutoExecuteCommand()
    {
        // Arrange
        var args = new[] { "create", "file", "-x" };

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.IsAutoExecuteCommand.ShouldBeTrue();
        arguments.TaskDescription.ShouldBe("create file");
        arguments.HasInput.ShouldBeTrue();
    }

    [Fact]
    public void Parse_without_auto_execute_flag_leaves_IsAutoExecuteCommand_false()
    {
        // Arrange
        var args = new[] { "get", "current", "time" };

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.IsAutoExecuteCommand.ShouldBeFalse();
        arguments.TaskDescription.ShouldBe("get current time");
        arguments.HasInput.ShouldBeTrue();
    }

    [Fact]
    public void Parse_with_auto_execute_flag_filters_it_from_task_description()
    {
        // Arrange
        var args = new[] { "--auto-execute", "list", "files", "-x", "in", "directory" };

        // Act
        var arguments = CommandLineArguments.Parse(args);

        // Assert
        arguments.IsAutoExecuteCommand.ShouldBeTrue();
        arguments.TaskDescription.ShouldBe("list files in directory");
        arguments.HasInput.ShouldBeTrue();
    }
}
