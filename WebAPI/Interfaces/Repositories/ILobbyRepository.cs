using WebAPI.Model.Lobby;

namespace WebAPI.Interfaces.Repositories;

public interface ILobbyRepository
{
    Task<bool> PublicIdExist(string publicId);
    Task<LobbyDb> CreateLobby(Guid creatorId, int maxPlayer, string publicId);
}