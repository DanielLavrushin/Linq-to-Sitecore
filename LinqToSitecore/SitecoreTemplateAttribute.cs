using System;
using Sitecore.Data;

namespace LinqToSitecore
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SitecoreTemplateAttribute: Attribute
    {
        public ID TemplateId { get; set; }

        public SitecoreTemplateAttribute(ID templateId)
        {
            TemplateId = templateId;

        }

        public SitecoreTemplateAttribute(string templateId)
        {
            TemplateId = ID.Parse(templateId);

        }

        public SitecoreTemplateAttribute(Guid templateId)
        {
            TemplateId = ID.Parse(templateId);

        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SitecoreFieldAttribute: Attribute
    {
        public string Name { get; set; }
        public SitecoreFieldAttribute(string fieldName)
        {
            Name = fieldName;

        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SitecoreSystemPropertyAttribute: Attribute
    {
        public SitecoreSystemPropertyType FieldType { get; private set; }

        public SitecoreSystemPropertyAttribute(SitecoreSystemPropertyType type)
        {
            FieldType = type;
        }
    }

    public enum SitecoreSystemPropertyType
    {
        Id, Name
    }

}