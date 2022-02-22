using WebAPI.Model.Session;

namespace WebAPI.Model.Lobby;

public record LobbyDto(string PublicId, Guid CurrentAdmin, int MaxPlayer, DateTime CreationTime,
    List<LobbyPlayer> CurrentUsers,
    SessionDb? CurrentSession)
{
    public LobbyDto(LobbyDb dbLobby, SessionDb? currentSession)
        : this(dbLobby.PublicId, dbLobby.CurrentAdmin, dbLobby.MaxPlayer, dbLobby.CreationTime, dbLobby.CurrentUsers,
            currentSession)
    {
    }
}