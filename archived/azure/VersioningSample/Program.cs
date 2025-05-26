using System;
using System.Diagnostics;
using Cadl.ProviderHubController.Common.Additions;
using Microsoft.PlayFab.Service.Models;
using Newtonsoft.Json;
using VersioningSample1.Additions;

namespace VersioningSample1
{

    class Program
    {
        static void Main(string[] args)
        {
            DiscriminatorSerialization();

            //VersioningSerializationWithResolver();
            //var test = new DiscriminatorTests();
            //test.SerializeSingle();
        }

        static void DiscriminatorSerialization()
        {
            var serializer = new VersionedSerializer();
            //serializer.AddConverters(new DiscriminatorJsonConverter<ScoringFunction>());
            var settings = serializer.GetJsonSerializerSettings("2022-01-01");
            var distance = new DistanceScoringFunction() { Type = "distance", Boost = 100, Distance = 11 };
            var v1Str = JsonConvert.SerializeObject(distance, Formatting.Indented, settings);
            var backObj = JsonConvert.DeserializeObject<ScoringFunction>(v1Str, settings);
            Console.WriteLine(backObj.GetType().Name);
            Debug.Assert(backObj.GetType() == distance.GetType());
        }

        static void VersioningSerializationWithResolver()
        {
            Console.WriteLine("Versioning Serialization with ContractResolver");

            var pdp = new PlayerDatabaseProperties
            {
                Color = "blue",
                UserId = "James",
                Titles = new[] { "heavyweight", "middleweight" },
                Weight = 10,
                Function = new DistanceScoringFunction { Boost = 100, Distance = 7, AdvancedDistance = 77},
            };

            var serializer = new VersionedSerializer();
            //serializer.AddConverters(new DiscriminatorJsonConverter<ScoringFunction>());
            var settings1 = serializer.GetJsonSerializerSettings("2022-01-01");
            Console.WriteLine("V1 Json (+ color, - weight)");
            Console.WriteLine(JsonConvert.SerializeObject(pdp, settings1));

            var settings2 = serializer.GetJsonSerializerSettings("2022-03-01");

            Console.WriteLine("V2 Json (- color, + weight)");
            Console.WriteLine(JsonConvert.SerializeObject(pdp, settings2));

            var pd = new PlayerDatabase
            {
                Flavor = "StrawBerry",
                Spin = "Up",
                Id = "/subscriptions/foo/resourcegroups/foo2/Microsoft.PlayFab/playerDatabases/pd",
                Location = "West Europe",
                AddedV3 = "Hope this is v2",
                RemovedV3 = "Hope this is v1",
                Properties = pdp,
            };

            var v1Str = JsonConvert.SerializeObject(pd, Formatting.Indented, serializer.GetJsonSerializerSettings());

            //settings1.Converters.Add(new DiscriminatorJsonConverter<ScoringFunction>());
            //settings1.Converters.Add(new DiscriminatorJsonConverter<ScoringFunction>());


            Console.WriteLine("V1 Json (+ removedV2, - addedV2, + spin, properties:(+ color, - weight)");
            Console.WriteLine(v1Str);
            //Console.WriteLine("V2 Json (- removedV2, + addedV2, - spin, properties:(- color, + weight)");
            //Console.WriteLine(v2Str);
            //Console.WriteLine("Canonical Version");
            //Console.WriteLine(canonicalStr);

            var roundtripObj = JsonConvert.DeserializeObject<PlayerDatabase>(v1Str, settings1);
            Console.WriteLine(roundtripObj.Properties.Function.GetType().Name);
            var roundtripObj2 = JsonConvert.DeserializeObject<PlayerDatabase>(v1Str, settings2);
            Console.WriteLine(roundtripObj.Properties.Function.GetType().Name);
        }

        static void VersioningSerialization()
        {

            Console.WriteLine("Versioning Serialization");

            var pdp = new PlayerDatabaseProperties
            {
                Color = "blue",
                UserId = "James",
                Titles = new[] {"heavyweight", "middleweight"},
                Weight = 10,
                Function = new DistanceScoringFunction { Boost = 100, Distance = 7 },
            };
            

            var settings1 = new JsonSerializerSettings();
            var settings2 = new JsonSerializerSettings();
            var builder = VersioningConverterBuilder.GetBuilder(new[] { "2022-01-01", "2022-03-01" });
            var converterV1 = new PlayerDatabasePropertiesConverter("2022-01-01");//builder.GetConverter<PlayerDatabaseProperties>("2022-01-01");
            var converterV2 = new PlayerDatabasePropertiesConverter("2022-03-01");//builder.GetConverter<PlayerDatabaseProperties>("2022-03-01");
            settings1.Converters.Add(converterV1);
            settings2.Converters.Add(converterV2);
            var v1 = JsonConvert.SerializeObject(pdp, settings1);
            Console.WriteLine("V1 Json (+ color, - weight)");
            Console.WriteLine(v1);
            Console.WriteLine();

            //pdp = pdp = new PlayerDatabaseProperties
            //{
            //    Color = "blue",
            //    UserId = "James",
            //    Weight = 10
            //};

            var v2 = JsonConvert.SerializeObject(pdp, settings2);
            Console.WriteLine("V2 Json (- color, + weight)");
            Console.WriteLine(v2);
            Console.WriteLine();

            Console.WriteLine("Versioning Deserialization");

            var pdpv1 = JsonConvert.DeserializeObject<PlayerDatabaseProperties>(@"{""UserId"":""James"",""ProvisioningState"":null,""Color"":""blue"",""Titles"":[""heavyweight"",""middleweight""]}", settings1);
            Console.WriteLine("V1 Json (+ color, - weight)");
            Console.WriteLine(JsonConvert.SerializeObject(pdpv1));
            var pdpv2 = JsonConvert.DeserializeObject<PlayerDatabaseProperties>(@"{""UserId"":""James"",""ProvisioningState"":null,""Weight"":10,""Titles"":[""heavyweight"",""middleweight""]}", settings2);
            Console.WriteLine("V2 Json (- color, + weight)");
            Console.WriteLine(JsonConvert.SerializeObject(pdpv2));

            Console.WriteLine("Roundtrip v1 -> v1");
            Console.WriteLine(JsonConvert.SerializeObject(pdpv1, settings1));

            Console.WriteLine("Roundtrip v2 -> v2");
            Console.WriteLine(JsonConvert.SerializeObject(pdpv2, settings2));

            Console.WriteLine("Roundtrip v1 -> v2");
            Console.WriteLine(JsonConvert.SerializeObject(pdpv1, settings2));

            Console.WriteLine("Roundtrip v2 -> v1");
            Console.WriteLine(JsonConvert.SerializeObject(pdpv2, settings1));
        }
    }
}
