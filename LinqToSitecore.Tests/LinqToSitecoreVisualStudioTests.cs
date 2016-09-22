using System;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EnvDTE;
using LinqToSitecore.VisualStudio;
using LinqToSitecore.VisualStudio.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToSitecore.Tests
{
    [TestClass]
    public class LinqToSitecoreVisualStudioTests
    {

        [TestMethod]
        public void GetItemsFromServiceTest()
        {
            var id = new Guid("{6B229418-DB30-4875-A721-6D0AAD0D8DE2}");
            var item = LinqToSitecoreFactory.GetChildren(id);



        }


        [TestMethod]
        public void TestConnection()
        {
            var item = LinqToSitecoreFactory.IsValidConnection();



        }
    }
}
