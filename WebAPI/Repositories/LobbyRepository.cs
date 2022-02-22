using WebAPI.Interfaces.Repositories;
using WebAPI.Model.Lobby;

namespace WebAPI.Repositories;

public class LobbyRepository : ILobbyRepository
{
    // TODO Use azure db
    public Task<bool> PublicIdExist(string publicId)
    {
        return Task.FromResult(false);
    }

    public Task<LobbyDb> CreateLobby(Guid creatorId, int maxPlayer, string publicId)
    {
        return Task.FromResult(new LobbyDb(publicId, creatorId, maxPlayer, DateTime.UtcNow, null,
            new List<LobbyPlayer>()));
    }
}