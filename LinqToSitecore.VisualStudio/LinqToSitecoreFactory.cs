using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LinqToSitecore.VisualStudio.Data;
using LinqToSitecore.VisualStudio.SitecoreWebService;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace LinqToSitecore.VisualStudio
{
    public static class LinqToSitecoreFactory
    {
        private static SitecoreWebService2SoapClient _service;
        public static SitecoreWebService2SoapClient Service
        {
            get { return _service ?? (_service = new SitecoreWebService2SoapClient()); }
        }

        private static Credentials Credentials
        {
            get
            {
                var credentials = new Credentials
                {
                    UserName = @"sitecore\admin",
                    Password = "b"
                };
                return credentials;
            }
        }

        public static Item GetItem(Guid id)
        {
            var xml = Service.GetXML(id.ToString("B"), false, "master", Credentials);

            var item = xml.ToItem();
            item.Fields = GetFields(id);
            return item;
        }
        public static ICollection<Field> GetFields(Guid id)
        {
            var xml = Service.GetXML(id.ToString("B"), true, "master", Credentials);
            var fields = new List<Field>();
            foreach (XmlNode x in xml.SelectNodes(@"//*[@template='template field']").Cast<XmlNode>().ToList())
            {
                    fields.Add(Field.Parse(x));
              
            }

            return fields;
        }
      
    }
}
