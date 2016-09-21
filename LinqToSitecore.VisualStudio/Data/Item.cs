using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LinqToSitecore.VisualStudio.Data
{
    public class Item
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid TemplateId { get; set; }
        public ICollection<Path> Path { get; set; }
        public ICollection<Field> Fields { get; set; }
        public ICollection<Item> Children { get; set; }
        public Guid ParentId { get; set; }
        public int SortOrder { get; set; }
        public Guid SectionId { get; set; }
        public XmlNode Node { get; set; }

        public Item()
        {
            Fields = new List<Field>();
            Path = new List<Path>();
            Children = new List<Item>();
        }

        public Item(XmlNode itemnode): base()
        {
            ParseItem(this, itemnode);
        }

        public static Item ParseItem(Item item, XmlNode itemnode)
        {
            if(item == null) item = new Item();

            item.Name = itemnode.GetAttributeValue<string>("name");
            item.Id = itemnode.GetAttributeValue<Guid>("id");
            item.TemplateId = itemnode.GetAttributeValue<Guid>("templateid");
            item.SortOrder = itemnode.GetAttributeValue<int>("sortorder");
            item.SectionId = itemnode.GetAttributeValue<Guid>("sectionid");
            item.Node = itemnode;

            var fieldsxml = itemnode.SelectNodes(@".//field");
            if (fieldsxml != null && fieldsxml.Count > 0)
            {
                item.Fields = fieldsxml.ToFields();
            }

            if (itemnode.ChildNodes.Count > 0)
            {
                item.Children = itemnode.ChildNodes.Cast<XmlNode>()
                    .Select(x => x.GetItemXml()).Where(x => x != null).Select(x=>x.ToItem()).ToList();
            }

            return item;
        }

      
    }

    public class Field
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public XmlNode Node { get; set; }

        public static Field Parse(XmlNode fieldnode)
        {
          return ParseField(new Field(), fieldnode);
        }

        public static Field ParseField(Field field, XmlNode fieldnode)
        {
            if (field == null) field = new Field();

            field.Id = fieldnode.GetAttributeValue<Guid>("id");
            field.TemplateId = fieldnode.GetAttributeValue<Guid>("tfid");
            field.Value = fieldnode.SelectSingleNode(@"value")?.InnerText;
            field.Name = fieldnode.GetAttributeValue<string>("name");
            field.Node = fieldnode;

           var content = fieldnode.SelectSingleNode(@"//field[@key='type']/content");
            if (content != null)
            {
                field.Type = content.InnerText;
            }
                return field;
        }
    }

    public class Path
    {
    }
}
