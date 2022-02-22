namespace WebAPI.Model.Session;

// TODO Add session information
public record SessionDb(DateTime DateTime, int CurrentRound, List<SessionPlayer> ActivePlayers);