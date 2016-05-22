#1 Linq to Sitecore
#2 About
This is a small library to help a developer to map sitecore items to the code-model.

#2 Examples
#3 Code-First Approach
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
Base class must define Properties Id and Name which are sitecore base item properties.

Next, create your class:

'''C#

'''


