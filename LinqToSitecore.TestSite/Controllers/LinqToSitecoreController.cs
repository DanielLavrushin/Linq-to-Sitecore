using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using Sitecore.Data;
using LinqToSitecore;
using Sitecore.Data.Items;

namespace LinqToSitecore.TestSite.Controllers
{
    public class LinqToSitecoreController: Controller
    {
        private Database _db;



        public ActionResult Index()
        {
            _db = Sitecore.Context.Database;
            var items = _db.OfType<MyLinqToObject>("/sitecore/content/home").Where(x => x.Parent == null);
            return Json(items, JsonRequestBehavior.AllowGet);
        }
    }

    public class MyLinqToObject
    {
        [SitecoreSystemProperty(SitecoreSystemPropertyType.Id)]
        public Guid Id { get; set; }

        [SitecoreSystemProperty(SitecoreSystemPropertyType.Name)]
        public string Name { get; set; }

        [SitecoreSystemProperty(SitecoreSystemPropertyType.Path)]
        public string Path { get; set; }

        [ScriptIgnore]
        [SitecoreSystemProperty(SitecoreSystemPropertyType.Item)]
        public Item Item { get; set; }

        [ScriptIgnore]
        [SitecoreSystemProperty(SitecoreSystemPropertyType.Parent)]
        public MyLinqToObject Parent { get; set; }

        [SitecoreSystemProperty(SitecoreSystemPropertyType.ParentId)]
        public Guid ParentId { get; set; }

        public ICollection<MyLinqToObject> Children
        {
            get { return Item.Children<MyLinqToObject>(); }
        }



    }
}