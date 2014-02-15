using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Inception.Converters
{
    /// <summary>
    /// Does the same as the MediaConverter but for the multiple MediaPicker
    /// </summary>
    public class MultipleMediaIdConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (value == null) return -1;
            string valueContent = value.ToString();
            IMediaService ms = ConverterHelper.GetMediaService();
            IMedia media = ms.GetMediaByPath(valueContent);
            if (media != null) return media.Id;
            return -1;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType != typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value == null) return null;
            string[] mediaIdArray = value.ToString().Split(',');
            string[] mediaUrlArray = new string[mediaIdArray.Length];

            for (int i = 0; i < mediaIdArray.Length; i++)
            {
                int mediaId;

                if (int.TryParse(mediaIdArray[i], out mediaId))
                {
                    if (mediaId < 1) return null;

                    var umbracoHelper = ConverterHelper.GetUmbracoHelper();
                    try
                    {
                        IPublishedContent media = umbracoHelper.TypedMedia(mediaId);
                        mediaUrlArray[i] = media.Url;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        return null;
                    }
                }
            }

            return mediaUrlArray;
        }
    }
}
