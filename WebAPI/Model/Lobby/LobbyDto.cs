using System.Text.Json.Serialization;
using WebAPI.Model.Session;

namespace WebAPI.Model.Lobby;

public record LobbyDto
{
    public LobbyDto(LobbyDb dbLobby, SessionDb? currentSession)
        : this(dbLobby.PublicId, dbLobby.CurrentAdmin, dbLobby.MaxPlayer, dbLobby.CreationTime, dbLobby.CurrentUsers,
            currentSession)
    {
    }

    [JsonConstructor]
    public LobbyDto(string publicId, Guid currentAdmin, int maxPlayer, DateTime creationTime,
        List<LobbyPlayer> currentUsers,
        SessionDb? currentSession)
    {
        this.PublicId = publicId;
        this.CurrentAdmin = currentAdmin;
        this.MaxPlayer = maxPlayer;
        this.CreationTime = creationTime;
        this.CurrentUsers = currentUsers;
        this.CurrentSession = currentSession;
    }

    public string PublicId { get; init; }
    public Guid CurrentAdmin { get; init; }
    public int MaxPlayer { get; init; }
    public DateTime CreationTime { get; init; }
    public List<LobbyPlayer> CurrentUsers { get; init; }
    public SessionDb? CurrentSession { get; init; }
}