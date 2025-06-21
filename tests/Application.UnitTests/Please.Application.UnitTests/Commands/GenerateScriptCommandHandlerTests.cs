using NUnit.Framework;
using Moq;
using Please.Application.Commands.GenerateScript;
using Please.Domain.Common;
using Please.Domain.Entities;
using Please.Domain.Enums;
using Please.Domain.Interfaces;

namespace Please.Application.UnitTests.Commands;

[TestFixture]
public class GenerateScriptCommandHandlerTests
{
    private Mock<IScriptGenerator> _mockScriptGenerator;
    private Mock<IScriptRepository> _mockScriptRepository;
    private GenerateScriptCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockScriptGenerator = new Mock<IScriptGenerator>();
        _mockScriptRepository = new Mock<IScriptRepository>();
        _handler = new GenerateScriptCommandHandler(_mockScriptGenerator.Object, _mockScriptRepository.Object);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldGenerateAndSaveScript()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("Deploy to production", ProviderType.OpenAI, "gpt-4");
        var expectedResponse = ScriptResponse.Create(
            "kubectl apply -f production.yaml",
            "Deploy to production",
            ProviderType.OpenAI,
            "gpt-4",
            ScriptType.Bash,
            RiskLevel.High
        );

        _mockScriptGenerator
            .Setup(x => x.GenerateScriptAsync(It.IsAny<ScriptRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScriptResponse>.Success(expectedResponse));

        _mockScriptRepository
            .Setup(x => x.SaveScriptAsync(It.IsAny<ScriptResponse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResponse));
        
        _mockScriptGenerator.Verify(
            x => x.GenerateScriptAsync(
                It.Is<ScriptRequest>(r => 
                    r.TaskDescription == command.TaskDescription && 
                    r.Provider == command.Provider &&
                    r.Model == command.Model
                ), 
                It.IsAny<CancellationToken>()
            ), 
            Times.Once
        );
        
        _mockScriptRepository.Verify(
            x => x.SaveScriptAsync(expectedResponse, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Test]
    public async Task Handle_WithMinimalCommand_ShouldSetWorkingDirectoryToCurrentDirectory()
    {
        // Arrange
        var command = GenerateScriptCommand.Create("List files");
        var expectedResponse = ScriptResponse.Create(
            "ls -la",
            "List files",
            ProviderType.OpenAI,
            "gpt-4",
            ScriptType.Bash
        );

        _mockScriptGenerator
            .Setup(x => x.GenerateScriptAsync(It.IsAny<ScriptRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ScriptResponse>.Success(expectedResponse));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockScriptGenerator.Verify(
            x => x.GenerateScriptAsync(
                It.Is<ScriptRequest>(r => 
                    r.WorkingDirectory == Environment.CurrentDirectory
                ), 
                It.IsAny<CancellationToken>()
            ), 
            Times.Once
        );
    }
}
