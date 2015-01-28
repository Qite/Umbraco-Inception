using System;

namespace Umbraco.Inception.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UmbracoTabAttribute : Attribute
    {
        public string Name { get; set; }
        public int SortOrder { get; set; }

        public UmbracoTabAttribute(string name)
        {
            Name = name;
            SortOrder = 0;
        }
        public UmbracoTabAttribute(string name, int sortOrder = 0)
        {
            Name = name;
            SortOrder = sortOrder;
        }
    }
}
