using System;
using System.Diagnostics;
using Cadl.ProviderHubController.Common.Additions;
using Microsoft.PlayFab.Service.Models;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace VersioningSample1
{

    class Program
    {
        static void Main(string[] args)
        {
            VersioningSerialization();
        }

        static void DiscriminatorSerialization()
        {
            var polySetting1 = new JsonSerializerSettings();
            polySetting1.Converters.Add(new PolymorphicDeserializeJsonConverter<ScoringFunction>("type"));
            var polySetting2 = new JsonSerializerSettings();
            polySetting1.Converters.Add(new PolymorphicSerializeJsonConverter<ScoringFunction>("type"));

            var x = new DistanceScoringFunction() { Type = "distance", Boost = 100, Distance = 11 };
            var str = JsonConvert.SerializeObject(x, polySetting2);
            var y = JsonConvert.DeserializeObject<ScoringFunction>(str, polySetting1);
            Debug.Assert(y.GetType() == x.GetType());
        }

        static void VersioningSerialization()
        {

            Console.WriteLine("Versioning Serialization");

            var pdp = new PlayerDatabaseProperties
            {
                Color = "blue",
                UserId = "James",
                Titles = new[] { "heavyweight", "middleweight" },
                Weight = 10,
                Function = new DistanceScoringFunction { Boost = 100, Distance = 7 }
            };

            var settings1 = new JsonSerializerSettings();
            var settings2 = new JsonSerializerSettings();
            // Add PolymophicConverter
            settings1.Converters.Add(new PolymorphicDeserializeJsonConverter<ScoringFunction>("type"));
            settings2.Converters.Add(new PolymorphicDeserializeJsonConverter<ScoringFunction>("type"));
            settings1.Converters.Add(new PolymorphicSerializeJsonConverter<ScoringFunction>("type"));
            settings2.Converters.Add(new PolymorphicSerializeJsonConverter<ScoringFunction>("type"));

            var builder = VersioningConverterBuilder.GetBuilder(new[] { "2022-01-01", "2022-03-01" });
            var converterV1 = new PlayerDatabasePropertiesConverter("2022-01-01"); //builder.GetConverter<PlayerDatabaseProperties>("2022-01-01");
            var converterV2 = new PlayerDatabasePropertiesConverter("2022-03-01"); //builder.GetConverter<PlayerDatabaseProperties>("2022-03-01");
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

            var pdpv1 = JsonConvert.DeserializeObject<PlayerDatabaseProperties>(@"{""UserId"":""James"",""ProvisioningState"":null,""Color"":""blue"",""Titles"":[""heavyweight"",""middleweight""],""Function"":{""distance"":7,""type"":""Distance"",""boost"":100}}", settings1);
            Console.WriteLine("V1 Json (+ color, - weight)");
            Console.WriteLine(JsonConvert.SerializeObject(pdpv1));
            var pdpv2 = JsonConvert.DeserializeObject<PlayerDatabaseProperties>(@"{""UserId"":""James"",""ProvisioningState"":null,""Weight"":10,""Titles"":[""heavyweight"",""middleweight""],""Function"":{""freshness"":77,""type"":""Freshness"",""boost"":100}}", settings2);
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

            Console.ReadLine();
        }
    }
}