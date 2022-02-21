using WebAPI.Model.Lobby;

namespace WebAPI.Interfaces.Handler;

public interface ILobbyHandler
{
    Task<LobbyDto> CreateLobby(Guid creatorUId,LobbySettings settings);
}