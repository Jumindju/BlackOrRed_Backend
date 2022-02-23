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
    public LobbyDto(string PublicId, Guid CurrentAdmin, int MaxPlayer, DateTime CreationTime,
        List<LobbyPlayer> CurrentUsers,
        SessionDb? CurrentSession)
    {
        this.PublicId = PublicId;
        this.CurrentAdmin = CurrentAdmin;
        this.MaxPlayer = MaxPlayer;
        this.CreationTime = CreationTime;
        this.CurrentUsers = CurrentUsers;
        this.CurrentSession = CurrentSession;
    }

    public string PublicId { get; init; }
    public Guid CurrentAdmin { get; init; }
    public int MaxPlayer { get; init; }
    public DateTime CreationTime { get; init; }
    public List<LobbyPlayer> CurrentUsers { get; init; }
    public SessionDb? CurrentSession { get; init; }
}