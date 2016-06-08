using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitecore.Data.Query;
using Sitecore.FakeDb;

namespace LinqToSitecore.Tests
{
    [TestClass]
    public class LambdaTests
    {

        [TestMethod]
        public void QueryBuilderTest()
        {


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
