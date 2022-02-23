using System.Net;
using System.Text;
using WebAPI.Interfaces.Handler;
using WebAPI.Interfaces.Repositories;
using WebAPI.Model.Exceptions;
using WebAPI.Model.Lobby;

namespace WebAPI.Handler;

public class LobbyHandler : ILobbyHandler
{
    private readonly ILogger _logger;
    private readonly ILobbyRepository _lobbyRepository;

    private readonly Random _random = new();

    public const int MinPlayers = 2;
    public const int MaxPlayers = 10;

    private const int PublicIdLength = 6;
    private const int AlphabetLength = 26;
    private const int Numbers = 10;
    private const int MaxPublicIdLookupTries = 10;

    public LobbyHandler(ILoggerFactory loggerFactory, ILobbyRepository lobbyRepository)
    {
        _lobbyRepository = lobbyRepository;
        _logger = loggerFactory.CreateLogger<LobbyHandler>();
    }

    public async Task<LobbyDto?> GetLobbyByPublicId(string publicId)
    {
        var lobby = await _lobbyRepository.GetLobbyByPublicId(publicId);
        return lobby is null
            ? null
            : new LobbyDto(lobby, null); // TODO load session
    }

    public async Task<LobbyDto> CreateLobby(Guid creatorUId, LobbySettings settings)
    {
        if (settings.MaxPlayer is < MinPlayers or > MaxPlayers)
        {
            throw new StatusCodeException(HttpStatusCode.BadRequest, "Lobby must have between 2 and 10 players");
        }

        string? publicId = null;
        var foundUniquePublicId = false;
        for (var i = 0; i < MaxPublicIdLookupTries; i++)
        {
            publicId = GeneratePublicId();
            if (await _lobbyRepository.PublicIdExist(publicId))
                continue;

            foundUniquePublicId = true;
            break;
        }

        if (!foundUniquePublicId)
        {
            _logger.LogError("Could not find publicId after 10 tries. (CreatorId: {CreatorId})", creatorUId);
            throw new StatusCodeException(HttpStatusCode.InternalServerError, "Could not find a unique publicId");
        }

        var createdLobby = await _lobbyRepository.CreateLobby(creatorUId, settings.MaxPlayer, publicId);
        _logger.LogInformation("Created lobby {PublicId}, created by {CurrentAdmin} at {CreationTime}",
            createdLobby.PublicId,
            createdLobby.CurrentAdmin, createdLobby.CreationTime);

        return new LobbyDto(createdLobby, null);
    }

    internal string GeneratePublicId()
    {
        var stringBuilder = new StringBuilder();

        for (var i = 0; i < PublicIdLength; i++)
        {
            // 0-25 Character A-Z
            // 26-35 Number 0-9
            var rndNum = _random.Next(0, AlphabetLength + Numbers);
            var publicIdChar = PublicIdNumToChar(rndNum);
            stringBuilder.Append(publicIdChar);
        }

        return stringBuilder.ToString();
    }

    internal static char PublicIdNumToChar(int number)
    {
        var characterCode = number switch
        {
            < 0 or > AlphabetLength + Numbers => throw new ArgumentException(
                "PublicId element must be between 0 and 35",
                nameof(number)),
            < AlphabetLength => 'A' + number,
            _ => '0' + (number - AlphabetLength)
        };

        return (char)characterCode;
    }
}