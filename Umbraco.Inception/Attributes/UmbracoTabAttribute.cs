using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Inception.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UmbracoTabAttribute : Attribute
    {
        public string Name { get; set; }

        public UmbracoTabAttribute(string name)
        {
            Name = name;
        }
    }
}
