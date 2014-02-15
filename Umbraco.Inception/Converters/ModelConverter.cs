using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Inception.Extensions;
using Umbraco.Inception.BL;


namespace Umbraco.Inception.Converters
{
    public class ModelConverter<T> : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            int id;
            if (int.TryParse(value as string, out id))
            {
                UmbracoHelper uh = ConverterHelper.GetUmbracoHelper();
                IPublishedContent content = uh.TypedContent(id);
                return content.ConvertToModel<T>();
            }

            return null;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(T));
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            try
            {
                T target = (T)value;
                UmbracoGeneratedBase baseObject = target as UmbracoGeneratedBase;
                if (baseObject != null && baseObject.UmbracoId != null)
                {
                    return baseObject.UmbracoId;
                }
                return -1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
