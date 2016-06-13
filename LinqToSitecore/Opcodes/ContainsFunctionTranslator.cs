using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.DataProviders.Sql.FastQuery;
using Sitecore.Data.Query;

namespace LinqToSitecore.Opcodes
{
    public class LinqToSitecoreFunction: Opcode
    {
        public string Type { get; set; }
        public string FieldName { get; set; }
        public object[] Parameters { get; set; }
        public LinqToSitecoreFunction(string type, params object[] parameters)
        {
            Type = type;
            FieldName = parameters[0].ToString();
            Parameters = parameters.Skip(1).ToArray();
        }
        public override object Evaluate(Query query, QueryContext contextNode)
        {
            return base.Evaluate(query, contextNode);
        }
    }

    public class LinqToSitecoreFunctionTranslator: IOpcodeTranslator
    {
        public string Translate(Opcode opcode, ITranslationContext context)
        {
            var str = string.Empty;
            if (opcode is LinqToSitecoreFunction)
            {
                var func = opcode as LinqToSitecoreFunction;
                switch (func.Type)
                {
                    case "contains":
                        string param1 = context.Parameters.AddParameter($"%{func.Parameters[0]}%");
                        str = context.SqlApi.Format("{2}" + param1 + "{3}");
                        break;
                    case "startswith":
                        string param2 = context.Parameters.AddParameter($"{func.Parameters[0]}%");
                        str = context.SqlApi.Format("{2}" + param2 + "{3}");
                        break;
                    case "endswith":
                        string param3 = context.Parameters.AddParameter($"%{func.Parameters[0]}");
                        str = context.SqlApi.Format("{2}" + param3 + "{3}");
                        break;
                    case "equals":
                        string param4 = context.Parameters.AddParameter($"{func.Parameters[0]}");
                        str = context.SqlApi.Format("{2}" + param4 + "{3}");
                        break;
                    default:
                        throw new NotImplementedException($"Function type '{func.Type}' is not supported.");

                }



            }

            return str;
        }


    }
}
