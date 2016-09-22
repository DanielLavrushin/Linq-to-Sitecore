using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        public ObservableCollection<Item> Children { get; set; }
        public Guid ParentId { get; set; }
        public int SortOrder { get; set; }
        public Guid SectionId { get; set; }
        public XmlNode Node { get; set; }
        public string TemplateKey { get; set; }
        public bool IsExpanded { get; set; }
        public bool HasChildren { get; set; }
        public Uri IconUrl { get; set; }
        public string ClassName { get; set; }

      

        public string DisplayName
        {
            get { return string.IsNullOrEmpty(Name) ? Id.ToString() : Name; }
        }

        public string Namespace { get;  set; }
        public bool IsSystemIncluded { get; set; }

        public Item()
        {
            Fields = new List<Field>();
            Path = new List<Path>();
            Children = new ObservableCollection<Item>();
        }
       

        public Item(XmlNode itemnode) : base()
        {
            ParseItem(this, itemnode);
        }

        public static Item ParseItem(Item item, XmlNode itemnode)
        {
            if (item == null) item = new Item();

            item.Name = itemnode.GetAttributeValue<string>("name");
            if (string.IsNullOrEmpty(item.Name))
            {
                item.Name = itemnode.InnerText;
            }

            item.Id = itemnode.GetAttributeValue<Guid>("id");
            item.TemplateId = itemnode.GetAttributeValue<Guid>("templateid");
            item.SortOrder = itemnode.GetAttributeValue<int>("sortorder");
            item.SectionId = itemnode.GetAttributeValue<Guid>("sectionid");
            item.TemplateKey = itemnode.GetAttributeValue<string>("template");
            item.HasChildren = itemnode.GetAttributeValue<bool>("haschildren");
            item.Node = itemnode;
            item.ClassName = item.Name;

            var itemIcon = itemnode.GetAttributeValue<string>("icon");

            if (!string.IsNullOrEmpty(itemIcon))
            {
                item.IconUrl = new Uri($"{AppSettings.Instance().SitecoreUrl}{itemIcon}",UriKind.Absolute);
            }
            else
            {
                var content = itemnode.SelectSingleNode(@".//field[@key='__icon']/content");
                if (content != null)
                {
                    item.IconUrl = new Uri($"{AppSettings.Instance().SitecoreUrl}/temp/IconCache/{content.InnerText}",UriKind.Absolute);
                }
            }
            if (item.TemplateKey == "template")
            {
                item.Fields = LinqToSitecoreFactory.GetFields(item.Id);
            }

            if (itemnode.ChildNodes.Count > 0)
            {
                item.Children = new ObservableCollection<Item>(itemnode.ChildNodes.Cast<XmlNode>()
                    .Select(x => x.GetItemXml()).Where(x => x != null).Select(x => x.ToItem()).ToList());
            }

            return item;
        }

    }

    public class NetType
    {
        public string NetTypeName { get; set; }
        public string SitecoreTypeName { get; set; }
        public Type Type { get; set; }
        public bool IsSelected { get; set; }
        public static NetType GetTypeInstance(string sitecoreName)
        {
            var net = new NetType
            {
                SitecoreTypeName = sitecoreName,
                Type = FromSitecoreFieldToType(sitecoreName)
            };
            net.NetTypeName = net.Type.Name;
            return net;
        }
       

        public static Type FromSitecoreFieldToType(string sitecoreType)
        {
            switch (sitecoreType)
            {
                case "Single-Line Text":
                    return typeof(string);
                case "Multi-Line Text":
                    return typeof(string);
                case "Checkbox":
                    return typeof(bool);
                case "File":
                    return typeof(string);
                case "Image":
                    return typeof(string);
                case "Date":
                    return typeof(DateTime);
                case "Datetime":
                    return typeof(DateTime);
                case "Number":
                    return typeof(int);
                case "Multilist":
                    return typeof(ICollection<Guid>);
                case "Treelist":
                    return typeof(ICollection<Guid>);
                case "Checklist":
                    return typeof(ICollection<Guid>);
                case "Multilist w. search":
                    return typeof(ICollection<Guid>);
                case "General link":
                    return typeof(Uri);
                case "Droplist":
                    return typeof(string);
                case "Name Value List":
                    return typeof(Dictionary<string, string>);
                default:
                    return typeof(string);
            }
        }
    }

    public class Field
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string NetType { get; set; }
        public string PropertyName { get; set; }
        public XmlNode Node { get; set; }

        public bool IsChecked { get; set; }

        public ObservableCollection<Type> NetTypes { get; set; }

        public static ObservableCollection<Type> GetNetTypes(string sitecoreType)
        {

            var sitecoreTypes = new Dictionary<string, ICollection<Type>>();
            sitecoreTypes.Add("Single-Line Text", new List<Type> {typeof(string)});
            sitecoreTypes.Add("Multi-Line Text", new List<Type> {typeof(string)});
            sitecoreTypes.Add("RichText", new List<Type> {typeof(string)});
            sitecoreTypes.Add("Checkbox", new List<Type> {typeof(bool), typeof(string), typeof(int)});
            sitecoreTypes.Add("File", new List<Type> {typeof(string), typeof(byte[])});
            sitecoreTypes.Add("Image", new List<Type> {typeof(string)});
            sitecoreTypes.Add("Date", new List<Type> {typeof(DateTime), typeof(string)});
            sitecoreTypes.Add("Datetime", new List<Type> {typeof(DateTime), typeof(string)});
            sitecoreTypes.Add("Number", new List<Type> {typeof(decimal), typeof(float), typeof(double), typeof(string)});
            sitecoreTypes.Add("Integer", new List<Type> {typeof(int), typeof(string)});
            sitecoreTypes.Add("Multilist", new List<Type> {typeof(string)});
            sitecoreTypes.Add("Treelist", new List<Type> {typeof(string)});
            sitecoreTypes.Add("Multilist w. search", new List<Type> {typeof(string)});
            sitecoreTypes.Add("Droplist", new List<Type> {typeof(string)});
            sitecoreTypes.Add("Name Value List",
                new List<Type> {typeof(NameValueCollection), typeof(Dictionary<string, string>), typeof(string)});
            sitecoreTypes.Add("General link", new List<Type> {typeof(Uri), typeof(string)});



            if (sitecoreTypes.ContainsKey(sitecoreType))
            {

                return new ObservableCollection<Type>(sitecoreTypes.FirstOrDefault(x => x.Key == sitecoreType).Value);
            }
            return new ObservableCollection<Type>(new List<Type> {typeof(string)});
        }

        public static Field Parse(XmlNode fieldnode)
        {
          return ParseField(new Field(), fieldnode);
        }

        public static Field ParseField(Field field, XmlNode fieldnode)
        {
            if (field == null) field = new Field();

            field.Id = fieldnode.GetAttributeValue<Guid>("id");
            field.TemplateId = fieldnode.GetAttributeValue<Guid>("tid");
            field.Value = fieldnode.SelectSingleNode(@"value")?.InnerText;
            field.Name = fieldnode.GetAttributeValue<string>("name");
            field.Node = fieldnode;
            field.IsChecked = true;
            field.PropertyName = field.Name;

            var content = fieldnode.SelectSingleNode(@".//field[@key='type']/content");
            if (content != null)
            {
                field.Type = content.InnerText;
                field.NetType = GetNetTypes(field.Type).FirstOrDefault().Name;
                field.NetTypes = GetNetTypes(field.Type);
            }
            return field;
        }
    }

    public class Path
    {
    }
}
