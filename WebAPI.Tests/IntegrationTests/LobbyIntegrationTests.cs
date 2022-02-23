using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WebAPI.Handler;
using WebAPI.Interfaces.Handler;
using WebAPI.Model;
using WebAPI.Model.Exceptions;
using WebAPI.Model.Helper;
using WebAPI.Model.Lobby;
using Xunit;

namespace WebAPI.Tests.Endpoints;

public class LobbyEndpointTests
{
    private readonly ILobbyHandler _lobbyHandler = Substitute.For<ILobbyHandler>();

    public LobbyEndpointTests()
    {
        Environment.SetEnvironmentVariable("Cosmos:DbName", "Test");
        Environment.SetEnvironmentVariable("Cosmos:Uri", "https://test.com");
        Environment.SetEnvironmentVariable("Cosmos:Key", "Test");
    }

    [Fact]
    private async Task CreateLobby_Return400_WhenSettingsIsNull()
    {
        // Arrange
        await using var app = new LobbyEndpointsApp(x => { x.AddSingleton(_lobbyHandler); });
        var httpClient = app.CreateClient();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "lobby");
        var response = await httpClient.SendAsync(request);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    private async Task CreateLobby_Return401_WhenNoPlayerHeader()
    {
        // Arrange
        var lobbySettings = new LobbySettings();

        await using var app = new LobbyEndpointsApp(x => { x.AddSingleton(_lobbyHandler); });
        var httpClient = app.CreateClient();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "lobby")
        {
            Content = JsonContent.Create(lobbySettings)
        };
        var response = await httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    private async Task CreateLobby_Return401_WhenPlayerUIdEmpty()
    {
        // Arrange
        var lobbySettings = new LobbySettings();
        const string playerName = "test";

        await using var app = new LobbyEndpointsApp(x => { x.AddSingleton(_lobbyHandler); });
        var httpClient = app.CreateClient();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "lobby")
        {
            Content = JsonContent.Create(lobbySettings),
            Headers =
            {
                { Constants.PlayerNameHeader, playerName }
            }
        };
        var response = await httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    private async Task CreateLobby_Return401_WhenNoPlayerNameEmpty()
    {
        // Arrange
        var lobbySettings = new LobbySettings();
        var playerGuid = Guid.NewGuid();

        await using var app = new LobbyEndpointsApp(x => { x.AddSingleton(_lobbyHandler); });
        var httpClient = app.CreateClient();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "lobby")
        {
            Content = JsonContent.Create(lobbySettings),
            Headers =
            {
                { Constants.PlayerUIdHeader, playerGuid.ToString() }
            }
        };
        var response = await httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private const string InvalidPlayerErrMessage = "Lobby must have between 2 and 10 players";

    [Fact]
    private async Task CreateLobby_Return400_WhenMaxPlayerToLow()
    {
        // Arrange
        const int maxPlayer = LobbyHandler.MinPlayers - 1;
        var lobbySettings = new LobbySettings(maxPlayer);

        var playerGuid = Guid.NewGuid();
        const string playerName = "test";

        _lobbyHandler.CreateLobby(Arg.Is(playerGuid), Arg.Is(lobbySettings))
            .Throws(new StatusCodeException(HttpStatusCode.BadRequest, InvalidPlayerErrMessage));

        await using var app = new LobbyEndpointsApp(x => { x.AddSingleton(_lobbyHandler); });
        var httpClient = app.CreateClient();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "lobby")
        {
            Content = JsonContent.Create(lobbySettings),
            Headers =
            {
                { Constants.PlayerUIdHeader, playerGuid.ToString() },
                { Constants.PlayerNameHeader, playerName }
            }
        };
        var response = await httpClient.SendAsync(request);
        var (responseMsg, responseEx) = await response.Content.ReadFromJsonAsync<StatusCodeExceptionResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        responseMsg.Should().Be(InvalidPlayerErrMessage);
        responseEx.Should().BeNull();
    }

    [Fact]
    private async Task CreateLobby_Return400_WhenMaxPlayerToHigh()
    {
        // Arrange
        const int maxPlayer = LobbyHandler.MaxPlayers + 1;
        var lobbySettings = new LobbySettings(maxPlayer);

        var playerGuid = Guid.NewGuid();
        const string playerName = "test";

        _lobbyHandler.CreateLobby(Arg.Is(playerGuid), Arg.Is(lobbySettings))
            .Throws(new StatusCodeException(HttpStatusCode.BadRequest, InvalidPlayerErrMessage));
        await using var app = new LobbyEndpointsApp(x => { x.AddSingleton(_lobbyHandler); });
        var httpClient = app.CreateClient();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "lobby")
        {
            Content = JsonContent.Create(lobbySettings),
            Headers =
            {
                { Constants.PlayerUIdHeader, playerGuid.ToString() },
                { Constants.PlayerNameHeader, playerName }
            }
        };
        var response = await httpClient.SendAsync(request);
        var (responseMsg, responseEx) = await response.Content.ReadFromJsonAsync<StatusCodeExceptionResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        responseMsg.Should().Be(InvalidPlayerErrMessage);
        responseEx.Should().BeNull();
    }

    [Fact]
    private async Task CreateLobby_Return201_WhenParameterValid()
    {
        // Arrange
        const int maxPlayer = LobbyHandler.MaxPlayers - 1;
        var lobbySettings = new LobbySettings(maxPlayer);

        var playerGuid = Guid.NewGuid();
        const string playerName = "test";

        var createdLobby =
            new LobbyDto("ABC123", playerGuid, maxPlayer, DateTime.UtcNow, new List<LobbyPlayer>(), null);

        _lobbyHandler.CreateLobby(Arg.Is(playerGuid), Arg.Is(lobbySettings))
            .Returns(createdLobby);

        await using var app = new LobbyEndpointsApp(x => { x.AddSingleton(_lobbyHandler); });
        var httpClient = app.CreateClient();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "lobby")
        {
            Content = JsonContent.Create(lobbySettings),
            Headers =
            {
                { Constants.PlayerUIdHeader, playerGuid.ToString() },
                { Constants.PlayerNameHeader, playerName }
            }
        };
        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadFromJsonAsync<LobbyDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        responseBody.Should().NotBeNull();
        responseBody.Should().BeEquivalentTo(createdLobby);
    }
}

internal class LobbyEndpointsApp : WebApplicationFactory<Program>
{
    private readonly Action<IServiceCollection> _serviceOverride;

    public LobbyEndpointsApp(Action<IServiceCollection> serviceOverride)
    {
        _serviceOverride = serviceOverride;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(_serviceOverride);

        return base.CreateHost(builder);
    }
}