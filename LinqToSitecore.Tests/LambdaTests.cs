using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

            Expression<Func<MyTestClass, bool>> q1 = x => x.Prop3 == true;
            Expression<Func<MyTestClass, bool>> q2 = x => x.Prop3;
            Expression<Func<MyTestClass, bool>> q3 = x => x.Prop3 != true;
            Expression<Func<MyTestClass, bool>> q4 = x => !x.Prop3;


            Expression<Func<MyTestClass, bool>> q5 = x => x.Prop3|| !x.Prop3 && (x.Prop1.Contains("asd") || !x.Prop1.Contains( "vce"));

            var m = Regex.Replace(q5.Body.ToString(), @"(\.Contains\(.(?<g1>.+?).\))", " = '%$1%'", RegexOptions.ExplicitCapture);

            var m2 = Regex.Replace(m, @"Not\((?<g1>[^=]+?)\)", "($1 != 1)", RegexOptions.ExplicitCapture);

            var m3 = Regex.Replace(m2, @"Not\((?<g1>.+?)\)", "($1)", RegexOptions.ExplicitCapture);

            var m4 = Regex.Replace(m3, @"(?<q1>\.[a-zA-Z0-9]+)(:?\)|\s\w+|$)", "$1 = 1", RegexOptions.ExplicitCapture);

            

            Console.WriteLine(q5.Body.ToString());
            Console.WriteLine(m);
            Console.WriteLine(m2);
            Console.WriteLine(m3);
            Console.WriteLine(m4);
        }
    }

    public class MyTestClass
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
        public bool Prop3 { get; set; }
    }
}
