using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cadl.ProviderHubController.Common;
using Newtonsoft.Json;
using VersioningSample1.Additions;

namespace Microsoft.PlayFab.Service.Models {
    [Discriminator("type", "Distance")]
    [JsonConverter(typeof(DiscriminatorJsonConverter<ScoringFunction>))]
    public partial class DistanceScoringFunction : ScoringFunction
    {
        private static readonly string DiscriminatorValue = "Distance";
        public DistanceScoringFunction()
        {
            Type = DiscriminatorValue;
        }

        [JsonProperty(PropertyName = "distance")]
        public int Distance { get; set; }

        [JsonProperty(PropertyName = "advancedDistance")]
        [Added(OnVersion = "2022-03-01")]
        public int AdvancedDistance { get; set; }
    }
}