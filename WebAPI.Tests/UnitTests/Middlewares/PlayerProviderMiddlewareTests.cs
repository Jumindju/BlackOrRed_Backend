using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WebAPI.Helper.Extensions;
using WebAPI.Helper.Middleware;
using WebAPI.Model;
using WebAPI.Model.Lobby;
using Xunit;

namespace WebAPI.Tests.UnitTests.Middlewares;

public class PlayerProviderMiddlewareTests
{
    private readonly PlayerProviderMiddleware _sut;

    private readonly RequestDelegate _next = Substitute.For<RequestDelegate>();
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();

    public PlayerProviderMiddlewareTests()
    {
        _sut = new PlayerProviderMiddleware(_next, _loggerFactory);
    }

    [Fact]
    private async Task Invoke_NoPlayer_WhenNoHeaderIsProvided()
    {
        // Arrange
        var httpContext = Substitute.For<HttpContext>();
        httpContext.Request.Headers.Returns(new HeaderDictionary());

        // Act
        await _sut.Invoke(httpContext);

        // Assert
        httpContext.GetPlayer().Should().BeNull();
    }

    [Fact]
    private async Task Invoke_NoPlayer_WhenNoUIdIsProvided()
    {
        // Arrange
        const string playerName = "test";

        var httpContext = Substitute.For<HttpContext>();
        var headers = new HeaderDictionary
        {
            { Constants.PlayerNameHeader, playerName }
        };
        httpContext.Request.Headers.Returns(headers);

        // Act
        await _sut.Invoke(httpContext);

        // Assert
        httpContext.GetPlayer().Should().BeNull();
    }

    [Fact]
    private async Task Invoke_NoPlayer_WhenNoNameIsProvided()
    {
        // Arrange
        var uid = Guid.NewGuid();

        var httpContext = Substitute.For<HttpContext>();
        var headers = new HeaderDictionary
        {
            { Constants.PlayerUIdHeader, uid.ToString() }
        };
        httpContext.Request.Headers.Returns(headers);

        // Act
        await _sut.Invoke(httpContext);

        // Assert
        httpContext.GetPlayer().Should().BeNull();
    }

    [Fact]
    private async Task Invoke_NoPlayerNoException_WhenSettingUserFails()
    {
        // Arrange
        var uid = Guid.NewGuid();
        const string playerName = "test";
        var player = new LobbyPlayer(uid, playerName);

        var httpContext = Substitute.For<HttpContext>();
        var headers = new HeaderDictionary
        {
            { Constants.PlayerUIdHeader, uid.ToString() },
            { Constants.PlayerNameHeader, playerName }
        };
        httpContext.Request.Headers.Returns(headers);
        httpContext.Items
            .When(x => x.Add(Constants.PlayerItemKey, player))
            .Do(x => throw new Exception());

        // Act
        var invokeTask = _sut.Invoking(middleware => middleware.Invoke(httpContext));

        // Assert
        invokeTask.Should().NotThrowAsync<Exception>();
        httpContext.GetPlayer().Should().BeNull();
    }

    [Fact]
    private async Task Invoke_ReturnPlayer_WhenHeadersAreSet()
    {
        // Arrange
        // Arrange
        var uid = Guid.NewGuid();
        const string playerName = "test";
        var player = new LobbyPlayer(uid, playerName);

        var httpContext = Substitute.For<HttpContext>();
        var headers = new HeaderDictionary
        {
            { Constants.PlayerUIdHeader, uid.ToString() },
            { Constants.PlayerNameHeader, playerName }
        };
        httpContext.Request.Headers.Returns(headers);
        httpContext.Items
            .When(x => x.Add(Constants.PlayerItemKey, player))
            .Do(_ => httpContext.Items = new Dictionary<object, object?>
            {
                { Constants.PlayerItemKey, player }
            });

        // Act
        await _sut.Invoke(httpContext);

        // Assert
        httpContext.GetPlayer().Should().BeEquivalentTo(player);
    }
}