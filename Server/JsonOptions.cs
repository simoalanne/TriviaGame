using System.Text.Json;
using System.Text.Json.Serialization;

namespace TriviaGame;

public static class JsonOptionsProvider
{
    /// <summary>
    /// Use this to prevent serializing null values in JSON output. This is important especially when the json is stored
    /// into the postgres database as jsonb, to prevent unnecessary storage bloat.
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}



