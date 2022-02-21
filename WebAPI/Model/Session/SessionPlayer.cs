namespace WebAPI.Model.Session;

public record SessionPlayer(Guid playerGuid, string UserName, DateTime? DisconnectionTime, bool Disqualified, List<Try> Tries);