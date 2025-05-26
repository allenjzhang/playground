using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cadl.ProviderHubController.Common;
using Newtonsoft.Json;
using VersioningSample1.Additions;

namespace Microsoft.PlayFab.Service.Models {
    [Discriminator("type", "Freshness")]
    [JsonConverter(typeof(DiscriminatorJsonConverter<ScoringFunction>))]
    public partial class FreshnessScoringFunction : ScoringFunction {

        private static readonly string DiscriminatorValue = "Freshness";
        
        public FreshnessScoringFunction()
        {
            Type = DiscriminatorValue;
        }

        [JsonProperty(PropertyName = "freshness")]
        public int Freshness { get; set; }
    }
}