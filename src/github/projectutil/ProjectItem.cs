using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projectutil
{
    public class ProjectItem
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string[] Labels { get; set; }

        IDictionary<string, object> _fields = new Dictionary<string, object>();
        public IDictionary<string, object> Fields => _fields;

        public string ProjectId { get; set; }
    }
}
