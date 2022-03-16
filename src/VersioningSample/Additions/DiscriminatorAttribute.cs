using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersioningSample1.Additions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DiscriminatorAttribute : Attribute
    {
        public DiscriminatorAttribute(string fieldName, string value)
        {
            FieldName = fieldName;
            Value = value;
        }

        public string FieldName { get; set; }

        public string Value { get; set; }
    }
}
