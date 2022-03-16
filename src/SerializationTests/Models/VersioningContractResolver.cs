using System;
using System.Collections.Generic;
using System.Linq;
using Cadl.ProviderHubController.Common.Additions;

namespace SerializationTests.Models
{
    // Note that making this class partial gives an extra extensibility point - they can override any of the base class methods
    public partial class VersioningContractResolver : VersioningContractResolverBase
    {
        public static readonly VersionComparer ServiceVersionComparer = new VersionComparer(new[] { "2022-01-01", "2022-03-01" });
        public static string GetLibraryVersion(string serviceVersion)
        {
            switch (serviceVersion)
            {
                case "2022-01-01":
                    return "v2";
                case "2022-03-01":
                    return "v3";
                default:
                    throw new ArgumentException($"Invalid service version '{serviceVersion}'");
            }
        }

        public VersioningContractResolver() : base() { }

        public VersioningContractResolver(string version) : base(ServiceVersionComparer, GetLibraryVersion, version) { }

        // extensibility point for the applicability of this contract resolver
        partial void UpdateVersionedTypes(IList<Type> typesToVersion);

        protected override bool ShouldCreateServiceProperties(Type type)
        {
            var typesToVersion = new List<Type>();
            UpdateVersionedTypes(typesToVersion);
            return typesToVersion.Any( t => t == type) 
                || string.Equals(type?.Namespace, "SerializationTests.Models", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
