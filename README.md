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
public class NewModel
{
    public string Name { get; set; }
}
```

Then you add the matching properties on your model:

```csharp
[UmbracoContentType("Name","alias", ...)]
public class NewModel
{
    [UmbracoProperty("Name","alias","Type",...)]
    public string Name { get; set; }
}
```
