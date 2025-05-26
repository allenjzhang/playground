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

        /// <summary>
        /// Gets or sets the discriminator field name.
        /// </summary>
        public string FieldName { get; set; }

        public string Value { get; set; }
    }
}
