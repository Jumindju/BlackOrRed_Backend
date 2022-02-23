using WebAPI.Model.Game;

namespace WebAPI.Model.Session;

public record Try(Answer Answer, byte ReceivedCard, bool Correct, DateTime StartTime);