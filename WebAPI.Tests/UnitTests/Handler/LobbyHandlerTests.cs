using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using WebAPI.Handler;
using WebAPI.Interfaces.Repositories;
using WebAPI.Model.Exceptions;
using WebAPI.Model.Lobby;
using Xunit;

namespace WebAPI.Tests.UnitTests.Handler;

public class LobbyHandlerTests
{
    private readonly LobbyHandler _sut;

    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly ILobbyRepository _lobbyRepository = Substitute.For<ILobbyRepository>();

    public LobbyHandlerTests()
    {
        _sut = new LobbyHandler(_loggerFactory, _lobbyRepository);
    }

    [Fact]
    private async Task GetLobbyByPublicId_ReturnNull_WhenLobbyNotFound()
    {
        // Arrange
        const string publicId = "ABC123";
        _lobbyRepository.GetLobbyByPublicId(publicId).ReturnsNull();

        // Act
        var result = await _sut.GetLobbyByPublicId(publicId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    private async Task GetLobbyByPublicId_ReturnLobby_WhenLobbyWasFound()
    {
        // Arrange
        var lobbyId = Guid.NewGuid();
        const string publicId = "ABC123";
        var adminGuid = Guid.NewGuid();
        const int maxPlayer = 4;
        var creationTime = DateTime.UtcNow;
        var currentPlayer = new List<LobbyPlayer>();
        Guid? currentSessionUId = null;

        var lobbyDb = new LobbyDb(lobbyId, publicId, adminGuid, maxPlayer, creationTime, currentSessionUId,
            currentPlayer);
        var lobbyDto = new LobbyDto(publicId, adminGuid, maxPlayer, creationTime, currentPlayer, null);
        _lobbyRepository.GetLobbyByPublicId(publicId).Returns(lobbyDb);

        // Act
        var result = await _sut.GetLobbyByPublicId(publicId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(lobbyDto);
    }

    [Fact]
    private async Task CreateLobby_Throw400Exception_WithToFewPlayers()
    {
        // Arrange
        const string exMsg = "Lobby must have between 2 and 10 players";

        var creatorGuid = Guid.NewGuid();
        var settings = new LobbySettings(LobbyHandler.MinPlayers - 1);

        // Act
        var createLobbyTask = _sut.Invoking(lobbyHandler => lobbyHandler.CreateLobby(creatorGuid, settings));

        // Assert
        createLobbyTask
            .Should()
            .ThrowAsync<StatusCodeException>()
            .Where(ex =>
                ex.Message == exMsg && ex.StatusCode == HttpStatusCode.BadRequest
            );
    }

    [Fact]
    private async Task CreateLobby_Throw400Exception_WithToManyPlayers()
    {
        // Arrange
        const string exMsg = "Lobby must have between 2 and 10 players";

        var creatorGuid = Guid.NewGuid();
        var settings = new LobbySettings(LobbyHandler.MaxPlayers + 1);

        // Act
        var createLobbyTask = _sut.Invoking(lobbyHandler => lobbyHandler.CreateLobby(creatorGuid, settings));

        // Assert
        createLobbyTask
            .Should()
            .ThrowAsync<StatusCodeException>()
            .Where(ex =>
                ex.Message == exMsg && ex.StatusCode == HttpStatusCode.BadRequest
            );
    }

    [Fact]
    private async Task CreateLobby_Throw500_WhenNoPublicIdWasFound()
    {
        // Arrange
        var settings = new LobbySettings();
        var creatorUId = Guid.NewGuid();

        _lobbyRepository.PublicIdExist(default).ReturnsForAnyArgs(Task.FromResult(false));
        // Act
        var createLobbyTask = _sut.Invoking(lobbyHandler => lobbyHandler.CreateLobby(creatorUId, settings));

        // Assert
        createLobbyTask
            .Should()
            .ThrowAsync<StatusCodeException>()
            .Where(ex =>
                ex.Message == "Could not find a unique publicId" &&
                ex.StatusCode == HttpStatusCode.InternalServerError
            );
    }

    [Fact]
    private async Task CreateLobby_CreatedLobby()
    {
        // Arrange
        var id = Guid.NewGuid();
        const int maxPlayers = LobbyHandler.MaxPlayers - 1;
        var lobbySettings = new LobbySettings(maxPlayers);
        var playerGuid = Guid.NewGuid();
        const string publicId = "ABC123";
        var creationTime = DateTime.UtcNow;
        var lobby = new LobbyDto(publicId, playerGuid, maxPlayers, creationTime, new List<LobbyPlayer>(), null);

        _lobbyRepository
            .CreateLobby(playerGuid, maxPlayers, Arg.Any<string>())
            .Returns(new LobbyDb(id, publicId, playerGuid, maxPlayers, creationTime, null, new List<LobbyPlayer>()));
        // Act
        var createdLobby = await _sut.CreateLobby(playerGuid, lobbySettings);

        // Assert
        createdLobby.Should().NotBeNull();
        createdLobby.Should().BeEquivalentTo(lobby);
    }

    [Fact]
    private void GeneratePublicId_CorrectFormat()
    {
        // Arrange

        // Act
        var createdPublicId = _sut.GeneratePublicId();
        // Assert
        createdPublicId.Should().HaveLength(6);
        createdPublicId.Should().MatchRegex("^[0-9A-Z]{6}$");
    }

    [Fact]
    private void PublicIdNumToChar_ThrowArgumentEx_WhenNumIsTooLow()
    {
        // Arrange
        const int numToConvert = int.MinValue;
        const string paramName = "number";

        // Act
        var convertTask = _sut.Invoking(_ => LobbyHandler.PublicIdNumToChar(numToConvert));

        // Assert
        convertTask.Should()
            .Throw<ArgumentException>()
            .WithParameterName(paramName);
    }

    [Fact]
    private void PublicIdNumToChar_ThrowArgumentEx_WhenNumIsTooHigh()
    {
        // Arrange
        const int numToConvert = int.MaxValue;
        const string paramName = "number";

        // Act
        var convertTask = _sut.Invoking(_ => LobbyHandler.PublicIdNumToChar(numToConvert));

        // Assert
        convertTask.Should()
            .Throw<ArgumentException>()
            .WithParameterName(paramName);
    }

    [Fact]
    private void PublicIdNumToChar_ReturnA_WhenNumIsFirstAlphabet()
    {
        // Arrange
        const int num = 0;
        const char resultChar = 'A';

        // Act
        var publicIdChar = LobbyHandler.PublicIdNumToChar(num);

        // Assert
        publicIdChar.Should().Be(resultChar);
    }

    [Fact]
    private void PublicIdNumToChar_ReturnA_WhenNumIsLastAlphabet()
    {
        // Arrange
        const int num = 25;
        const char resultChar = 'Z';

        // Act
        var publicIdChar = LobbyHandler.PublicIdNumToChar(num);

        // Assert
        publicIdChar.Should().Be(resultChar);
    }

    [Fact]
    private void PublicIdNumToChar_ReturnA_WhenNumIsFirstNumber()
    {
        // Arrange
        const int num = 26;
        const char resultChar = '0';

        // Act
        var publicIdChar = LobbyHandler.PublicIdNumToChar(num);

        // Assert
        publicIdChar.Should().Be(resultChar);
    }

    [Fact]
    private void PublicIdNumToChar_ReturnA_WhenNumIsLastNumber()
    {
        // Arrange
        const int num = 35;
        const char resultChar = '9';

        // Act
        var publicIdChar = LobbyHandler.PublicIdNumToChar(num);

        // Assert
        publicIdChar.Should().Be(resultChar);
    }
}