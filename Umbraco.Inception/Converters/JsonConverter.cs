using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Inception.Converters
{
    /// <summary>
    /// Converts a json string to the object. Internally Json.Net is used so if you can serialize and deserialize your object with Newton's Json.Net than you are safe to use this Converter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonConverter<T> : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string json = value as string;
            if (json != null)
            {
                try
                {
                    T target = JsonConvert.DeserializeObject<T>(json);
                    return target;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

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
                return JsonConvert.SerializeObject(target);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
