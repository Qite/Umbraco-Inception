namespace Umbraco.Inception.Converters
{
    using Umbraco.Core;
    using Umbraco.Core.Services;
    using Umbraco.Web;

    /// <summary>
    /// The converter helper.
    /// </summary>
    public static class ConverterHelper
    {
        /// <summary>
        /// Gets the current media service.
        /// </summary>
        /// <returns>
        /// The <see cref="IMediaService"/>.
        /// </returns>
        public static IMediaService GetMediaService()
        {
            return ApplicationContext.Current.Services.MediaService;
        }

        /// <summary>
        /// Gets an <see cref="UmbracoHelper"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="UmbracoHelper"/>.
        /// </returns>
        public static UmbracoHelper GetUmbracoHelper()
        {
            return new UmbracoHelper(UmbracoContext.Current);
        }
    }
}
