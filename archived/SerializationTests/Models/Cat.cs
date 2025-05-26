using Cadl.ProviderHubController.Common;
using Microsoft.PlayFab.Service.Models;
using Newtonsoft.Json;
using VersioningSample1.Additions;

namespace SerializationTests.Models
{
    [Discriminator("type", "Cat")]
    [JsonConverter(typeof(DiscriminatorJsonConverter<Pet>))]
    public class Cat : Pet
    {
        private static readonly string DiscriminatorValue = "Cat";

        public Cat()
        {
            Type = DiscriminatorValue;
        }

        [JsonProperty(PropertyName = "acfaBreed")]
        public string AcfaBreed { get; set; }

        [JsonProperty(PropertyName = "acfaId")]
        public string AcfaId { get; set; }

        [JsonProperty(PropertyName = "longhair")]
        [Removed(OnVersion = "2022-01-01")]
        public bool IsLongHair { get; set; }
    }
}
