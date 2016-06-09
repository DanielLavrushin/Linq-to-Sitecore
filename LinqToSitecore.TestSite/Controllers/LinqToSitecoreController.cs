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
using Sitecore.Data.Query;

namespace LinqToSitecore.TestSite.Controllers
{
    public class LinqToSitecoreController: Controller
    {
        private Database _db;



        public ActionResult Index(string  param1, string param2, string param3)
        {
            _db = Sitecore.Context.Database;

            var myLinq = new MyLinqToObject();
            myLinq.SingleLine =  param1;

            var items = _db.Query<MyLinqToObject>(x => x.Checkbox == true || x.Checkbox);
            

            return Json(items, JsonRequestBehavior.AllowGet);
        }
    }

    public class MyLinqToObject
    {
        [SitecoreSystemProperty(SitecoreSystemPropertyType.Id)]
        public Guid Id { get; set; }

        public string SingleLine { get; set; }
        public bool Checkbox { get; set; }



    }
}