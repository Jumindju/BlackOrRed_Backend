namespace WebAPI.Model.Lobby;

public record LobbyDb(string PublicId, int MaxUser, DateTime CreationTime, Guid? sessionUId, List<LobbyPlayer> currentUsers);