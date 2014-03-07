using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Inception.Attributes;
using System.Reflection;
using System.ComponentModel;
using Umbraco.Web;
using Umbraco.Core;
using Umbraco.Inception.BL;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Umbraco.Inception.Extensions
{
    public static class UmbracoCodeFirstExtensions
    {
        /// <summary>
        /// Extension used to convert an IPublishedContent back to a Typed model instance.
        /// Your model does need to inherit from UmbracoGeneratedBase and contain the correct attributes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public static T ConvertToModel<T>(this IPublishedContent content)
        {
            T instance = Activator.CreateInstance<T>();
            var propertiesWithTabsAttribute = typeof(T).GetProperties().Where(x => x.GetCustomAttribute<UmbracoTabAttribute>() != null);
            foreach (var property in propertiesWithTabsAttribute)
            {
                //tab instance
                UmbracoTabAttribute tabAttribute = property.GetCustomAttribute<UmbracoTabAttribute>();
                var propertyTabInstance = Activator.CreateInstance(property.PropertyType);
                var propertiesOnTab = property.PropertyType.GetProperties();

                foreach (var propertyOnTab in propertiesOnTab.Where(x => x.GetCustomAttribute<UmbracoPropertyAttribute>() != null))
                {
                    UmbracoPropertyAttribute propertyAttribute = propertyOnTab.GetCustomAttribute<UmbracoPropertyAttribute>();
                    string alias = HyphenToUnderscore(ParseUrl(propertyAttribute.Alias + "_" + tabAttribute.Name, false));
                    GetPropertyValueOnInstance(content, propertyTabInstance, propertyOnTab, propertyAttribute, alias);
                }

                property.SetValue(instance, propertyTabInstance);

            }

            //properties on Generic Tab
            var propertiesOnGenericTab = typeof(T).GetProperties().Where(x => x.GetCustomAttribute<UmbracoPropertyAttribute>() != null);
            foreach (var item in propertiesOnGenericTab)
            {
                UmbracoPropertyAttribute umbracoPropertyAttribute = item.GetCustomAttribute<UmbracoPropertyAttribute>();
                GetPropertyValueOnInstance(content, instance, item, umbracoPropertyAttribute);
            }

            (instance as UmbracoGeneratedBase).UmbracoId = content.Id;
            return instance;
        }

        private static void GetPropertyValueOnInstance(IPublishedContent content, object objectInstance, PropertyInfo propertyOnTab, UmbracoPropertyAttribute propertyAttribute, string alias = null)
        {
            if (alias == null) alias = propertyAttribute.Alias;
            string umbracoStoredValue = content.GetPropertyValue<string>(alias);

            if (propertyAttribute.ConverterType != null)
            {
                TypeConverter converter = (TypeConverter)Activator.CreateInstance(propertyAttribute.ConverterType);
                propertyOnTab.SetValue(objectInstance, converter.ConvertFrom(null, CultureInfo.InvariantCulture, umbracoStoredValue));
            }
            else
            {
                propertyOnTab.SetValue(objectInstance, umbracoStoredValue);
            }
        }

        /// <summary>
        /// Function to parse an URL to a better format
        /// </summary>
        /// <param name="input">The URL that needs to be parsed</param>
        /// <returns></returns>
        public static string ParseUrl(string input, bool toLowerCase = true, bool allowComas = false)
        {
            input = input.Replace(" ", "-");
            input = input.Replace("é", "e");
            input = input.Replace("è", "e");
            input = input.Replace("à", "a");
            input = input.Replace("â", "a");
            input = input.Replace("ç", "c");
            input = input.Replace("î", "i");
            input = input.Replace("ï", "i");
            input = input.Replace("ë", "e");
            input = input.Replace("Ë", "e");
            input = input.Replace("ô", "o");
            input = input.Replace("ù", "u");
            string returnValue = "";

            for (int i = 0; i < input.Length; i++)
            {
                string s = input.Substring(i, 1);
                string regexString = "";

                if (allowComas)
                {
                    regexString = "[A-z0-9,/\\-\\(\\)_]";
                }
                else
                {
                    regexString = "[A-z0-9/\\-\\(\\)_]";
                }

                Regex reg = new Regex(regexString);
                if (reg.IsMatch(s))
                    returnValue += s;
            }

            // If multiple spaces (-), clear to 1
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[\\-]{2,}", options);
            returnValue = regex.Replace(returnValue, @"-");

            // In case of all bad characters
            if (returnValue == "")
            {
                returnValue = "empty";
            }

            // Lowercase urls better for SEO
            if (toLowerCase)
            {
                return returnValue.ToLower();
            }

            return returnValue;
        }


        /// <summary>
        /// Replace hyphen ('-') with underscore ('_')
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string HyphenToUnderscore(string input)
        {
            return input.Replace('-', '_');
        }
    }
}
