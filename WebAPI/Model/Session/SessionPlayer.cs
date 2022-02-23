namespace WebAPI.Model.Session;

public record SessionPlayer(Guid Guid, string Name, DateTime? DisconnectionTime, bool Disqualified, List<Try> Tries);