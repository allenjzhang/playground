// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Cadl.ProviderHubController.Common;
using Newtonsoft.Json;

namespace VersioningSample1.Additions
{
    public class VersioningJsonConverter<T> : JsonConverter<T>
    {
        public VersioningJsonConverter(StringComparer comparer, string version)
        {
            _versionComparer = comparer;
            _version = version;
        }

        string _version { get; }

        StringComparer _versionComparer { get; }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            T result = serializer.Deserialize<T>(reader);
            foreach (var prop in typeof(T).GetProperties())
            {
                if (AddedAttribute.IsDefined(prop, typeof(AddedAttribute)))
                {
                    var added = AddedAttribute.GetCustomAttribute(prop, typeof(AddedAttribute)) as AddedAttribute;
                    if (_versionComparer.Compare(_version, added.OnVersion) < 0)
                    {
                        prop.SetValue(result, default);
                    }
                }

                if (RemovedAttribute.IsDefined(prop, typeof(RemovedAttribute)))
                {
                    var removed = RemovedAttribute.GetCustomAttribute(prop, typeof(RemovedAttribute)) as RemovedAttribute;
                    if (_versionComparer.Compare(_version, removed.OnVersion) >= 0)
                    {
                        prop.SetValue(result, default);
                    }
                }
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                if (AddedAttribute.IsDefined(prop, typeof(AddedAttribute)))
                {
                    var added = AddedAttribute.GetCustomAttribute(prop, typeof(AddedAttribute)) as AddedAttribute;
                    if (_versionComparer.Compare(_version, added.OnVersion) < 0)
                    {
                        prop.SetValue(value, default);
                    }
                }

                if (RemovedAttribute.IsDefined(prop, typeof(RemovedAttribute)))
                {
                    var removed = RemovedAttribute.GetCustomAttribute(prop, typeof(RemovedAttribute)) as RemovedAttribute;
                    if (_versionComparer.Compare(_version, removed.OnVersion) >= 0)
                    {
                        prop.SetValue(value, default);
                    }
                }
            }

            serializer.Converters.Remove(this);
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Serialize(writer, value, typeof(T));
        }
    }

}
