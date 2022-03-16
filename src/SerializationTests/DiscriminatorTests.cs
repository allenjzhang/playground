using System;
using Cadl.ProviderHubController.Common.Additions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SerializationTests.Models;
using VersioningSample1.Additions;

namespace SerializationTests
{
    public class DiscriminatorTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [Test]
        public void SerializeSingle()
        {
            Pet p = GeneratePet("dog");

            var jsonStr = JsonConvert.SerializeObject(p, GetSerializerSettings("2022-03-01"));
            Assert.IsTrue(JObject.DeepEquals(JToken.FromObject(p), JToken.Parse(jsonStr)));
        }

        [Test]
        public void SerializeSingleVersioned()
        {
            Pet p = GeneratePet("dog");

            var jsonStr = JsonConvert.SerializeObject(p, GetSerializerSettings("2022-01-01"));
            // Should not match original one
            Assert.IsFalse(JObject.DeepEquals(JToken.FromObject(p), JToken.Parse(jsonStr)));

            var jDog = new JObject
            {
                { "akcBreed", "Cavapoo" },
                { "akcId", "A10000" },
                { "type", "Dog" },
                { "gender", "F" },
                { "age", 2 }
            };
            // Should match versioned one
            Assert.IsTrue(JObject.DeepEquals(jDog, JToken.Parse(jsonStr)));
        }


        public void DeserializeSingleVersioned()
        { }

        public void DeserializeNested()
        {

        }

        public void SerializeList()
        {

        }

        public void DeserializeList()
        { }

        private JsonSerializerSettings GetSerializerSettings(string version = VersionComparer.CanonicalVersion)
        {
            var serializer = new VersionedSerializer();
            //serializer.AddConverters(new DiscriminatorJsonConverter<Pet>("type"));

            return serializer.GetJsonSerializerSettings(version);
        }

        private Pet GeneratePet(string type) => type switch
        {
            "dog" => new Dog { Age = 2, AkcBreed = "Cavapoo", AkcId = "A10000", Gender = "F", Nickname = "Kiwi" },
            "cat" => new Cat { Age = 7, AcfaBreed = "Persian", AcfaId = "B9999", Gender = "M" },
            _ => throw new ArgumentException(nameof(type)),

        };
    }
}