// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace VersioningSample1.Additions
{
    /// <summary>
    /// Base JSON converter for discriminated polymorphic objects.
    /// </summary>
    public class DiscriminatorJsonConverter<T> : JsonConverter
        where T : class
    {
        /// <summary>
        /// Initializes an instance of the PolymorphicDeserializeJsonConverter.
        /// </summary>
        /// <param name="discriminatorField">The JSON field used as a discriminator</param>
        public DiscriminatorJsonConverter(string discriminatorField)
        {

            if (discriminatorField == null)
            {
                throw new ArgumentNullException(nameof(discriminatorField));
            }
            Discriminator = discriminatorField;
        }

        /// <summary>
        /// Returns true for serialization.
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Returns true for deserialization.
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Discriminator property name.
        /// </summary>
        public string Discriminator { get; protected set; }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a JSON field and deserializes into an appropriate object based on discriminator
        /// field and object name. If JsonObject attribute is available, its value is used instead.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="objectType">The type of the object.</param>
        /// <param name="existingValue">The existing value.</param>
        /// <param name="serializer">The JSON serializer.</param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var item = JObject.Load(reader);
            var typeDiscriminator = (string)item[Discriminator];
            var resultType = GetDerivedType(objectType, typeDiscriminator) ?? objectType;

            // create instance of correct type
            var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(resultType);
            var result = contract.DefaultCreator();
            serializer.Populate(item.CreateReader(), result);

            return result as T;
        }

        private static Type GetDerivedType(Type baseType, string name)
        {
            foreach (TypeInfo type in baseType.GetTypeInfo().Assembly.DefinedTypes
                .Where(t => t.Namespace == baseType.Namespace && baseType.GetTypeInfo().IsAssignableFrom(t)))
            {
                string typeName = type.Name;
                if (type.GetCustomAttributes<DiscriminatorAttribute>().Any())
                {
                    var discriminatorValue = type.GetCustomAttribute<DiscriminatorAttribute>()?.Value;

                    if (discriminatorValue != null && discriminatorValue.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return type.AsType();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if the object being deserialized is assignable to the base type. False otherwise.
        /// </summary>
        /// <param name="objectType">The type of the object to check.</param>
        /// <returns>True if the object being deserialized is assignable to the base type. False otherwise.</returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(T).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        /// <summary>
        /// Case insensitive (and reduced version) of JToken.SelectToken which unfortunately does not offer
        /// such functionality and has made all potential extension points `internal`.
        /// </summary>
        private JToken SelectTokenCaseInsensitive(JObject obj, string path)
        {
            JToken result = obj;
            foreach (var pathComponent in path.Split('.'))
            {
                result = (result as JObject)?
                    .Properties()
                    .FirstOrDefault(p => string.Equals(p.Name, pathComponent, StringComparison.OrdinalIgnoreCase))?
                    .Value;
            }
            return result;
        }
    }
}

