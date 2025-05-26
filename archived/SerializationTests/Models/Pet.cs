using Newtonsoft.Json;
using VersioningSample1.Additions;

namespace SerializationTests.Models
{
    [Discriminator("type", default)]
    [JsonConverter(typeof(DiscriminatorJsonConverter<Pet>))]
    public class Pet
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }

        [JsonProperty(PropertyName = "age")]
        public int Age { get; set; }
    }
}
