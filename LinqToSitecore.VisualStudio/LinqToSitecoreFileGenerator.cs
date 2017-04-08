using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EnvDTE;
using LinqToSitecore.VisualStudio.Data;
using CodeNamespace = System.CodeDom.CodeNamespace;
using Constants = EnvDTE.Constants;

namespace LinqToSitecore.VisualStudio
{
    public class LinqToSitecoreFileGenerator
    {

        private readonly Item _item;


        public LinqToSitecoreFileGenerator(Item item) 
        {
            _item = item;
            _item.Name = FixName(_item.Name);
        }

        private CodeTypeDeclaration CreateClass()
        {

            var typeDeclaration = new CodeTypeDeclaration(_item.Name)
            {
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Class
            };

            if (_item.IsSystemIncluded)
            {
                typeDeclaration.Members.Add(CreateSystemProperty("Id", "Guid", "Id"));
                typeDeclaration.Members.Add(CreateSystemProperty("ParentId", "Guid", "ParentId"));
                typeDeclaration.Members.Add(CreateSystemProperty("Item", "Item", "Item"));
                typeDeclaration.Members.Add(CreateSystemProperty("Path", "System.String", "Path"));
                typeDeclaration.Members.Add(CreateSystemProperty("TemplateId", "Guid", "TemplateId"));
            }

            foreach (var field in _item.Fields.Where(x => x.IsChecked))
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

        private CodeMemberField CreateSystemProperty(string name, string typeName, string attrType)
        {
            var bpt = GetBuiltInType(typeName);
            var mField = bpt == null ? new CodeMemberField(typeName, name) : new CodeMemberField(bpt, name);


            var attrParam =
                new CodeAttributeArgument(new CodeTypeReferenceExpression($"SitecoreSystemPropertyType.{attrType}"));
            var attr = new CodeAttributeDeclaration("SitecoreSystemProperty", attrParam);
            mField.CustomAttributes.Add(attr);
            mField.Name += " { get; set; }";
            return mField;
        }

        private CodeMemberField CreateProperty(Field field)
        {
            var bpt = GetBuiltInType(field.NetType);
            var mField = bpt == null
                ? new CodeMemberField(field.NetType, FixName(field.PropertyName))
                : new CodeMemberField(bpt, FixName(field.PropertyName));
            mField.Attributes = MemberAttributes.Public;

            if (field.Name != mField.Name)
            {
                mField.CustomAttributes.Add(CreateLinqToSitecoreAttribute(field.Name));
            }

            mField.Name += " { get; set; }";
            return mField;

        }

        private string FixName(string name)
        {
            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            var rgx = new Regex("[^a-zA-Z0-9_]");
            return rgx.Replace( textInfo.ToTitleCase(name), string.Empty);

        }

        private Type GetBuiltInType(string fieldNetType)
        {
            switch (fieldNetType)
            {
                case "String":
                    return typeof(string);
                case "Guid":
                    return typeof(Guid);
                case "Int32":
                    return typeof(int);
                case "Decimal":
                    return typeof(decimal);
                case "Boolean":
                    return typeof(bool);
                case "Single":
                    return typeof(float);
                case "float":
                    return typeof(float);
                case "Byte":
                    return typeof(byte);
                case "Char":
                    return typeof(char);
                case "Int16":
                    return typeof(short);
                case "Int64":
                    return typeof(long);
                case "Double":
                    return typeof(double);
                default:
                    return null;
            }
        }
      

        private CodeAttributeDeclaration CreateLinqToSitecoreAttribute(string fieldName)
        {
            var attrParam = new CodeAttributeArgument(new CodePrimitiveExpression(fieldName));
            var attr = new CodeAttributeDeclaration("SitecoreField", attrParam);
            return attr;
        }

        private CodeCompileUnit CreateCodeCompileUnit(string codenamespace)
        {
            var code = new CodeCompileUnit();
        


            var codeNamespace = string.IsNullOrEmpty(codenamespace) ?  new CodeNamespace() : new CodeNamespace(codenamespace);
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("LinqToSitecore"));

            if (_item.IsSystemIncluded)
            {
                codeNamespace.Imports.Add(new CodeNamespaceImport("Sitecore.Data.Items"));
            }

            codeNamespace.Types.Add(CreateClass());
            code.Namespaces.Add(codeNamespace);

            return code;
        }

        public bool SaveToFile()
        {
            var code = GenerateCode();
            LinqToSitecoreFactory.DteService.ItemOperations.NewFile(@"General\Visual C# Class", _item.DisplayName,
            Constants.vsProjectItemKindPhysicalFile);

            var txtSel = (TextSelection)LinqToSitecoreFactory.DteService.ActiveDocument.Selection;
            txtSel.SelectAll();
            txtSel.Delete();
            txtSel.Insert(code);
            txtSel.MoveTo(1, 1);

            var projectDir = new FileInfo(LinqToSitecoreFactory.Project.FullName).Directory?.FullName;
            var namespacepath = _item.Namespace.EndsWith(".") ? _item.Namespace : _item.Namespace + '.';


            string pattern = $@"^(?<project>{LinqToSitecoreFactory.ProjectNamespace}\.)(?<sub>.*)";
            var regexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant;
            var regex = new Regex(pattern, regexOptions);

            namespacepath = regex.Replace(namespacepath, @"${sub}");
            namespacepath = namespacepath.Replace('.', '\\');


            if (!Directory.Exists($@"{projectDir}\{namespacepath}"))
            {
                Directory.CreateDirectory($@"{projectDir}\{namespacepath}");
            }

            var codeFilePath = $@"{projectDir}\{namespacepath}{_item.DisplayName}.cs";

            try
            {

                LinqToSitecoreFactory.DteService.ActiveDocument.Save(codeFilePath);
                LinqToSitecoreFactory.Project.ProjectItems.AddFromFile(codeFilePath);
                return true;
            }
            catch (Exception)
            {
                return false;
           
            }
        }
        public string GenerateCode()
        {
            var provider = CodeDomProvider.CreateProvider("C#");
            using (var writer = new StringWriter(new StringBuilder()))
            {
                var options = new CodeGeneratorOptions
                {
                    BlankLinesBetweenMembers = true,
                    BracingStyle = "Block"
                };


                provider.GenerateCodeFromCompileUnit(CreateCodeCompileUnit(_item.Namespace), writer, options);

                writer.Flush();
                var enc = Encoding.GetEncoding(writer.Encoding.WindowsCodePage);
                var preamble = enc.GetPreamble();

                var body = enc.GetBytes(writer.ToString());
                var preambleLength = preamble.Length;

                Array.Resize(ref preamble, preambleLength + body.Length);
                Array.Copy(body, 0, preamble, preambleLength, body.Length);

                //TODO: find a better way to print gets and sets
                return writer.ToString().Replace("};", "}"); 
            }

        }
    }
}