using Cadl.ProviderHubController.Common.Additions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersioningSample1.Additions
{
    // Virtual factory - extensibility point for custom serialization
    public class VersionedSerializer
    {
        public static string[] Versions { get; } = new[] { "2022-01-01", "2022-03-01" };

        protected static IDictionary<string, JsonSerializerSettings> Settings { get; } = new ConcurrentDictionary<string, JsonSerializerSettings>(StringComparer.OrdinalIgnoreCase);

        private IList<JsonConverter> Converters = new List<JsonConverter>();

        // Canonincal Version
        protected virtual VersioningContractResolver GetResolver()
        {
            return new VersioningContractResolver();
        }
        protected virtual VersioningContractResolver GetResolver(string version)
        {
            if (Versions.Contains(version, StringComparer.InvariantCultureIgnoreCase))
                return new VersioningContractResolver(version);
            throw new ArgumentException($"Version '{version}' is not valid. Please choose from '{string.Join(", ", Versions)}'");
        }

        protected virtual IList<JsonConverter> GetConverters(string version)
        {
            // TODO version specific converter
            return Converters;
        }

        protected virtual IList<JsonConverter> GetConverters()
        {
            return Converters;
        }

        public virtual void AddConverters(JsonConverter converter)
        {
            Converters.Add(converter);
        }


        public virtual JsonSerializerSettings GetJsonSerializerSettings(string version)
        {
            if (Settings.ContainsKey(version))
                return Settings[version];

            var settings = new JsonSerializerSettings { ContractResolver = GetResolver(version) };
            var converters = GetConverters(version);
            if (converters != null)
            {
                settings.Converters = converters;
            }

            Settings[version] = settings;
            return settings;
        }

        public virtual JsonSerializerSettings GetJsonSerializerSettings()
        {
            if (Settings.ContainsKey(VersionComparer.CanonicalVersion))
                return Settings[VersionComparer.CanonicalVersion];
            var settings = new JsonSerializerSettings { ContractResolver = GetResolver() };
            var converters = GetConverters();
            if (converters != null)
            {
                settings.Converters = converters;
            }

            Settings[VersionComparer.CanonicalVersion] = settings;
            return settings;
        }


        public async Task<T> DeserializeAsync<T>(HttpRequest request) 
        {
            string version = request?.Query?["api-version"];

            // need to make this a safe read, using buffers and maximum size
            var reader = new StreamReader(request.Body, Encoding.UTF8, true);
            if (!string.IsNullOrWhiteSpace(version))
                return Deserialize<T>(await reader.ReadToEndAsync(), version);
            return Deserialize<T>(await reader.ReadToEndAsync());

        }

        public T Deserialize<T>(string content, string version) 
        {
            return JsonConvert.DeserializeObject<T>(content, GetJsonSerializerSettings(version));
        }

        public T Deserialize<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content, GetJsonSerializerSettings());
        }


        public string Serialize<T>(T content, string version)
        {
            return JsonConvert.SerializeObject(content, GetJsonSerializerSettings(version));
        }

        public string Serialize<T>(T content)
        {
            return JsonConvert.SerializeObject(content, GetJsonSerializerSettings());
        }

    }
}
