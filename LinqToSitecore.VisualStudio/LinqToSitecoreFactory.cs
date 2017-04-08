using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EnvDTE;
using LinqToSitecore.VisualStudio.Data;
using LinqToSitecore.VisualStudio.SitecoreWebService;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace LinqToSitecore.VisualStudio
{
    public static class LinqToSitecoreFactory
    {
        public static DTE DteService { get; set; }

        public static ICollection<Item> Items { get; set; }

        public static Project Project
        {
            get { return ((object[]) DteService.ActiveSolutionProjects)[0] as Project; }

        }


        private static VisualSitecoreServiceSoapClient _service;
        private static AppSettings _settings = AppSettings.Instance();

        public static string ProjectNamespace
        {
            get
            {
                var properties = Project.Properties.Cast<Property>().ToDictionary(x => x.Name);
                return properties["DefaultNamespace"].Value.ToString();

            }
        }

        public static VisualSitecoreServiceSoapClient Service
        {
            get
            {

                if (_service == null && !string.IsNullOrEmpty(_settings.SitecoreUrl))
                {
                    var binding = new BasicHttpBinding
                    {
                        TransferMode = TransferMode.Buffered,
                        MaxBufferPoolSize = 524288,
                        MaxBufferSize = 16777216,
                        MaxReceivedMessageSize = 16777216,
                        ReaderQuotas = {MaxStringContentLength = 16777216}
                    };

                    var endpoint = new EndpointAddress($"{_settings.SitecoreUrl}/sitecore/shell/WebService/Service.asmx");

                    _service = new VisualSitecoreServiceSoapClient(binding, endpoint);

          
                }
                return _service;
            }
        }


        public static void Refresh()
        {
            _service = null;
            
        }
        private static Credentials Credentials
        {
            get
            {
                var credentials = new Credentials
                {
                    UserName = _settings.SitecoreLogin,
                    Password = _settings.SitecorePassword
                };
                return credentials;
            }
        }

        public static bool IsValidConnection()
        {

            if (Service == null) return false;

            if (string.IsNullOrEmpty(_settings.SitecoreUrl)) return false;


            try
            {
                var test = Service.VerifyCredentials(Credentials);
                return test.FirstChild.InnerText != "failed";
            }
            catch (Exception ex)
            {
                return false;

            }
        }


        public static Item GetItem(Guid id)
        {
            var xml = Service.GetXML(id.ToString("B"), false, "master", Credentials);

            var item = xml.ToItem();
            return item;
        }
        public static Item GetItem(Guid id, bool loadAll)
        {
            var xml = Service.GetXML(id.ToString("B"), loadAll, "master", Credentials);

            var item = xml.ToItem();
            return item;
        }
        public static Item GetRoot()
        {
            return GetItem(SitecoreGuids.Root);
        }

        public static ICollection<Item> GetChildren(Guid id)
        {
            var xml = Service.GetChildren(id.ToString("B"), "master", Credentials);
            var items = xml.ChildNodes.ToItems();

            return items;


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
