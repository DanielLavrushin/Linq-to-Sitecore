// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.DataProviders.Sql;
using Sitecore.Data.DataProviders.Sql.FastQuery;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;

namespace LinqToSitecore
{
    public class LinqToSitecoreQueryTranslator: QueryToSqlTranslator
    {
        public LinqToSitecoreQueryTranslator([NotNull] SqlDataApi api) : base(api)
        {
            Assert.ArgumentNotNull(api, nameof(api));
        }

        public object QueryFast([NotNull] Database database, [NotNull] Opcode opcode)
        {
            Assert.ArgumentNotNull(database, nameof(database));
            Assert.ArgumentNotNull(opcode, nameof(opcode));

            var parameters = new ParametersList();

            var sql = TranslateQuery(opcode, database, parameters);
            if (sql == null)
            {
                return null;
            }


            using (var reader = _api.CreateReader(sql, parameters.ToArray()))
            {
                var idList = new IDList();

                while (reader.Read())
                {
                    idList.Add(_api.GetId(0, reader));
                }

                if (idList.Count == 1)
                {
                    return new QueryContext(database.DataManager, idList[0]);
                }

                return idList.Cast<ID>().Select(id => new QueryContext(database.DataManager, id)).ToArray();
            }
        }

        public virtual string TranslateQuery(Opcode opcode, Database database, ParametersList parameters)
        {
            var step = opcode as Step;
            if (!(step is Root))
            {
                return null;
            }

            step = step.NextStep;
            var sql = string.Empty;

            while (step != null)
            {
                if (!(step is Children) && !(step is Descendant) && !(step is Ancestor) && !(step is Parent))
                {
                    throw new Exception("Can navigate only child, parent, descendant, and ancestor axes.");
                }

                ITranslationContext context = new BasicTranslationContext(_factory, _api, database, parameters);
                var predicate = GetPredicate(step);
                var name = GetName(step);

                if (name.StartsWith("_."))
                {
                    name = name.Substring(2);
                }

                Opcode expression = null;
                if (predicate != null)
                {
                    expression = predicate.Expression;
                }

                var where = string.Empty;
                if (expression != null)
                {
                    where = context.Factory.GetTranslator(expression).Translate(expression, context);
                }

                var builder = new StringBuilder();

                AddInitialStatement(builder);


                AddFieldJoins(context, builder);

                if (!string.IsNullOrEmpty(sql))
                {
                    AddNestedQuery(step, sql, builder);
                }

                AddExtraJoins(context, builder);

                where = where.Trim();
                var whereAppended = false;
                if (where.Length > 0)
                {
                    whereAppended = AddConditionJoint(false, builder);
                    builder.Append(where);
                }

                if (name.Length > 0 && name != "*")
                {
                    whereAppended = AddConditionJoint(whereAppended, builder);
                    AddNameFilter(name, builder);
                }

                if (step is Children && string.IsNullOrEmpty(sql))
                {
                    AddConditionJoint(whereAppended, builder);
                    AddRootItemFilter(parameters, builder);
                }

                sql = builder.ToString();
                step = step.NextStep;
            }

            return sql;
        }
    }
}
