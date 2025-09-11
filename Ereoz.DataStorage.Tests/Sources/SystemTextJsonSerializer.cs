using Ereoz.Abstractions.Serialization;
using System.Text.Json;

namespace Ereoz.DataStorage.Tests.Sources
{
    public class SystemTextJsonSerializer : IStringSerializer
    {
        public T Deserialize<T>(string serializedObject) =>
            JsonSerializer.Deserialize<T>(serializedObject);

        public object Deserialize(string serializedObject, Type targetType) =>
            JsonSerializer.Deserialize(serializedObject, targetType);

        public string Serialize<T>(T objectForSerialization, bool formattingIndented = false) =>
            JsonSerializer.Serialize(objectForSerialization, new JsonSerializerOptions { WriteIndented = formattingIndented });
    }
}
