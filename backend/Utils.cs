using System.Text.Json;
using System.Text.Json.Serialization;

namespace TriviaGame;

public static class Utils
{
    public static string NewGuid() => Guid.NewGuid().ToString();
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    /// <summary>
    /// Serializes an object to literal JSON string, ignoring null values.
    /// </summary>
    /// <param name="obj"> the object to serialize</param>
    /// <typeparam name="T"> the type of the object</typeparam>
    /// <returns> the JSON string</returns>
    public static string ToJson<T>(T obj) =>
        JsonSerializer.Serialize(obj, JsonOptions);
}