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

    private readonly Random _random = new Random();

    private const int MinPlayers = 2;
    private const int MaxPlayers = 10;

    private const int PublicIdLength = 6;
    private const int AlphabetLength = 26;
    private const int Numbers = 10;
    private const int MaxPublicIdLookupTries = 10;

    public LobbyHandler(ILoggerFactory loggerFactory, ILobbyRepository lobbyRepository)
    {
        _lobbyRepository = lobbyRepository;
        _logger = loggerFactory.CreateLogger<LobbyHandler>();
    }

    public async Task<LobbyDto> CreateLobby(Guid creatorUId, LobbySettings settings)
    {
        if (settings.MaxPlayer is < MinPlayers or > MaxPlayers)
        {
            throw new StatusCodeException(HttpStatusCode.BadRequest, "Lobby must have between 2 and 10 players");
        }

        string? publicId = null;
        for (var i = 0; i < MaxPublicIdLookupTries; i++)
        {
            publicId = GeneratePublicId();
            if (!await _lobbyRepository.PublicIdExist(publicId))
                break;
        }

        if (publicId is null)
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

    private string GeneratePublicId()
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

    private static char PublicIdNumToChar(int num)
    {
        var characterCode = num switch
        {
            < 0 or > AlphabetLength + Numbers => throw new ArgumentException(
                "PublicId element must be between 0 and 35"),
            < AlphabetLength => 'A' + num,
            _ => '0' + (num - AlphabetLength)
        };

        return (char)characterCode;
    }
}