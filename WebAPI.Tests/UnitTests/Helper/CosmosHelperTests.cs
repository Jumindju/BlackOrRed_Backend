using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using WebAPI.Helper.Cosmos;
using Xunit;

namespace WebAPI.Tests.UnitTests.Helper;

public class CosmosHelperTests
{
    [Fact]
    private void GetCosmosStoreSettings_ThrowEx_WhenDbNameIsInvalid()
    {
        // Arrange
        const string expectedMsg = "Cosmos db name not provided";
        
        var configuration = new ConfigurationBuilder()
            .Build();
        // Act
        var act = () => CosmosHelper.GetCosmosStoreSettings(configuration);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage(expectedMsg);
    }

    [Fact]
    private void GetCosmosStoreSettings_ThrowEx_WhenUriIsInvalid()
    {
        // Arrange
        const string expectedMsg = "Cosmos uri not provided";

        const string testDbName = "Test";
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Cosmos:DbName", testDbName}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        // Act
        var act = () => CosmosHelper.GetCosmosStoreSettings(configuration);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage(expectedMsg);
    }

    [Fact]
    private void GetCosmosStoreSettings_ThrowEx_WhenKeyIsInvalid()
    {
        // Arrange
        const string expectedMsg = "Cosmos key not provided";
        
        const string testDbName = "Test";
        const string testUri = "Test Uri";
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Cosmos:DbName", testDbName},
            {"Cosmos:Uri", testUri}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        // Act
        var act = () => CosmosHelper.GetCosmosStoreSettings(configuration);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage(expectedMsg);
    }

    [Fact]
    private void GetCosmosStoreSettings_ReturnSettings_WhenSettingsAreValid()
    {
        // Arrange
        const string testDbName = "Test";
        const string testUriRaw = "https://google.com";
        var testUri = new Uri(testUriRaw);
        const string testKey = "Test key";
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Cosmos:DbName", testDbName},
            {"Cosmos:Uri", testUriRaw},
            {"Cosmos:Key", testKey}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        // Act
        var settings = CosmosHelper.GetCosmosStoreSettings(configuration);

        // Assert
        settings.Should().NotBeNull();
        settings.DatabaseName.Should().Be(testDbName);
        settings.EndpointUrl.Should().Be(testUri);
    }
}