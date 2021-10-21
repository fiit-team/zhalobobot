using System;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Zhalobobot.Common.Models.Serialization
{
    public static class JsonSerialization
    {
        public static readonly JsonSerializerSettings Settings;
        private static readonly JsonSerializerSettings ErrorToleranceSettings;

        private static readonly JsonSerializer Serializer;

        private static readonly ThreadLocal<bool> HasDeserializationError;

        static JsonSerialization()
        {
            HasDeserializationError = new(() => false);

            Settings = SetupCommonSettings(new());
            Serializer = JsonSerializer.CreateDefault(Settings);

            ErrorToleranceSettings = SetupCommonSettings(new()
            {
                Error = (_, args) =>
                {
                    HasDeserializationError.Value = true;
                    args.ErrorContext.Handled = true;
                },
                MissingMemberHandling = MissingMemberHandling.Error
            });
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

        public static object? FromJson(this string serialized, Type type) => JsonConvert.DeserializeObject(serialized, type, Settings);

        public static object? FromJson(this string serialized) => JsonConvert.DeserializeObject(serialized, Settings);

        public static T? FromJson<T>(this byte[]? content)
            where T : class
        {
            return content == null
                ? null
                : Encoding.UTF8.GetString(content).FromJson<T>();
        }

        public static bool TryFromJson<T>(this string serialized, out T? deserialized)
            where T : class
        {
            HasDeserializationError.Value = false;
            deserialized = JsonConvert.DeserializeObject<T>(serialized, ErrorToleranceSettings);
            return !HasDeserializationError.Value;
        }
        
        public static JsonSerializerSettings SetupCommonSettings(JsonSerializerSettings settings)
        {
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DateParseHandling = DateParseHandling.None;

            return settings;
        }
    }
}