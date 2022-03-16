using Cadl.ProviderHubController.Common;
using Microsoft.PlayFab.Service.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using VersioningSample1.Additions;

namespace Cadl.ProviderHubController.Common.Additions
{
    public class VersionComparer : StringComparer
    {
        public const string CanonicalVersion = "__CANONICAL__";
        string[] _versions { get; }

        public VersionComparer(string[] versions)
        {
            _versions = versions;
        }
        public override int Compare(string x, string y)
        {
            if (x == CanonicalVersion || y == CanonicalVersion)
                return 0;
            int xIndex = -1, yIndex = -1;
            for (int i = 0; i < _versions.Length; ++i)
            {
                if (string.Equals(_versions[i], x, StringComparison.InvariantCultureIgnoreCase))
                    xIndex = i;
                if (string.Equals(_versions[i], y, StringComparison.InvariantCultureIgnoreCase))
                    yIndex = i;
            }

            if (xIndex == -1)
                throw new ArgumentException($"Invalid version {x}, valid api-versions include {string.Join(", ", _versions)}");
            if (yIndex == -1)
                throw new ArgumentException($"Invalid version {y}, valid api-versions include {string.Join(", ", _versions)}");

            return xIndex - yIndex;
        }

        public override bool Equals(string x, string y)
        {
            return Compare(x, y) == 0;
        }

        public override int GetHashCode(string obj)
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj);
        }
    }

    public class VersioningConverterBuilder
    {
        StringComparer _versionComparer { get; }

        public static VersioningConverterBuilder GetBuilder(string[] versions)
        {
            return new VersioningConverterBuilder(new VersionComparer(versions));
        }

        VersioningConverterBuilder(StringComparer comparer)
        {
            _versionComparer = comparer;
        }

        public JsonConverter<T> GetConverter<T>(string version)
        {
            return new VersioningJsonConverter<T>(_versionComparer, version);
        }
    }

    public interface IVersionedProperties
    {
        bool ShouldSerialize(JsonProperty property, string version, VersionComparer comparer);
    }


    public partial class PlayerDatabasePropertiesConverter : JsonConverter<PlayerDatabaseProperties>
    {
        protected string _version { get; }

        bool IsCanonical => _version == VersionComparer.CanonicalVersion;
        protected StringComparer _versionComparer { get; } = new VersionComparer(new[] { "2022-01-01", "2022-03-01" });

        public PlayerDatabasePropertiesConverter() : this(VersionComparer.CanonicalVersion)
        {
        }

        public PlayerDatabasePropertiesConverter(string version)
        {
            _version = version;
        }


        public override bool CanRead => true;
        public override PlayerDatabaseProperties ReadJson(JsonReader reader, Type objectType, PlayerDatabaseProperties existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            serializer.Converters.Remove(this);
            var deserializedValue = serializer.Deserialize<PlayerDatabaseProperties>(reader);
            // partial method to alter the json after deserializing all values
            AfterReadJson(deserializedValue);
            serializer.Converters.Add(this);
            return deserializedValue;
        }

        partial void BeforeWriteJson(PlayerDatabaseProperties value);
        partial void AfterReadJson(PlayerDatabaseProperties value);
        public override void WriteJson(JsonWriter writer, PlayerDatabaseProperties value, JsonSerializer serializer)
        {
            // partial method allowing type-specific changes to the model before serialization
            BeforeWriteJson(value);
            writer.WriteStartObject();
            if (value != null)
            {
                WriteBaseProperty(nameof(value.UserId), value.UserId, writer, serializer.NullValueHandling);
                WriteBaseProperty(nameof(value.ProvisioningState), value.ProvisioningState, writer, serializer.NullValueHandling);

                if (IsCanonical || (_versionComparer.Compare(_version, "2022-01-01") >= 0 && _versionComparer.Compare(_version, "2022-03-01") < 0))
                {
                    WriteBaseProperty(nameof(value.Color), value.Color, writer, serializer.NullValueHandling);
                }
                if (IsCanonical || (_versionComparer.Compare(_version, "2022-03-01") >= 0))
                {
                    WriteNullableProperty(nameof(value.Weight), value.Weight, writer, serializer.NullValueHandling);
                }

                WriteStringArray(nameof(value.Titles), value.Titles, writer, serializer.NullValueHandling);

            }
            writer.WriteEndObject();
        }

        void WriteBaseProperty<T>(string name, T value, JsonWriter writer, NullValueHandling nullHandling) where T : class
        {
            if (value != null || nullHandling == NullValueHandling.Include)
            {
                writer.WritePropertyName(name);
                writer.WriteValue(value);
            }
        }

        void WriteNullableProperty<T>(string name, T? value, JsonWriter writer, NullValueHandling nullHandling) where T : struct
        {
            if (value != null || nullHandling == NullValueHandling.Include)
            {
                writer.WritePropertyName(name);
            }
            if (value == null && nullHandling == NullValueHandling.Include)
            {
                writer.WriteNull();
            }
            if (value != null)
            {
                writer.WriteValue(value.Value);
            }
        }

        void WriteStringArray(string name, string[] value, JsonWriter writer, NullValueHandling nullHandling)
        {
            if (value != null || nullHandling == NullValueHandling.Include)
            {
                writer.WritePropertyName(name);
            }
            if (value == null && nullHandling == NullValueHandling.Include)
            {
                writer.WriteNull();
            }
            if (value != null)
            {
                writer.WriteStartArray();
                for (int i = 0; i < value.Length; ++i)
                {
                    writer.WriteValue(value[i]);
                }
                writer.WriteEndArray();
            }

        }

        void WriteComplexArray<T>(string name, T[] value, JsonWriter writer, JsonSerializer serializer) where T : class
        {
            if (value != null || serializer.NullValueHandling == NullValueHandling.Include)
            {
                writer.WritePropertyName(name);
            }
            if (value == null && serializer.NullValueHandling == NullValueHandling.Include)
            {
                writer.WriteNull();
            }
            if (value != null)
            {
                writer.WriteStartArray();
                for (int i = 0; i < value.Length; ++i)
                {
                    serializer.Serialize(writer, value, typeof(T[]));
                }
                writer.WriteEndArray();
            }

        }
    }
}
