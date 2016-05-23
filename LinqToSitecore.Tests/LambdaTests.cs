using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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



            var item = new MyTestClass { Prop2 = 123, Prop4 = new MyTestClass() { Prop1 = "dsa" } };
            var prop2 = 23;
            string prop = "abc";

            Expression<Func<MyTestClass, bool>> q5 = x => x.Prop1 == prop || x.Prop2 == item.Prop2 || x.Prop1 == item.Prop4.Prop1;

            var m = Regex.Replace(q5.Body.ToString(), @"(\.Contains\(.(?<g1>.+?).\))", " = '%$1%'", RegexOptions.ExplicitCapture);
            var m2 = Regex.Replace(m, @"Not\((?<g1>[^=]+?)\)", "($1 != 1)", RegexOptions.ExplicitCapture);
            var m3 = Regex.Replace(m2, @"Not\((?<g1>.+?)\)", "($1)", RegexOptions.ExplicitCapture);
            var m4 = Regex.Replace(m3, @"(?<q1>\.[a-zA-Z0-9]+)(:?\)|\s\w+|$)", "$1 = 1", RegexOptions.ExplicitCapture);

            var query = q5.Body.ToString();

            query = LinqToSitecoreExtensions.LambdaToSitecoreQuery(q5);
            Console.WriteLine(q5.Body.ToString());
            Console.WriteLine(query);


        }

    }



    public class MyTestClass
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
        public bool Prop3 { get; set; }
        public MyTestClass Prop4 { get; set; }
    }
}
