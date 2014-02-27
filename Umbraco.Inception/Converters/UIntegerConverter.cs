using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Inception.Converters
{
    public class UIntegerConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string valueAsString = value as string;
            if (!string.IsNullOrEmpty(valueAsString))
            {
                int i;
                if (int.TryParse(valueAsString, out i))
                {
                    return i;
                }
            }

            if (value == null) return int.MinValue;

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (value == typeof(int))
            {
                return value;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
