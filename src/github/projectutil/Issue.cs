using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projectutil
{
    public class Issue
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Url { get; set; }

        public string[] Assignees { get; set; }

        public string[] Labels { get; set; }

        public string MileStone { get; set; }

        public string Repository { get; set; }

        public string State { get; set; }
    }
}
