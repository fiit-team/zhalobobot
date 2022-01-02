using System.IO;
using Newtonsoft.Json;

namespace Zhalobobot.Common.Models.Serialization
{
    public static class JsonSerialization
    {
        private static readonly JsonSerializerSettings Settings;

        private static readonly JsonSerializer Serializer;
        
        static JsonSerialization()
        {
            Settings = SetupCommonSettings(new JsonSerializerSettings());
            Serializer = JsonSerializer.CreateDefault(Settings);
        }

        public static string ToPrettyJson(this object? @object) => JsonConvert.SerializeObject(@object, Formatting.Indented, Settings);

        public static string ToJson(this object? @object) => JsonConvert.SerializeObject(@object, Settings);
        
        public static T FromJson<T>(this string serialized) => JsonConvert.DeserializeObject<T>(serialized, Settings)!;
        
        public static T FromJsonStream<T>(this Stream serialized)
        {
            using var streamReader = new StreamReader(serialized);
            using var jsonReader = new JsonTextReader(streamReader);
            
            return Serializer.Deserialize<T>(jsonReader)!;
        }

        private static JsonSerializerSettings SetupCommonSettings(JsonSerializerSettings settings)
        {
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DateParseHandling = DateParseHandling.None;

            return settings;
        }
    }
}