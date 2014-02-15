using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Umbraco.Inception.Converters
{
    public static class ConverterHelper
    {
        public static IMediaService GetMediaService()
        {
            return ApplicationContext.Current.Services.MediaService;
        }

        public static UmbracoHelper GetUmbracoHelper()
        {
            return new UmbracoHelper(UmbracoContext.Current);
        }
    }
}
