using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using WebAPI.Helper;
using WebAPI.Helper.Middleware;
using WebAPI.Model;
using WebAPI.Model.Exceptions;
using WebAPI.Model.Helper;
using Xunit;

namespace WebAPI.Tests.UnitTests.Middlewares;

public class RequestLoggingMiddlewareTests
{
    private readonly RequestLoggingMiddleware _sut;

    private readonly RequestDelegate _next = Substitute.For<RequestDelegate>();
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();

    public RequestLoggingMiddlewareTests()
    {
        _sut = new RequestLoggingMiddleware(_next, _loggerFactory);
    }

    [Fact]
    private async Task Invoke_WritesStatusCodeAndMessage_WhenStatusCodeExceptionMessageIsThrown()
    {
        // Arrange
        const HttpStatusCode customStatusCode = HttpStatusCode.PaymentRequired;
        const string customExMsg = "Test msg";
        var httpContext = GetBaseHttpContext();

        _next
            .WhenForAnyArgs(x => x.Invoke(httpContext))
            .Do(_ => throw new StatusCodeException(customStatusCode, customExMsg));
        // Act
        var invokeTask = _sut.Invoking(middleware => middleware.Invoke(httpContext));

        // Assert
        await invokeTask.Should().NotThrowAsync();

        httpContext.Items[Constants.RequestGuidItemKey].Should().NotBeNull();
        httpContext.Response.StatusCode.Should().Be((int)customStatusCode);

        httpContext.Response.Body.Position = 0;
        using var sr = new StreamReader(httpContext.Response.Body);
        var responseBodyRaw = await sr.ReadToEndAsync();
        responseBodyRaw.Should().NotBeNullOrEmpty();

        var responseBody = JsonConvert.DeserializeObject<StatusCodeExceptionResponse>(responseBodyRaw);
        responseBody.Should().NotBeNull();
        responseBody.Message.Should().Be(customExMsg);
        responseBody.Inner.Should().BeNull();
    }

    [Fact]
    private async Task
        Invoke_WritesStatusCodeAndMessageAndException_WhenStatusCodeExceptionMessageIsThrownWithInnerException()
    {
        // Arrange
        const HttpStatusCode customStatusCode = HttpStatusCode.PaymentRequired;
        const string customExMsg = "Test msg";
        const string customInnerExMsg = "Inner test";
        var testException = new Exception(customInnerExMsg);

        var httpContext = GetBaseHttpContext();

        _next
            .WhenForAnyArgs(x => x.Invoke(httpContext))
            .Do(_ => throw new StatusCodeException(customStatusCode, customExMsg, testException));
        // Act
        var invokeTask = _sut.Invoking(middleware => middleware.Invoke(httpContext));

        // Assert
        await invokeTask.Should().NotThrowAsync();

        httpContext.Items[Constants.RequestGuidItemKey].Should().NotBeNull();
        httpContext.Response.StatusCode.Should().Be((int)customStatusCode);

        httpContext.Response.Body.Position = 0;
        using var sr = new StreamReader(httpContext.Response.Body);
        var responseBodyRaw = await sr.ReadToEndAsync();
        responseBodyRaw.Should().NotBeNullOrEmpty();

        var responseBody = JsonConvert.DeserializeObject<StatusCodeExceptionResponse>(responseBodyRaw);
        responseBody.Should().NotBeNull();
        responseBody.Message.Should().Be(customExMsg);
        responseBody.Inner.Should().NotBeNull();
    }

    [Fact]
    private async Task Invoke_SetCorrectStatusCode_WhenWritingStatusCodeExceptionFails()
    {
        // Arrange
        const HttpStatusCode customStatusCode = HttpStatusCode.PaymentRequired;
        const string customExMsg = "Test msg";

        var httpContext = GetBaseHttpContext();
        httpContext.Response
            .WhenForAnyArgs(x =>
                x.WriteAsJsonAsync("", Serializer.IgnoreNullSerializer))
            .Do(_ => throw new Exception("Write exception"));

        _next
            .WhenForAnyArgs(x => x.Invoke(httpContext))
            .Do(_ => throw new StatusCodeException(customStatusCode, customExMsg));
        // Act
        var invokeTask = _sut.Invoking(middleware => middleware.Invoke(httpContext));

        // Assert
        await invokeTask.Should().NotThrowAsync();

        httpContext.Items[Constants.RequestGuidItemKey].Should().NotBeNull();
        httpContext.Response.StatusCode.Should().Be((int)customStatusCode);
    }

    [Fact]
    private async Task Invoke_Set500_WhenUnhandledExceptionIsThrown()
    {
        // Arrange
        var httpContext = GetBaseHttpContext();

        _next
            .WhenForAnyArgs(x => x.Invoke(httpContext))
            .Do(_ => throw new Exception("Unhandled exception"));
        // Act
        var invokeTask = _sut.Invoking(middleware => middleware.Invoke(httpContext));

        // Assert
        await invokeTask
            .Should()
            .NotThrowAsync();
        httpContext.Items[Constants.RequestGuidItemKey].Should().NotBeNull();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // TODO Test request log

    private static HttpContext GetBaseHttpContext()
    {
        var httpContext = Substitute.For<HttpContext>();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Items.Returns(new Dictionary<object, object?>());

        return httpContext;
    }
}