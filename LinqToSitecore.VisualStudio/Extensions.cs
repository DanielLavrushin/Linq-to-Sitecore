using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using LinqToSitecore.VisualStudio.Data;

namespace LinqToSitecore.VisualStudio
{
    public static class Extensions
    {
        public static Item ToItem(this XmlNode node)
        {
            var itemxml = node.GetItemXml();
            if (itemxml != null)
            {
                return new Item(itemxml);
            }

            return null;
        }

        public static ICollection<Item> ToItems(this XmlNodeList nodes)
        {
            return nodes.Cast<XmlNode>().Where(x=> x != null).Select(x => x.ToItem()).ToList();
        }
        public static ICollection<Field> ToFields(this XmlNodeList nodes)
        {
            return nodes.Cast<XmlNode>().ToFields();
        }
        public static ICollection<Field> ToFields(this IEnumerable<XmlNode> nodes)
        {
            return nodes.Where(x => x != null).Select(Field.Parse).ToList();
        }
        public static XmlNode GetItemXml(this XmlNode node)
        {

            if (node.Name == "item") return node;

            foreach (XmlNode child in node.ChildNodes)
            {
                var potentialItemXml = child.GetItemXml();

                if (potentialItemXml != null && potentialItemXml.Name == "item")
                {
                    return potentialItemXml;
                }
            }
            return null;
        }

        public static T GetAttributeValue<T>(this XmlNode node, string name)
        {

            if (node == null)
            {
                return default(T);
            }

            var attributes = node.Attributes;
            if (attributes == null)
            {
                return default(T);
            }

            var attribute = attributes[name];
            if (attribute == null)
            {
                return default(T);
            }
            return (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(attribute.Value);
        }

        public static List<dynamic> ToDynamicList(this XElement elements, List<dynamic> data = null)
        {
            if (data == null)
            {
                data = new List<dynamic>();
            }

            foreach (XElement element in elements.Elements())
            {
                dynamic person = new ExpandoObject();

                if (element.HasAttributes)
                {
                    foreach (var attribute in element.Attributes())
                    {
                        ((IDictionary<string, object>) person).Add(attribute.Name.LocalName, attribute.Value);
                    }
                }

                if (element.HasElements)
                {
                    foreach (XElement subElement in element.Elements())
                    {
                        if (subElement.HasElements)
                        {
                            ((IDictionary<string, object>) person).Add(subElement.Name.LocalName,
                                subElement.ToDynamicList());
                        }
                        else
                        {
                            ((IDictionary<string, object>) person).Add(subElement.Name.LocalName, subElement.Value);
                        }
                    }
                }

                data.Add(person);
            }

            return data;
        }
    }
}
