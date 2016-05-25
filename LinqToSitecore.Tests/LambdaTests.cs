using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitecore.FakeDb;

namespace LinqToSitecore.Tests
{
    [TestClass]
    public class LambdaTests
    {
        [TestMethod]
        public void TestMethod1()
        {

            var list = new List<MyTestClass>();

            list.Add(new MyTestClass());
            list.Add(new MyTestClass());
            list.Add(new MyTestClass());



            var item = new MyTestClass
            {
                Prop2 = 123,
                Prop4 = new MyTestClass { Prop1 = "dsa" },
                Prop5 = new List<MyTestClass> { new MyTestClass(), new MyTestClass(), new MyTestClass() }

            };
            var prop2 = 23;
            string prop = "abc";

            Expression<Func<MyTestClass, bool>> q5 = x => x.Prop6 > DateTime.Now && x.Prop3 == item.Prop3;




            var expBody = ExpressionEvaluator.EvalToString(q5);


            var query = SitecoreQueryWorker.ToSitecoreQuery(q5);

            Console.WriteLine(q5.Body.ToString());
            Console.WriteLine(expBody);
            Console.WriteLine(query);


        }

        private Db PrepareFakeDb()
        {
            var templateId = Sitecore.Data.ID.NewID;
            var db = new Db("master");
            db.Add(new DbTemplate("Operation", templateId) { });


            return db;
        }

    }



    public class MyTestClass
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
        public bool Prop3 { get; set; }
        public MyTestClass Prop4 { get; set; }
        public ICollection<MyTestClass> Prop5 { get; set; }
        public DateTime Prop6 { get; set; }
    }
}
