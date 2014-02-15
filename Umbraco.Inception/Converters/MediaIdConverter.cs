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
    /// If you use the MediaPicker, an Id of the media is stored into Umbraco
    /// This Converter converts an MediaId to the direct url of the image
    /// The Convert backmethod expects an valid image url f.ex /media/1002/1502/your-image.jpg
    /// And then it can persist it into the document
    /// </summary>
    public class MediaIdConverter : TypeConverter
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
            int mediaId;
            if (int.TryParse(value.ToString(), out mediaId))
            {
                if (mediaId < 1) return null;

                var umbracoHelper = ConverterHelper.GetUmbracoHelper();
                try
                {
                    IPublishedContent media = umbracoHelper.TypedMedia(mediaId);
                    return media.Url;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    try
                    {
                        var ms = ConverterHelper.GetMediaService();
                        IMedia media = ms.GetById(mediaId);
                        if (media != null) return media.GetValue("umbracoFile");
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }

            }
            return null;
        }
    }
}
