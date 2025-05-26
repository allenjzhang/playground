using Cadl.ProviderHubController.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PlayFab.Service.Models
{
    public partial class PlayerDatabase : VersionedTrackedResource<PlayerDatabaseProperties>
    {
        public string Flavor { get; set; }

        [Added(OnVersion ="2022-03-01")]
        public string Spin { get; set; }
    }
}
