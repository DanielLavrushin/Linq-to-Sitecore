using System;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using LinqToSitecore.VisualStudio;
using LinqToSitecore.VisualStudio.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitecore;
using Sitecore.Data;

namespace LinqToSitecore.Tests
{
    [TestClass]
    public class LinqToSitecoreVisualStudioTests
    {
        [TestMethod]
        public void ReadConnectionStringsTest()
        {
            var file = @"C:\Sitecore\scdev\Website\App_Config\ConnectionStrings.config";

            var xml = XElement.Load(file);
            var c = xml.ToDynamicList();
            var connectionString = c.First(s => s.name == "master").connectionString;

            Sitecore.Context.Database.ConnectionStringName = connectionString;
            var db = new Context();
            var items = Context.Database.GetItem("/sitecore/content");

            Console.WriteLine(items.Name);
        }


        [TestMethod]
        public void GetItemsFromServiceTest()
        {
            var item = LinqToSitecoreFactory.GetItem(SitecoreGuids.Site);
        }
    }
}
