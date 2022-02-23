using Cosmonaut;
using Cosmonaut.Attributes;

namespace WebAPI.Model.Lobby;

[SharedCosmosCollection(Constants.SharedCollectionName, "Lobbies")]
public record LobbyDb(
    Guid Id,
    [property: CosmosPartitionKey] string PublicId,
    Guid CurrentAdmin,
    int MaxPlayer,
    DateTime CreationTime,
    Guid? SessionUId,
    List<LobbyPlayer> CurrentUsers
) : ISharedCosmosEntity
{
    public string CosmosEntityName { get; set; } = null!;
}