namespace Umbraco.Inception.Converters
{
    using System;
    using System.ComponentModel;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;
    using Umbraco.Web;

    /// <summary>
    /// Does the same as the MediaConverter but for the multiple MediaPicker
    /// </summary>
    public class MultipleMediaIdConverter : TypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
        /// <returns>
        /// true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            // Always call the base to see if it can perform the conversion. 
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
        /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
        /// <returns>
        /// An <see cref="T:System.Object" /> that represents the converted value.
        /// </returns>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value != null)
            {
                string[] mediaIdArray = value.ToString().Split(',');
                string[] mediaUrlArray = new string[mediaIdArray.Length];

                for (int i = 0; i < mediaIdArray.Length; i++)
                {
                    int mediaId;

                    if (int.TryParse(mediaIdArray[i], out mediaId))
                    {
                        if (mediaId > 0)
                        {
                            UmbracoHelper umbracoHelper = ConverterHelper.GetUmbracoHelper();
                            IPublishedContent media = umbracoHelper.TypedMedia(mediaId);
                            mediaUrlArray[i] = media.Url;
                        }
                    }
                }

                return mediaUrlArray;
            }

            // Always call base, even if you can't convert. 
            return base.ConvertFrom(context, culture, null);
        }

        /// <summary>
        /// Returns whether this converter can convert the object to the specified type, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you want to convert to.</param>
        /// <returns>
        /// true if this converter can perform the conversion; otherwise, false.
        /// </returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }

            // Always call the base to see if it can perform the conversion. 
            return base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        /// Converts the given value object to the specified type, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" />. If null is passed, the current culture is assumed.</param>
        /// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
        /// <param name="destinationType">The <see cref="T:System.Type" /> to convert the <paramref name="value" /> parameter to.</param>
        /// <returns>
        /// An <see cref="T:System.Object" /> that represents the converted value.
        /// </returns>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (value != null)
            {
                string valueContent = value.ToString();
                IMediaService mediaService = ConverterHelper.GetMediaService();
                IMedia media = mediaService.GetMediaByPath(valueContent);
                if (media != null)
                {
                    return media.Id;
                }
            }

            // Always call base, even if you can't convert. 
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
