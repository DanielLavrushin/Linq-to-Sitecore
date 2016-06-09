using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.DataProviders.Sql.FastQuery;
using Sitecore.Data.Query;

namespace LinqToSitecore.Opcodes
{
    public class ContainsFunctionTranslator: IOpcodeTranslator
    {
        public string Translate(Opcode opcode, ITranslationContext context)
        {
            var str = string.Empty;
            if (opcode is Function)
            {
                var func = opcode as Function;
                var name = func.Name;
                var 
                switch (func.Name)
                {
                    case "contains":

                    default:
                        return String.Empty;
                        
                }

            }

            return string.Empty;
        }


    }
}
