<img src= "http://s32.postimg.org/eiwhq873p/linqtositecore.png" height="75" style="float:right" />
# Linq to Sitecore
## About
This is a small lightweight library to help a developer to map sitecore items to the code-model using LINQ extensions.
## Nuget
<a href="https://www.nuget.org/packages/LinqToSitecore/">nuget project</a>
```
Install-Package LinqToSitecore
```

##Visual Studio LinqToSitecore Template Generator
This is a quick and lightweight Visual Studio Extension, which could help you to generate your classes based on the Sitecore templates. See more details on Visual Studio Gallery.
<a href="https://visualstudiogallery.msdn.microsoft.com/2513cf32-c7f3-4a45-8d12-c375dbee67c8">visualstudiogallery</a>
<img src="https://i1.visualstudiogallery.msdn.s-msft.com/2513cf32-c7f3-4a45-8d12-c375dbee67c8/image/file/229215/1/capture.png" />


## Short Example
Lets imagine you created a user defined custom item template called 'MyCustomTemplate'. It has 3 fields: checkbox, singleline and integer field.

## Manually create classes
Create your C# class based on existing Sitecore Template:
```C#

public class MyCustomTemplate{
  public string SingleLine { get; set; }
  public bool IsChecked { get; set; }
  public int MyInteger { get; set; }
  public DateTime Date { get; set; }
}
```
That is it! Now you are ready to get all items from the Sitecore database based on the class you just created.
Write your queries:
```C#
var myItem = Sitecore.Context.Database.ToList<MyCustomTemplate>();

var myitemsWtithQuery = Sitecore.Context.Database.Where<MyCustomTemplate>(x=>x.IsChecked);

var myItem = Sitecore.Context.Database.GetItem("/sitecore/content/myItem").ReflectTo<MyCustomTemplate>();

var myItem = Sitecore.Context.Database.SelectItems("/sitecore/content/myItem").ToList<MyCustomTemplate>();

var typedFieldValue = Sitecore.Context.Database.GetItem("/sitecore/content/myItem").GetFieldValue<DateTime>("Date");



```
Enjoy :)

## Supports
###Supported LINQ Methods
n/r - meants not relevant for this specific object
yes - means supported

| LINQ Method  | Sitecore Database | Sitecore Item | Sitecore ItemList| Item[] collection | 
| ------------- | ------------- | ------------- | ------------- | ------------- |
| ToList\<T>(lambda expression)  | yes  | yes| yes | yes |
| Where\<T>(lambda expression)  | yes  | yes | yes | yes |
| FirstOrDefault\<T>(lambda expression)  | yes  | yes | yes | yes |


ReflectTo\<>() method extends Sitecore Item class, and allow you to convert any item to a trongly typed object.

IsOfType\<T>() method extends Sitecore item Class, and returns true if the template of the specific Item is of provided type (T).

## Known Methods in Predicates
- operators:  >=, <=, <, >, ==, !=, !bool, bool
- method Contains(string)
- method StartsWith(string)
- method EndsWith(string)
- method Equals(object)

Examples:

```C#
  var mylinqitems = Sitecore.Data.Context.Database.OfType<MyLinqToSitecore>(x=>x.SingleLine.Contains("Text"));
  
  var mylinqitems2 = Sitecore.Data.Context.Database.OfType<MyLinqToSitecore>(x=>x.Date >= DateTime.Now.Date.AddDays(-10));
  
  var mylinqitems3 = Sitecore.Data.Context.Database.OfType<MyLinqToSitecore>(x=> !x.Checkbox);
```

##Property Types
The following Sitecore Field Types will be reflected to .NET Class Property Types:

| Sitecore Field  | Net Property Type | 
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
| Droplink | Generic Type |
| Multilist | ICollection\<T> |
| Multilist w. search | ICollection\<T> |
| Treelist | ICollection\<T> |
| Checklist | ICollection\<T> |
| General link | string (url path) |
| General link | Uri |
| Name Value List|NameValueCollection or Dictionary\<string, string>  |

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

### Build Parent/Child Hierarchy
You are also able to build Parent/Child Hierarchy. In fact with LinqToSitecore you could do this within 2 lines of code.

Let say you have this Sitecore Items structure:
<img src= "http://nordiccrm.com/content/sc1.png" />

And you want to get something like this:
<img src= "http://nordiccrm.com/content/sc2.png" />

Easy.
Here is how you could get it in 2 lines of code:
```C#
        public ActionResult Index()
        {
            _db = Sitecore.Context.Database;
            var items = _db.OfType<MyLinqToObject>("/sitecore/content/home").Where(x => x.Parent == null);
            return Json(items, JsonRequestBehavior.AllowGet);
        }
```

All you need is to mark your Parent class property by adding the SitecoreSystemProperty with the flag Parent.
Here is an example of the class:
```C#
    public class MyLinqToObject
    {
        //tell linqToSitecore to set Id property of the item
        [SitecoreSystemProperty(SitecoreSystemPropertyType.Id)]
        public Guid Id { get; set; }
       
        //tell linqToSitecore to set Name property of the item
        [SitecoreSystemProperty(SitecoreSystemPropertyType.Name)]
        public string Name { get; set; }

        //tell linqToSitecore to set Path property of the item
        [SitecoreSystemProperty(SitecoreSystemPropertyType.Path)]
        public string Path { get; set; }

        //tell linqToSitecore to set Item property of the item, be aware to do not output this to the code, add ScriptIgnore or JsonIgnore attribute
        [ScriptIgnore]
        [SitecoreSystemProperty(SitecoreSystemPropertyType.Item)]
        public Item Item { get; set; }

        [ScriptIgnore]
        //tell linqToSitecore to set Parent property of the item. It automatically converts ParentItem of the item into the generic   class
        [SitecoreSystemProperty(SitecoreSystemPropertyType.Parent)]
        public MyLinqToObject Parent { get; set; }

        //tell linqToSitecore to set ParentId property of the item
        [SitecoreSystemProperty(SitecoreSystemPropertyType.ParentId)]
        public Guid ParentId { get; set; }


        //This is how you could easelly get all children
        public ICollection<MyLinqToObject> Children
        {
            get { return Item.Children<MyLinqToObject>(); }
        }
    }
```

###Lazy Loading Items
By default the library reflects only basic fields in order to reduce connections to the database. However, you could force it to load linked items and reflect them into your classes.
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
