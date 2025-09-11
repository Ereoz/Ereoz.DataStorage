using Ereoz.Abstractions.Serialization;
using Newtonsoft.Json;

namespace Ereoz.DataStorage.Tests.Sources
{
    public class NewtonsoftJsonSerializer : IStringSerializer
    {
        public T Deserialize<T>(string serializedObject) =>
            JsonConvert.DeserializeObject<T>(serializedObject);

        public object Deserialize(string serializedObject, Type targetType) =>
            JsonConvert.DeserializeObject(serializedObject, targetType);

        public string Serialize<T>(T objectForSerialization, bool formattingIndented = false) =>
            JsonConvert.SerializeObject(objectForSerialization, formattingIndented ? Formatting.Indented : Formatting.None);
    }
}
