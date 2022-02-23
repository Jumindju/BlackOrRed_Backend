using System.Net;
using Cosmonaut;
using WebAPI.Interfaces.Repositories;
using WebAPI.Model.Exceptions;
using WebAPI.Model.Lobby;

namespace WebAPI.Repositories;

public class LobbyRepository : ILobbyRepository
{
    private readonly ICosmosStore<LobbyDb> _lobbyStore;
    private readonly ILogger _logger;

    public LobbyRepository(ICosmosStore<LobbyDb> lobbyStore, ILoggerFactory loggerFactory)
    {
        _lobbyStore = lobbyStore;
        _logger = loggerFactory.CreateLogger<LobbyRepository>();
    }

    public async Task<LobbyDb?> GetLobbyByPublicId(string publicId)
    {
        const string query = "select * from c where c.PublicId = @publicId";
        var parameter = new
        {
            publicId
        };
        return await _lobbyStore.QuerySingleAsync<LobbyDb>(query, parameter);
    }

    public async Task<bool> PublicIdExist(string publicId)
    {
        var lobby = await GetLobbyByPublicId(publicId);
        return lobby is not null;
    }

    public async Task<LobbyDb> CreateLobby(Guid creatorId, int maxPlayer, string publicId)
    {
        var createdLobbyResponse = await _lobbyStore.AddAsync(new LobbyDb(
            Guid.NewGuid(),
            publicId,
            creatorId,
            maxPlayer,
            DateTime.UtcNow,
            null,
            new List<LobbyPlayer>()));

        if (createdLobbyResponse.IsSuccess)
        {
            _logger.LogInformation("Lobby {LobbyId} created", createdLobbyResponse.Entity.PublicId);
            return createdLobbyResponse.Entity;
        }

        _logger.LogError(createdLobbyResponse.Exception, "Could not create lobby. Status: {OperationStatus}",
            createdLobbyResponse.CosmosOperationStatus);

        throw new StatusCodeException(HttpStatusCode.InternalServerError,
            "Could not create lobby",
            createdLobbyResponse.Exception);
    }
}