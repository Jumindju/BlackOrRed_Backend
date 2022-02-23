using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAPI.Helper;

public static class Serializer
{
    public static JsonSerializerOptions IgnoreNullSerializer { get; } = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}