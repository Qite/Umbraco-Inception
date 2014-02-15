using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Inception.Converters
{
    /// <summary>
    /// Umbraco saves bools as 1 & 0. This parser converts them to booleans
    /// </summary>
    public class UBooleanConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            bool parsedValue;
            if (value == null) return -1;
            if (bool.TryParse(value.ToString(), out parsedValue))
            {
                if (parsedValue == true) return 1;
            }

            return 0;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType != typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value == null) return null;
            bool parsedValue;

            if (bool.TryParse(value.ToString(), out parsedValue))
            {
                return parsedValue;
            }

            return null;
        }
    }
}
