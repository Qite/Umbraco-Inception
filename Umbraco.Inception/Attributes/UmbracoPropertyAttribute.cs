using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Inception.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UmbracoPropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string DataType { get; set; }
        public string DataTypeInstanceName { get; set; }
        public string Tab { get; set; }
        public bool Mandatory { get; set; }
        public string Description { get; set; }
        public Type ConverterType { get; set; }
        public int SortOrder { get; set; }
        public bool AddTabAliasToPropertyAlias { get; set; }

        /// <summary>
        /// Put this on properties of your class that inherits from TabBase
        /// </summary>
        /// <param name="name">Friendly name of the property</param>
        /// <param name="alias">Alias of the property</param>
        /// <param name="dataType">Alias of your propertyEditor</param>
        /// <param name="dataTypeInstanceName">Name of the instance of your property editor, leave empty if you are using a built-in</param>
        /// <param name="converterType">Converter class that inherits TypeConverter</param>
        /// <param name="mandatory">if set to <c>true</c> [mandatory].</param>
        /// <param name="description">The description.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="addTabAliasToPropertyAlias">if set to <c>true</c> add's the tab's alias as a suffix to the property alias.</param>
        public UmbracoPropertyAttribute(string name, string alias, string dataType, string dataTypeInstanceName = null, Type converterType = null, bool mandatory = false, string description = "", int sortOrder = 0, bool addTabAliasToPropertyAlias=true)
        {
            Name = name;
            Alias = alias;
            DataType = dataType;
            DataTypeInstanceName = dataTypeInstanceName;
            ConverterType = converterType;
            Mandatory = mandatory;
            Description = description;
            SortOrder = sortOrder;
            AddTabAliasToPropertyAlias = addTabAliasToPropertyAlias;
        }
    }

    public static class BuiltInUmbracoDataTypes
    {
        public const string Boolean = "Umbraco.TrueFalse";
        public const string Integer = "Umbraco.Integer";
        public const string TinyMce = "Umbraco.TinyMCEv3";
        public const string Textbox = "Umbraco.Textbox";
        public const string TextboxMultiple = "Umbraco.TextboxMultiple";
        public const string UploadField = "Umbraco.UploadField";
        public const string NoEdit = "Umbraco.NoEdit";
        public const string DateTime = "Umbraco.DateTime";
        public const string ColorPicker = "Umbraco.ColorPickerAlias";
        public const string FolderBrowser = "Umbraco.FolderBrowser";
        public const string DropDownMultiple = "Umbraco.DropDownMultiple";
        public const string RadioButtonList = "Umbraco.RadioButtonList";
        public const string Date = "Umbraco.Date";
        public const string DropDown = "Umbraco.DropDown";
        public const string CheckBoxList = "Umbraco.CheckBoxList";
        public const string ContentPickerAlias = "Umbraco.ContentPickerAlias";
        public const string MediaPicker = "Umbraco.MediaPicker";
        public const string MemberPicker = "Umbraco.MemberPicker";
        public const string RelatedLinks = "Umbraco.RelatedLinks";
        public const string Tags = "Umbraco.Tags";
        public const string MultipleMediaPicker = "Umbraco.MultipleMediaPicker";
    } 
}
