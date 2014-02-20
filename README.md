Umbraco Inception
=================

A code first approach for Umbraco (7)

Created by [Qite](http://qite.be "Qite Intelligent IT")

##How to install

Install the Umbraco.Inception package from [nuget]("http://www.nuget.org/packages/Umbraco.Inception/").

Or download the package from the [Umbraco Package Repository](http://our.umbraco.org/projects/developer-tools/umbraco-inception)

##Getting started

First you create your models as you would do in any Asp Mvc application.

```csharp
public class Person
{
    public string Name { get; set; }
}
```

Then you add the matching properties on your model and let it inherit from UmbracoGeneratedBase (or an other class that inherits UmbracoGeneratedBase).

```csharp
[UmbracoContentType("Name","alias", ...)]
public class Person:UmbracoGeneratedBase
{
    [UmbracoProperty("Name","alias","Type",...)]
    public string Name { get; set; }
}
```

The UmbracoProperty attribute has a couple of parameters which may need some guidance.

```string dataType, string dataTypeInstanceName = null, Type converterType = null``` 

DataType is the umbraco dataType you want to use. All the built-in dataTypes of Umbraco are stored as constants in the static class BuiltInUmbracoDataTypes.
If you have create your own U7 DataType (the cool stuff with AngularJS) then you enter the alias and the dataTypeInstanceName is the name of your dataType instance you configured in the Developer section.

So if you just want to use TextString, you can say:

```csharp
[UmbracoProperty("My name", "myAlias",BuiltInUmbracoDataTypes.Textbox, null, null)]
```

Built in dataTypes don't need an dataTypeInstance name. Your own do if you have multiple instances of them.

And the last funky parameter is the converterType.
Why do we need that? Well Umbraco storeds certain types in a way we can't use them directly.
For example: MediaPicker, booleans, dates, ...
In the case of MediaPicker the id is stored. But normally you don't want the id of the image, you want it's url.

That's why you can define a converter class to make this process easy:

```csharp
[UmbracoProperty("Picture","pictureAlias",BuiltInUmbracoDataTypes.MediaPicker,null, typeof(Umbraco.Inception.Converters.MediaIdConverter)]
public string PictureUrl { get; set; }
```

The converters we provide (all living in Umbraco.Inception.Converters) are JsonConverter (generic), MediaIdConverter, ModelConverter (generic), MultipleMediaIdConverter, UBooleanConverter, UDateTimeConverter.

If you want to write your own then you need to create a class that inherits [TypeConverter]("http://msdn.microsoft.com/en-us/library/system.componentmodel.typeconverter(v=vs.110).aspx")
and override the following methods:

- public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
- public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
- public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
- public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)


If you would like to group properties to a specific tab you create a tab class.

```csharp
public class AddressTab:TabBase
{
    [UmbracoProperty("Street","street",...)]
    public string Street { get; set; }
    [UmbracoProperty("Zip","zip",...)]
    public string Zip { get; set; }
    [UmbracoProperty("City","city",...)]
    public string City { get; set; }
}
```

Create a property on the model of your TabBase c# class and decorate it with an UmbracoTab attribute

```csharp
[UmbracoContentType("Name","alias", ...)]
public class Person:UmbracoGeneratedBase
{
    [UmbracoProperty("Name","alias","Type",...)]
    public string Name { get; set; }
    
    [UmbracoTab("Work address")]
    public AddressTab Work { get; set; }
    
    [UmbracoTab("Home address")]
    public AddressTab Home {get;set;}
}
```

Note that this can be great for multi-lingual sites.

Next step is to register your model.
This can be done on Application Startup
f.ex:

```csharp
    public class RegisterEvents : ApplicationEventHandler
    {

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Once the content types are generated you don't need this to run every time
            //unless you did some changes to the models
            RegisterModels();
        }

        private void RegisterModels()
        {
            UmbracoCodeFirstInitializer.CreateOrUpdateEntity(typeof(Person));
        }

    }
```

```UmbracoCodeFirstInitializer.CreateOrUpdateEntity(typeof(Person));``` will create a matching Umbraco Content Type.
If it already exists then it will look for changes.

**Be aware that some changes you make to the attributes can cause major structural changes of the contentType and Umbraco might remove existing data.**

##Ok, so far so good but now what

The project contains an extension method (living in Umbraco.Inception.Extensions) that can convert a IPublishedContent back to your original model.

So you can have a typed instance of your Umbraco document in your view.

```razor
@inherits Umbraco.Web.Mvc.UmbracoTemplatePage
@using Umbraco.Inception.Extensions;
@{
    Layout = null;
    Person person = Model.Content.ConvertToModel<Person>();
}
<h1>Hi my name is @person.Name</h1>
```

##It goes one step further

If you read the section on converterType carefully you'll notice that we convert both ways.
This means that you can make a change to your model and save it back to Umbraco.
You can call it's inherited method Persist.

```csharp
using Umbraco.Inception.Extensions;

public void SomeMethodInAController(int contentId)
{
    IPublishedContent content = Umbraco.TypedContent(contentId);
    Person johnDoe = content.ConvertToModel<Person>();
    johnDoe.Name = "Johnny";
    johnDoe.Persist();
}
```

##Check out the demo project

We provided a demo project at [Github](https://github.com/Qite/InceptionDemo). 

##Cool, where can I help
- Well by testing it you might discover something we forgot or some situation we haven't faced yet.
Don't hesitate to create a [bug report](https://github.com/Qite/Umbraco-Inception/issues)

- Solve your own problem, reporting a bug is cool but solving it is just plain awesome!

##Contact
Any further questions may be directed here on github or at florian@qite.be

##Thanks to
We would like to thank everyone who has made any (small or big) contribution to this project:

- JimBobSquarePants