using Cadl.ProviderHubController.Common;
using Newtonsoft.Json;
using VersioningSample1.Additions;

namespace SerializationTests.Models
{
    [Discriminator("type", "Dog")]
    [JsonConverter(typeof(DiscriminatorJsonConverter<Pet>))]
    public class Dog : Pet
    {
        private static readonly string DiscriminatorValue = "Dog";

        public Dog()
        {
            Type = DiscriminatorValue;
        }

        [JsonProperty(PropertyName = "akcBreed")]
        public string AkcBreed { get; set; }

        [JsonProperty(PropertyName = "akcId")]
        public string AkcId { get; set; }

        [JsonProperty(PropertyName = "nickname")]
        [Added(OnVersion = "2022-03-01")]
        public string Nickname { get; set; }
    }
}
