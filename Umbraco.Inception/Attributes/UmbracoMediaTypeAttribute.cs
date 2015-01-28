using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Inception.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UmbracoMediaTypeAttribute : Attribute
    {
        public string MediaTypeName { get; set; }

        public string MediaTypeAlias { get; set; }
    
        public Type[] AllowedChildren { get; set; }

        public bool AllowedAtRoot { get; set; }

        public bool EnableListView { get; set; }

        public string Icon { get; set; }

        public string Description { get; set; }

        public UmbracoMediaTypeAttribute(string contentTypeName, string contentTypeAlias, Type[] allowedChildren, 
            string description = "", string icon = "icon-folder", bool allowAtRoot = false, bool enableListView = false)
        {
            MediaTypeName = contentTypeName;
            MediaTypeAlias = contentTypeAlias;
            EnableListView = enableListView;
            AllowedAtRoot = allowAtRoot;
            AllowedChildren = allowedChildren;
            Icon = icon;
            Description = description;
        }
    }
}
