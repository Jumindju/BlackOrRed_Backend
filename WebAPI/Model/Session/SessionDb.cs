using Cosmonaut;
using Cosmonaut.Attributes;

namespace WebAPI.Model.Session;

// TODO Add session information
[SharedCosmosCollection(Constants.SharedCollectionName, "Sessions")]
public record SessionDb(
    [property: CosmosPartitionKey]
    Guid SessionUId,
    DateTime DateTime,
    int CurrentRound,
    List<SessionPlayer> ActivePlayers
) : ISharedCosmosEntity
{
    public string CosmosEntityName { get; set; }
}