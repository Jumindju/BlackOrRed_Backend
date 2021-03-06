using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Response;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using WebAPI.Model.Exceptions;
using WebAPI.Model.Lobby;
using WebAPI.Repositories;
using Xunit;

namespace WebAPI.Tests.UnitTests.Repositories;

public class LobbyRepositoryTests
{
    private readonly LobbyRepository _sut;

    private readonly ICosmosStore<LobbyDb> _cosmosStore = Substitute.For<ICosmosStore<LobbyDb>>();
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();

    public LobbyRepositoryTests()
    {
        _sut = new LobbyRepository(_cosmosStore, _loggerFactory);
    }

    [Fact]
    private async Task GetLobbyByPublicId_ReturnNull_WhenEntityNotFound()
    {
        // Arrange
        const string publicId = "123abc";
        _cosmosStore.QuerySingleAsync<LobbyDto>(default)
            .ReturnsNullForAnyArgs();

        // Act
        var result = await _sut.GetLobbyByPublicId(publicId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    private async Task GetLobbyByPublicId_ReturnEntity_WhenEntityFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string publicId = "ABC123";
        var adminUser = Guid.NewGuid();
        const int maxPlayer = 5;
        var creationTime = DateTime.UtcNow;
        Guid? currentSession = null;
        var players = new List<LobbyPlayer>();
        var lobby = new LobbyDb(id, publicId, adminUser, maxPlayer, creationTime, currentSession, players);
        _cosmosStore
            .QuerySingleAsync<LobbyDb>(default)
            .ReturnsForAnyArgs(lobby);

        // Act
        var result = await _sut.GetLobbyByPublicId(publicId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(lobby);
    }

    [Fact]
    private async Task PublicIdExist_ReturnFalse_WhenLobbyNotFound()
    {
        // Arrange
        const string publicId = "123ABC";
        _cosmosStore.QuerySingleAsync<LobbyDto>(default)
            .ReturnsNullForAnyArgs();

        // Act
        var result = await _sut.PublicIdExist(publicId);
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    private async Task PublicIdExist_ReturnTrue_WhenLobbyFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string publicId = "ABC123";
        var adminUser = Guid.NewGuid();
        const int maxPlayer = 5;
        var creationTime = DateTime.UtcNow;
        Guid? currentSession = null;
        var players = new List<LobbyPlayer>();
        var lobby = new LobbyDb(id, publicId, adminUser, maxPlayer, creationTime, currentSession, players);
        _cosmosStore
            .QuerySingleAsync<LobbyDb>(default)
            .ReturnsForAnyArgs(lobby);

        // Act
        var result = await _sut.PublicIdExist(publicId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    private async Task CreateLobby_ThrowStatusEx_WhenLobbyNotCreated()
    {
        // Arrange
        const string publicId = "ABC123";
        var adminUId = Guid.NewGuid();
        const int maxPlayer = 5;

        const string expectedExMsg = "Could not create lobby";
        const HttpStatusCode expectedStatusCode = HttpStatusCode.InternalServerError;
        const string testExceptionMsg = "Test exception";
        var testException = new ArgumentException(testExceptionMsg);

        var anyLobbyArgument = new LobbyDb(Guid.NewGuid(), publicId, adminUId, maxPlayer, DateTime.UtcNow, null,
            new List<LobbyPlayer>());
        var createErrResult = new CosmosResponse<LobbyDb>(anyLobbyArgument, testException, CosmosOperationStatus.Conflict);

        _cosmosStore
            .AddAsync(anyLobbyArgument)
            .ReturnsForAnyArgs(createErrResult);

        // Act
        var createLobbyTask = _sut.Invoking(x => x.CreateLobby(adminUId, maxPlayer, publicId));

        // Assert
        await createLobbyTask.Should()
            .ThrowAsync<StatusCodeException>()
            .Where(ex =>
                ex.StatusCode == expectedStatusCode &&
                ex.Message == expectedExMsg &&
                ex.InnerException != null &&
                ex.InnerException.GetType() == typeof(ArgumentException) &&
                ex.InnerException.Message == testExceptionMsg);
    }

    [Fact]
    private async Task CreateLobby_ReturnLobby_WhenLobbyCreated()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string publicId = "ABC123";
        var adminUId = Guid.NewGuid();
        const int maxPlayer = 5;
        var creationTime = DateTime.UtcNow;
        Guid? currentSession = null;
        var players = new List<LobbyPlayer>();

        var lobby = new LobbyDb(id, publicId, adminUId, maxPlayer, creationTime, currentSession, players);
        var createResult = new CosmosResponse<LobbyDb>(lobby, null, CosmosOperationStatus.Success);

        _cosmosStore
            .AddAsync(lobby)
            .ReturnsForAnyArgs(createResult);

        // Act
        var result = await _sut.CreateLobby(adminUId, maxPlayer, publicId);

        // Assert
        result.Should().BeEquivalentTo(lobby);
    }
}