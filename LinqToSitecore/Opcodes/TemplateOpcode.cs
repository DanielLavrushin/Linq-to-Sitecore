using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Sitecore.Data.Query;

namespace LinqToSitecore.Opcodes
{
    public class TemplateOpcode: Opcode
    {
        public string TemplateName { get; set; }

        public override object Evaluate(Query query, QueryContext contextNode)
        {
            return base.Evaluate(query, contextNode);
        }

        public override void Print(HtmlTextWriter output)
        {
            base.Print(output);
        }
    }
}
