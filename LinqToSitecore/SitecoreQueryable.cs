using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;

namespace LinqToSitecore
{
    public class SitecoreQueryable<T>: IOrderedQueryable<T>
    {
        internal Database _database;
        public SitecoreQueryable(Database database)
        {
            _database = database;
            Provider = new SitecoreQueryProvider<T>(database);
            Expression = Expression.Constant(this);
        }

        public SitecoreQueryable(SitecoreQueryProvider<T> provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }

            Provider = provider;
            Expression = expression;
        }



        public IEnumerator<T> GetEnumerator()
        {
            return (Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (Provider.Execute<IEnumerable>(Expression)).GetEnumerator();
        }

        public Expression Expression { get; }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public IQueryProvider Provider { get; }
    }
}
