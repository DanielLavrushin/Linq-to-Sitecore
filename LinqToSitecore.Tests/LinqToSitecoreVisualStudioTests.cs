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
            var item = LinqToSitecoreFactory.GetItem(SitecoreGuids.Site8MyLinqToSitecore);



        }
    }
}
