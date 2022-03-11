using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cadl.ProviderHubController.Common;
using Newtonsoft.Json;

namespace Microsoft.PlayFab.Service.Models {
    [Discriminator(Value = "Distance")]
    [JsonObject("Distance")]
    public partial class DistanceScoringFunction : ScoringFunction
    {
        private static readonly string DiscriminatorValue = "Distance";
        public DistanceScoringFunction()
        {
            Type = DiscriminatorValue;
        }

        [JsonProperty(PropertyName = "distance")]
        public int Distance { get; set; }
    }
}