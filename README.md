# Linq to Sitecore
## About
This is a small library to help a developer to map sitecore items to the code-model.
## Nuget
<a href="https://www.nuget.org/packages/LinqToSitecore/">nuget project</a>
```
Install-Package LinqToSitecore
```
## Supports
###Supported LINQ Methods
n/r - meants not relevant for this specific object
yes - means supported

| LINQ Method  | Sitecore Database | Sitecore Item | Sitecore ItemList| Item[] collection | 
| ------------- | ------------- | ------------- | ------------- | ------------- |
| OfType\<T>(lambda expression) | yes  | n/r | no (use ToList\<T>()) | no (use ToList\<T>()) |
| ToList\<T>(lambda expression)  | n/r  | yes| yes | yes |
| Where\<T>(lambda expression)  | yes  | yes | yes | yes |
| FirstOrDefault\<T>(lambda expression)  | yes  | yes | yes | yes |
| Count\<T>(lambda expression)  | yes  | yes | yes | yes |
| Max\<T>(lambda expression)  | no  | yes | yes | yes |
| Min\<T>(lambda expression)  | no  | yes | yes | yes |

ReflectTo\<>() method extends Sitecore Item class, and allow you to convert any item to a trongly typed object.

IsOfType\<T>() method extends Sitecore item Class, and returns true if the template of the specific Item is of provided type (T).

##Property Types
The following Sitecore Field Types will be reflected to .NET Class Property Types:

| Sitecore Field  | Net Type | 
| ------------- | ------------- |
| Singline | string  |
| Checkbox | bool  |
| Multiline | string  |
| File | string (path)  |
| File | byte[] |
| Image | string (path)  |
| Image | byte[] |
| Number | decimal float double |
| Integer | int |
| Date | DateTime |
| Datetime | DateTime |
| RichText | string |
| Droplist | string |
| Droplist | Generic Type |
| Multilist | ICollection\<T> |
| Multilist w. search | ICollection\<T> |
| Treelist | ICollection\<T> |
| Checklist | ICollection\<T> |

For example:
```C#
using LinqToSitecore;
public class MyLinqToSitecore
    {
        public string SingleLine { get; set; }
        public bool Checkbox { get; set; }
        public string Multiline { get; set; }
        public string File { get; set; }
        public byte[] Image { get; set; }
        public float Number { get; set; }
        public int Integer { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateTime { get; set; }
        public string RichText { get; set; }
        public string Droplist { get; set; }
        public ICollection<MyAnotherCustomClass> CustomItems { get; set; }
        public ICollection<MyAnotherCustomClass> CustomItems2 { get; set; }
        public MyLinqToSitecore Droplink { get; set; }
    }
```


## Examples
### Code-First Approach
First of all you need to prepare your classes and reflect them in the sitecore items.

Lets create our base object class:

```C#
using LinqToSitecore;
 public class TemplateObject
    {
        [SitecoreSystemProperty(SitecoreSystemPropertyType.Id)]
        public ID Id { get; set; }

        [SitecoreSystemProperty(SitecoreSystemPropertyType.Name)]
        public string Name { get; set; }
    }
```
Base class could contain predefined Sitecore Properties - Id and Name which are base item properties.

Next, create your class:

```C#
using LinqToSitecore;

namespace MyTestClasses
{
    [SitecoreTemplate("{17CF7553-53AD-4260-B1F8-DCD0D9BE363C}")]
    public class MySitecoreItem: TemplateObject
    {
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string Combination { get; set; }
        public int SumNumber { get; set; }

        [SitecoreField("Custom Field")]
        public bool IsCustomField { get; set; }
    }
}
```
Use class attribute SitecoreTemplate to define Sitecore template which reflects your class. 
Also use SitecoreField property attribute to define a custom sitecore field name (in order if you want to have different names in your code and the sitecore template).

It is also possible to do not specify SitecoreTemplate attribute. In tis case LinqToSitecore will try to query and search the Sitecore database by the TemplateName which must be the same as your class name (in our case you have to create a template with the name MySitecoreItem).

Next, you could call linq-to-sitecore extended methods:
```C#
using LinqToSitecore;
 public class MySitecoreController: MyBaseController
    {
        [HttpGet]
        public ActionResult Output()
        {

            var allPossibleObjects = Sitecore.Context.Database.OfType<MySitecoreItem>();
            var allPossibleObjectsFromSpecificLocation = Sitecore.Context.Database.OfType<MySitecoreItem>("sitecore/content/myitems");
            
            var queriable = Sitecore.Context.Database.Where<MySitecoreItem>(s => s.IsCustomField == true || CategoryName == "BestCategory").ToList();

            var snigleItem  = Sitecore.Context.Database.FirstOrDefault<MySitecoreItem>();
            
            var oneItemToClass = Sitecore.Context.Database.GetItem("some_path_to_item").ReflectTo<MySitecoreItem>();
            
            var itemsToClass = Sitecore.Context.Database.GetItem("some_path_to_item").Children.ToList<MySitecoreItem>();
            
            var SelectItemsToClass = Sitecore.Context.Database.SelectItems("some_path_to_item").ToList<MySitecoreItem>();
            

            return Json(queriable);

```


Possible to use comparables:

```C#
using LinqToSitecore;
 public class MySitecoreController: MyBaseController
    {
        [HttpGet]
        public ActionResult Output()
        {

            var queriable = Sitecore.Context.Database.Where<MySitecoreItem>(s => s.Name.Contains("Black") && !s.Name.Contains("Red")).ToList();
        }

    }

```


###Lazy Loading Items
You could also reflect your Droplink fields into the classes.
Lets say you have a class:
```C#
    public class MyLinqToSitecore: TemplateObject
    {
        public MyLinqToSitecore Droplink { get; set; }
    }
```
Droplink is a standart Sitecorefield of type Drop-Link.

You could get the items like this:
```C#
    Sitecore.Context.Database.GetItem("pathtotheitem").ReflectTo<MyLinqToSitecore>();
```
In this case Droplink property will return NULL.
If you want to reflect all possible properties, do this:
```C#
    Sitecore.Context.Database.GetItem("pathtotheitem").ReflectTo<MyLinqToSitecore>(true);
```
