using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Inception.Attributes;
using Umbraco.Inception.Extensions;

namespace Umbraco.Inception.BL
{
    /// <summary>
    /// Your models needs to inherit this Base class in order to go both ways,
    /// and persist changes to the model back to Umbraco.
    /// </summary>
    public abstract class UmbracoGeneratedBase
    {
        public int? UmbracoId { get; set; }

        /// <summary>
        /// Abstract base class for any entity you want to be generated
        /// The persist method makes sure that you can save all the changes to you entity back into the database
        /// </summary>
        /// <param name="contentId">Id of the Umbraco Document</param>
        /// <param name="userId"></param>
        /// <param name="raiseEvents"></param>
        public virtual void Persist(int contentId, int userId = 0, bool raiseEvents = false)
        {
            IContentService contentSerivce = ApplicationContext.Current.Services.ContentService;
            IContent content = contentSerivce.GetById(contentId);

            //search for propertys with the UmbracoTab on
            Type currentType = this.GetType();
            var propertiesWithTabAttribute = currentType.GetProperties().Where(x => x.GetCustomAttributes<UmbracoTabAttribute>() != null).ToArray();
            int length = propertiesWithTabAttribute.Count();
            for (int i = 0; i < length; i++)
            {
                PropertyInfo tabProperty = propertiesWithTabAttribute[i];
                Type tabType = tabProperty.PropertyType;
                object instanceOfTab = tabProperty.GetValue(this);
                UmbracoTabAttribute tabAttribute = tabProperty.GetCustomAttribute<UmbracoTabAttribute>();

                //persist the fields foreach tab
                var propertiesInsideTab = tabType.GetProperties().Where(x => x.GetCustomAttribute<UmbracoPropertyAttribute>() != null).ToArray();
                int propertyLength = propertiesInsideTab.Length;
                for (int j = 0; j < propertyLength; j++)
                {
                    PropertyInfo property = propertiesInsideTab[j];
                    UmbracoPropertyAttribute umbracoPropertyAttribute = property.GetCustomAttribute<UmbracoPropertyAttribute>();
                    object propertyValue = property.GetValue(instanceOfTab);
                    string alias = UmbracoCodeFirstExtensions.HyphenToUnderscore(UmbracoCodeFirstExtensions.ParseUrl(umbracoPropertyAttribute.Alias + "_" + tabAttribute.Name, false));
                    SetPropertyOnIContent(content, umbracoPropertyAttribute, propertyValue, alias);
                }
            }

            //properties on generic tab
            var propertiesOnGenericTab = currentType.GetProperties().Where(x => x.GetCustomAttribute<UmbracoPropertyAttribute>() != null);
            foreach (var item in propertiesOnGenericTab)
            {
                UmbracoPropertyAttribute umbracoPropertyAttribute = item.GetCustomAttribute<UmbracoPropertyAttribute>();
                object propertyValue = item.GetValue(this);
                SetPropertyOnIContent(content, umbracoPropertyAttribute, propertyValue);
            }

            //persist object into umbraco database
            contentSerivce.SaveAndPublishWithStatus(content, userId, raiseEvents);
        }

        private void SetPropertyOnIContent(IContent content, UmbracoPropertyAttribute umbracoPropertyAttribute, object propertyValue, string alias = null)
        {
            object convertedValue;
            if (umbracoPropertyAttribute.ConverterType != null)
            {

                TypeConverter converter = (TypeConverter)Activator.CreateInstance(umbracoPropertyAttribute.ConverterType);
                convertedValue = converter.ConvertTo(null, CultureInfo.InvariantCulture, propertyValue, typeof(string));
            }
            else
            {
                //No converter is given so basically we push the string back into umbraco
                convertedValue = (propertyValue != null ? propertyValue.ToString() : string.Empty);
            }

            if (alias == null) alias = umbracoPropertyAttribute.Alias;

            content.SetValue(alias, convertedValue);
        }

    }
}
