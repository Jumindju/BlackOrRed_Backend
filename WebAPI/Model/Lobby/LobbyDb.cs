namespace WebAPI.Model.Lobby;

public record LobbyDb(string PublicId, Guid CurrentAdmin, int MaxPlayer, DateTime CreationTime, Guid? SessionUId, List<LobbyPlayer> CurrentUsers);