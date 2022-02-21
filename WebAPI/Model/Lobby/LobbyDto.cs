using WebAPI.Model.Session;

namespace WebAPI.Model.Lobby;

public record LobbyDto(string PublicId, int MaxUser, DateTime CreationTime, List<LobbyPlayer> currentUsers, SessionDb? currentSession);