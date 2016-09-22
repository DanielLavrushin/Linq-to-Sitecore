using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LinqToSitecore.VisualStudio.Data;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace LinqToSitecore.VisualStudio
{
    public class LinqToSitecoreFileGenerator
    {

        private Item _item;
        private CodeDomProvider _provider;

        public LinqToSitecoreFileGenerator(Item item, CodeDomProvider provider) : base()
        {
            _item = item;
            _provider = provider;
        }

        private CodeTypeDeclaration CreateClass()
        {

            var typeDeclaration = new CodeTypeDeclaration(_item.Name);
            typeDeclaration.TypeAttributes = TypeAttributes.Public | TypeAttributes.Class;




            foreach (var field in _item.Fields.Where(x=> x.IsChecked))
            {
                typeDeclaration.Members.Add(CreateProperty(field));
            }

            if (_item.ClassName != _item.Name)
            {
                var attrParam = new CodeAttributeArgument(new CodePrimitiveExpression(_item.Id.ToString("B")));
                var attr = new CodeAttributeDeclaration("SitecoreTemplate", attrParam);
                typeDeclaration.CustomAttributes.Add(attr);
            }

            return typeDeclaration;
        }

        private CodeMemberField CreateProperty(Field field)
        {
            var mField = new CodeMemberField(field.NetType.Type, field.Name.Replace(" ", string.Empty));
            mField.Attributes = MemberAttributes.Public;

            if (field.Name != mField.Name)
            {
                mField.CustomAttributes.Add(CreateLinqToSitecoreAttribute(field.Name));
            }

            mField.Name += " { get; set; }";
            return mField;

        }

        private CodeAttributeDeclaration CreateLinqToSitecoreAttribute(string fieldName)
        {
            var attrParam = new CodeAttributeArgument(new CodePrimitiveExpression(fieldName));
            var attr = new CodeAttributeDeclaration("SitecoreField", attrParam);
            return attr;
        }

        private CodeCompileUnit CreateCodeCompileUnit()
        {
            var code = new CodeCompileUnit();
        
            var codeNamespace = new CodeNamespace();
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("LinqToSitecore"));

            codeNamespace.Types.Add(CreateClass());

            code.Namespaces.Add(codeNamespace);

            return code;
        }

        public string GenerateCode()
        {
            using (StringWriter writer = new StringWriter(new StringBuilder()))
            {
                var options = new CodeGeneratorOptions();
                options.BlankLinesBetweenMembers = true;
                options.BracingStyle = "Block";


                _provider.GenerateCodeFromCompileUnit(CreateCodeCompileUnit(), writer, options);

                writer.Flush();
                var enc = Encoding.GetEncoding(writer.Encoding.WindowsCodePage);
                var preamble = enc.GetPreamble();

                byte[] body = enc.GetBytes(writer.ToString());
                int preambleLength = preamble.Length;

                Array.Resize(ref preamble, preambleLength + body.Length);
                Array.Copy(body, 0, preamble, preambleLength, body.Length);

                //TODO: find a better way to print gets and sets
                return writer.ToString().Replace("};", "}"); ;
            }

        }
    }
}