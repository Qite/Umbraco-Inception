using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Inception.Attributes;
using Umbraco.Inception.BL;
using Umbraco.Inception.Extensions;

namespace Umbraco.Inception.CodeFirst
{
    public static class UmbracoCodeFirstInitializer
    {
        private const string ViewsFolderDefaultLocation = "~/Views";

        /// <summary>
        /// This method will create or update the Content Type in Umbraco.
        /// It's possible that you need to run this method a few times to create all relations between Content Types.
        /// </summary>
        /// <param name="type">The type of your model that contains an UmbracoContentTypeAttribute</param>
        public static void CreateOrUpdateEntity(Type type)
        {
            var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
            var fileService = ApplicationContext.Current.Services.FileService;
            var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

            UmbracoContentTypeAttribute attribute = type.GetCustomAttribute<UmbracoContentTypeAttribute>();
            if (attribute == null) return;

            if (!contentTypeService.GetAllContentTypes().Any(x => x != null && x.Alias == attribute.ContentTypeAlias))
            {
                CreateContentType(contentTypeService, fileService, attribute, type, dataTypeService);
            }
            else
            {
                //update
                IContentType contentType = contentTypeService.GetContentType(attribute.ContentTypeAlias);
                UpdateContentType(contentTypeService, fileService, attribute, contentType, type, dataTypeService);
            }
        }

        #region Create

        /// <summary>
        /// This method is called when the Content Type declared in the attribute hasn't been found in Umbraco
        /// </summary>
        /// <param name="contentTypeService"></param>
        /// <param name="fileService"></param>
        /// <param name="attribute"></param>
        /// <param name="type"></param>
        /// <param name="dataTypeService"></param>
        private static void CreateContentType(IContentTypeService contentTypeService, IFileService fileService,
            UmbracoContentTypeAttribute attribute, Type type, IDataTypeService dataTypeService)
        {
            IContentType newContentType;
            Type parentType = type.BaseType;
            if (parentType != null && parentType != typeof(UmbracoGeneratedBase) && parentType.GetBaseTypes(false).Any(x => x == typeof(UmbracoGeneratedBase)))
            {
                UmbracoContentTypeAttribute parentAttribute = parentType.GetCustomAttribute<UmbracoContentTypeAttribute>();
                if (parentAttribute != null)
                {
                    string parentAlias = parentAttribute.ContentTypeAlias;
                    IContentType parentContentType = contentTypeService.GetContentType(parentAlias);
                    newContentType = new ContentType(parentContentType);
                }
                else
                {
                    throw new Exception("The given base class has no UmbracoContentTypeAttribute");
                }
            }
            else
            {
                newContentType = new ContentType(-1);
            }

            newContentType.Name = attribute.ContentTypeName;
            newContentType.Alias = attribute.ContentTypeAlias;
            newContentType.Icon = attribute.Icon;

            if (attribute.CreateMatchingView)
            {
                CreateMatchingView(fileService, attribute, type, newContentType);
            }

            newContentType.AllowedAsRoot = attribute.AllowedAtRoot;
            newContentType.IsContainer = attribute.EnableListView;
            newContentType.AllowedContentTypes = FetchAllowedContentTypes(attribute.AllowedChildren, contentTypeService);

            //create tabs
            CreateTabs(newContentType, type, dataTypeService);

            //create properties on the generic tab
            var propertiesOfRoot = type.GetProperties().Where(x => x.GetCustomAttribute<UmbracoPropertyAttribute>() != null);
            foreach (var item in propertiesOfRoot)
            {
                CreateProperty(newContentType, null, dataTypeService, true, item);
            }

            //Save and persist the content Type
            contentTypeService.Save(newContentType, 0);
        }

        /// <summary>
        /// Creates a View if specified in the attribute
        /// </summary>
        /// <param name="fileService"></param>
        /// <param name="attribute"></param>
        /// <param name="type"></param>
        /// <param name="newContentType"></param>
        private static void CreateMatchingView(IFileService fileService, UmbracoContentTypeAttribute attribute, Type type, IContentType newContentType)
        {
            var currentTemplate = fileService.GetTemplate(attribute.ContentTypeAlias) as Template;
            if (currentTemplate == null)
            {
                string templatePath;
                if (string.IsNullOrEmpty(attribute.TemplateLocation))
                {
                    templatePath = string.Format(CultureInfo.InvariantCulture, "~/Views/{0}.cshtml", attribute.ContentTypeAlias);
                }
                else
                {
                    templatePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}.cshtml",
                        attribute.TemplateLocation,                                     // The template location
                        attribute.TemplateLocation.EndsWith("/") ? string.Empty : "/",  // Ensure the template location ends with a "/"
                        attribute.ContentTypeAlias);                                    // The alias
                }

                currentTemplate = new Template(templatePath, attribute.ContentTypeName, attribute.ContentTypeAlias);
                CreateViewFile(attribute.MasterTemplate, currentTemplate, type, fileService);
            }

            newContentType.AllowedTemplates = new ITemplate[] { currentTemplate };
            newContentType.SetDefaultTemplate(currentTemplate);

            //TODO: in Umbraco 7.1 it will be possible to set the master template of the newly created template
            //https://github.com/umbraco/Umbraco-CMS/pull/294
        }

        /// <summary>
        /// Scans for properties on the model which have the UmbracoTab attribute
        /// </summary>
        /// <param name="newContentType"></param>
        /// <param name="model"></param>
        /// <param name="dataTypeService"></param>
        private static void CreateTabs(IContentType newContentType, Type model, IDataTypeService dataTypeService)
        {
            var properties = model.GetProperties().Where(x => x.DeclaringType == model && x.GetCustomAttribute<UmbracoTabAttribute>() != null).ToArray();
            int length = properties.Length;

            for (int i = 0; i < length; i++)
            {
                var tabAttribute = properties[i].GetCustomAttribute<UmbracoTabAttribute>();

                newContentType.AddPropertyGroup(tabAttribute.Name);

                CreateProperties(properties[i], newContentType, tabAttribute.Name, dataTypeService);
            }
        }

        /// <summary>
        /// Every property on the Tab object is scanned for the UmbracoProperty attribute
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="newContentType"></param>
        /// <param name="tabName"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="atTabGeneric"></param>
        private static void CreateProperties(PropertyInfo propertyInfo, IContentType newContentType, string tabName, IDataTypeService dataTypeService, bool atTabGeneric = false)
        {
            //type is from TabBase
            Type type = propertyInfo.PropertyType;
            var properties = type.GetProperties().Where(x => x.GetCustomAttribute<UmbracoPropertyAttribute>() != null);
            if (properties.Count() > 0)
            {
                foreach (var item in properties)
                {
                    CreateProperty(newContentType, tabName, dataTypeService, atTabGeneric, item);
                }
            }
        }

        /// <summary>
        /// Creates a new property on the ContentType under the correct tab
        /// </summary>
        /// <param name="newContentType"></param>
        /// <param name="tabName"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="atTabGeneric"></param>
        /// <param name="item"></param>
        private static void CreateProperty(IContentType newContentType, string tabName, IDataTypeService dataTypeService, bool atTabGeneric, PropertyInfo item)
        {
            UmbracoPropertyAttribute attribute = item.GetCustomAttribute<UmbracoPropertyAttribute>();

            IDataTypeDefinition dataTypeDef;
            if (string.IsNullOrEmpty(attribute.DataTypeInstanceName))
            {
                dataTypeDef = dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(attribute.DataType).FirstOrDefault();
            }
            else
            {
                dataTypeDef = dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(attribute.DataType).FirstOrDefault(x => x.Name == attribute.DataTypeInstanceName);
            }

            if (dataTypeDef != null)
            {
                PropertyType propertyType = new PropertyType(dataTypeDef);
                propertyType.Name = attribute.Name;
                propertyType.Alias = (atTabGeneric ? attribute.Alias : UmbracoCodeFirstExtensions.HyphenToUnderscore(UmbracoCodeFirstExtensions.ParseUrl(attribute.Alias + "_" + tabName, false)));
                propertyType.Description = attribute.Description;
                propertyType.Mandatory = attribute.Mandatory;
                propertyType.SortOrder = attribute.SortOrder;

                if (atTabGeneric)
                {
                    newContentType.AddPropertyType(propertyType);
                }
                else
                {
                    newContentType.AddPropertyType(propertyType, tabName);
                }
            }
        }

        #endregion Create

        #region Update

        /// <summary>
        /// Update the existing content Type based on the data in the attributes
        /// </summary>
        /// <param name="contentTypeService"></param>
        /// <param name="fileService"></param>
        /// <param name="attribute"></param>
        /// <param name="contentType"></param>
        /// <param name="type"></param>
        /// <param name="dataTypeService"></param>
        private static void UpdateContentType(IContentTypeService contentTypeService, IFileService fileService, UmbracoContentTypeAttribute attribute, IContentType contentType, Type type, IDataTypeService dataTypeService)
        {
            contentType.Name = attribute.ContentTypeName;
            contentType.Alias = attribute.ContentTypeAlias;
            contentType.Icon = attribute.Icon;
            contentType.IsContainer = attribute.EnableListView;
            contentType.AllowedContentTypes = FetchAllowedContentTypes(attribute.AllowedChildren, contentTypeService);
            contentType.AllowedAsRoot = attribute.AllowedAtRoot;

            Type parentType = type.BaseType;
            if (parentType != null && parentType != typeof(UmbracoGeneratedBase) && parentType.GetBaseTypes(false).Any(x => x == typeof(UmbracoGeneratedBase)))
            {
                UmbracoContentTypeAttribute parentAttribute = parentType.GetCustomAttribute<UmbracoContentTypeAttribute>();
                if (parentAttribute != null)
                {
                    string parentAlias = parentAttribute.ContentTypeAlias;
                    IContentType parentContentType = contentTypeService.GetContentType(parentAlias);
                    contentType.ParentId = parentContentType.Id;
                }
                else
                {
                    throw new Exception("The given base class has no UmbracoContentTypeAttribute");
                }
            }

            if (attribute.CreateMatchingView)
            {
                CreateMatchingView(fileService, attribute, type, contentType);

                //Template currentTemplate = fileService.GetTemplate(attribute.ContentTypeAlias) as Template;
                //if (currentTemplate == null)
                //{
                //    //there should be a template but there isn't so we create one
                //    currentTemplate = new Template("~/Views/" + attribute.ContentTypeAlias + ".cshtml", attribute.ContentTypeName, attribute.ContentTypeAlias);
                //    CreateViewFile(attribute.ContentTypeAlias, attribute.MasterTemplate, currentTemplate, type, fileService);
                //    fileService.SaveTemplate(currentTemplate, 0);
                //}
                //contentType.AllowedTemplates = new ITemplate[] { currentTemplate };
                //contentType.SetDefaultTemplate(currentTemplate);
            }

            VerifyProperties(contentType, type, dataTypeService);

            //verify if a tab has no properties, if so remove
            var propertyGroups = contentType.PropertyGroups.ToArray();
            int length = propertyGroups.Length;
            for (int i = 0; i < length; i++)
            {
                if (propertyGroups[i].PropertyTypes.Count == 0)
                {
                    //remove
                    contentType.RemovePropertyGroup(propertyGroups[i].Name);
                }
            }

            //persist
            contentTypeService.Save(contentType, 0);
        }

        /// <summary>
        /// Loop through all properties and remove existing ones if necessary
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="type"></param>
        /// <param name="dataTypeService"></param>
        private static void VerifyProperties(IContentType contentType, Type type, IDataTypeService dataTypeService)
        {
            var properties = type.GetProperties().Where(x => x.GetCustomAttribute<UmbracoTabAttribute>() != null).ToArray();
            List<string> propertiesThatShouldExist = new List<string>();

            foreach (var propertyTab in properties)
            {
                var tabAttribute = propertyTab.GetCustomAttribute<UmbracoTabAttribute>();
                if (!contentType.PropertyGroups.Any(x => x.Name == tabAttribute.Name))
                {
                    contentType.AddPropertyGroup(tabAttribute.Name);
                }

                propertiesThatShouldExist.AddRange(VerifyAllPropertiesOnTab(propertyTab, contentType, tabAttribute.Name, dataTypeService));
            }

            var propertiesOfRoot = type.GetProperties().Where(x => x.GetCustomAttribute<UmbracoPropertyAttribute>() != null);
            foreach (var item in propertiesOfRoot)
            {
                //TODO: check for correct name
                propertiesThatShouldExist.Add(VerifyExistingProperty(contentType, null, dataTypeService, item, true));
            }

            //loop through all the properties on the ContentType to see if they should be removed;
            var existingUmbracoProperties = contentType.PropertyTypes.ToArray();
            int length = contentType.PropertyTypes.Count();
            for (int i = 0; i < length; i++)
            {
                if (!propertiesThatShouldExist.Contains(existingUmbracoProperties[i].Alias))
                {
                    //remove the property
                    contentType.RemovePropertyType(existingUmbracoProperties[i].Alias);
                }
            }
        }

        /// <summary>
        /// Scan the properties on tabs
        /// </summary>
        /// <param name="propertyTab"></param>
        /// <param name="contentType"></param>
        /// <param name="tabName"></param>
        /// <param name="dataTypeService"></param>
        /// <returns></returns>
        private static IEnumerable<string> VerifyAllPropertiesOnTab(PropertyInfo propertyTab, IContentType contentType, string tabName, IDataTypeService dataTypeService)
        {
            Type type = propertyTab.PropertyType;
            var properties = type.GetProperties().Where(x => x.GetCustomAttribute<UmbracoPropertyAttribute>() != null);
            if (properties.Count() > 0)
            {
                List<string> propertyAliases = new List<string>();
                foreach (var item in properties)
                {
                    propertyAliases.Add(VerifyExistingProperty(contentType, tabName, dataTypeService, item));
                }
                return propertyAliases;
            }
            return new string[0];
        }

        private static string VerifyExistingProperty(IContentType contentType, string tabName, IDataTypeService dataTypeService, PropertyInfo item, bool atGenericTab = false)
        {
            UmbracoPropertyAttribute attribute = item.GetCustomAttribute<UmbracoPropertyAttribute>();
            IDataTypeDefinition dataTypeDef;
            if (string.IsNullOrEmpty(attribute.DataTypeInstanceName))
            {
                dataTypeDef = dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(attribute.DataType).FirstOrDefault();
            }
            else
            {
                dataTypeDef = dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(attribute.DataType).FirstOrDefault(x => x.Name == attribute.DataTypeInstanceName);
            }

            if (dataTypeDef != null)
            {
                PropertyType property;
                bool alreadyExisted = contentType.PropertyTypeExists(attribute.Alias);
                // TODO: Added attribute.Tab != null after Generic Properties add, is this bulletproof?
                if (alreadyExisted && attribute.Tab != null)
                {
                    property = contentType.PropertyTypes.FirstOrDefault(x => x.Alias == attribute.Alias);
                }
                else
                {
                    property = new PropertyType(dataTypeDef);
                }

                property.Name = attribute.Name;
                //TODO: correct name?
                property.Alias = (atGenericTab ? attribute.Alias : UmbracoCodeFirstExtensions.HyphenToUnderscore(UmbracoCodeFirstExtensions.ParseUrl(attribute.Alias + "_" + tabName, false)));
                property.Description = attribute.Description;
                property.Mandatory = attribute.Mandatory;

                if (!alreadyExisted)
                {
                    if (atGenericTab)
                    {
                        contentType.AddPropertyType(property);
                    }
                    else
                    {
                        contentType.AddPropertyType(property, tabName);
                    }
                }

                return property.Alias;
            }
            return null;
        }

        #endregion Update

        #region Shared logic

        /// <summary>
        /// Gets the allowed children
        /// </summary>
        /// <param name="types"></param>
        /// <param name="contentTypeService"></param>
        /// <returns></returns>
        private static IEnumerable<ContentTypeSort> FetchAllowedContentTypes(Type[] types, IContentTypeService contentTypeService)
        {
            if (types == null) return new ContentTypeSort[0];

            List<ContentTypeSort> contentTypeSorts = new List<ContentTypeSort>();

            List<string> aliases = GetAliasesFromTypes(types);

            var contentTypes = contentTypeService.GetAllContentTypes().Where(x => aliases.Contains(x.Alias)).ToArray();

            int length = contentTypes.Length;
            for (int i = 0; i < length; i++)
            {
                ContentTypeSort sort = new ContentTypeSort();
                sort.Alias = contentTypes[i].Alias;
                int id = contentTypes[i].Id;
                sort.Id = new Lazy<int>(() => { return id; });
                sort.SortOrder = i;
                contentTypeSorts.Add(sort);
            }
            return contentTypeSorts;
        }

        private static List<string> GetAliasesFromTypes(Type[] types)
        {
            List<string> aliases = new List<string>();

            foreach (Type type in types)
            {
                UmbracoContentTypeAttribute attribute = type.GetCustomAttribute<UmbracoContentTypeAttribute>();
                if (attribute != null)
                {
                    aliases.Add(attribute.ContentTypeAlias);
                }
            }

            return aliases;
        }

        private static void CreateViewFile(string masterTemplate, Template template, Type type, IFileService fileService)
        {
            string physicalViewFileLocation = HostingEnvironment.MapPath(template.Path);
            if (string.IsNullOrEmpty(physicalViewFileLocation))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Failed to {0} to a physical location", template.Path));
            }

            var templateContent = CreateDefaultTemplateContent(masterTemplate, type);
            template.Content = templateContent;

            using (var sw = System.IO.File.CreateText(physicalViewFileLocation))
            {
                sw.Write(templateContent);
            }

            //This code doesn't work because template.MasterTemplateId is defined internal
            //I'll do a pull request to change this
            //TemplateNode rootTemplate = fileService.GetTemplateNode(master);
            //template.MasterTemplateId = new Lazy<int>(() => { return rootTemplate.Template.Id; });
            fileService.SaveTemplate(template, 0);

            //    //TODO: in Umbraco 7.1 it will be possible to set the master template of the newly created template
            //    //https://github.com/umbraco/Umbraco-CMS/pull/294
        }

        private static string CreateDefaultTemplateContent(string master, Type type)
        {
            var sb = new StringBuilder();
            sb.AppendLine("@inherits Umbraco.Web.Mvc.UmbracoTemplatePage");
            sb.AppendLine("@*@using Qite.Umbraco.CodeFirst.Extensions;*@");
            sb.AppendLine("@{");
            sb.AppendLine("\tLayout = \"" + master + ".cshtml\";");
            sb.AppendLine("\t//" + type.Name + " model = Model.Content.ConvertToRealModel<" + type.Name + ">();");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion Shared logic
    }
}