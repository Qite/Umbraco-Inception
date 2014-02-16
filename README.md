Umbraco Inception
=================

A code first approach for Umbraco (7)

Created by [Qite]("http://qite.be" "Qite Intelligent IT")

##How to install

Install the Umbraco.Inception package from [nuget](http://www.nuget.org/packages/Umbraco.Inception/).

Or download the package from the [Umbraco Package Repository](http://our.umbraco.org/projects/developer-tools/umbraco-inception)

##Getting started

First you create your models as you would do in any Asp Mvc application.

```csharp
public class Person
{
    public string Name { get; set; }
}
```

Then you add the matching properties on your model and let it inherit from UmbracoGeneratedBase:

```csharp
[UmbracoContentType("Name","alias", ...)]
public class Person:UmbracoGeneratedBase
{
    [UmbracoProperty("Name","alias","Type",...)]
    public string Name { get; set; }
}
```

If you would like to group properties to a specific tab you create a tab class

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

Next step if to register your model.
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

