using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Data.Query;
using IQueryable = System.Linq.IQueryable;

namespace LinqToSitecore
{
    public class SitecoreQueryProvider<T>: IQueryProvider
    {
        private bool IsEnumerable;
        internal Database _database;

        public SitecoreQueryProvider(Database database)
        {
            _database = database;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(SitecoreQueryable<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var provider = this as SitecoreQueryProvider<TElement>;
            return new SitecoreQueryable<TElement>(provider, expression);
        }

        public object Execute(Expression expression)
        {
            var result = Query(expression);

            return result;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            IsEnumerable = (typeof(TResult).Name == "IEnumerable`1");
            return (TResult)Execute(expression);


        }

        internal object Query(Expression expression)
        {

            var opcode = LinqToSitecoreVisitor.GetCode(BooleanFixVisitor.Process<T>(expression), null, typeof(T));
            var context = new QueryContext(_database.DataManager);

            var qTranslator = LinqToSitecoreExtensions.GetQueryTranslator(context, _database);

            var contextResult = qTranslator.QueryFast(_database, opcode);

            var data = contextResult;

            if (data is QueryContext[])
            {
                return ((QueryContext[])data).Select(x => x.GetQueryContextItem().ReflectTo<T>(true)).ToList();

            }

            return new List<T> { ((QueryContext)data).GetQueryContextItem().ReflectTo<T>(true) };

        }
    }
}
