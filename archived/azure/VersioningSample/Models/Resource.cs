using Cadl.ProviderHubController.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cadl.ProviderHubController.Common
{
    public class VersionedTrackedResource<TProperties> where TProperties : class, new()
    {
        public string Id { get; set; }

        public string Location { get; set; }

        [Added(OnVersion = "v3")]
        public string AddedV3 { get; set; }

        [Removed(OnVersion = "v3")]
        public string RemovedV3 { get; set; }

        public TProperties Properties { get; set; }

    }

}
