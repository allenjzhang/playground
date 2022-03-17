using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VersioningSample1.Additions;

namespace Microsoft.PlayFab.Service.Models {

    [Discriminator("type", default)]
    [JsonConverter(typeof(DiscriminatorJsonConverter<ScoringFunction>))]
    public partial class ScoringFunction {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "boost")]
        public int Boost { get; set; }
    }
}