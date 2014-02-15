using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Inception.Converters
{
    /// <summary>
    /// Converts the datetime string to a date
    /// Convert back doesn't do anything with the dateTime value
    /// Because Umbraco knows how to store DateTime
    /// </summary>
    public class UDateTimeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return value;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType != typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string valueContent = value.ToString();
            DateTime dateTime;
            if (DateTime.TryParse(valueContent, out dateTime))
            {
                return dateTime;
            }
            return null;
        }
    }
}
