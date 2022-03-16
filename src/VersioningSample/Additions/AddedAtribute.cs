using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cadl.ProviderHubController.Common
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property)]
    public class AddedAttribute : Attribute
    {
        public string OnVersion { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Property)]
    public class RemovedAttribute : Attribute
    {
        public string OnVersion { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ReplacedBy : Attribute
    {
        public string ReplacedByProperty { get; set; }
        public string OnVersion { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Replaces : Attribute
    {
        public string ReplacesProperty { get; set; }
        public string OnVersion { get; set; }
    }

}
