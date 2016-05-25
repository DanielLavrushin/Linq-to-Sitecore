using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sitecore.Data;

namespace LinqToSitecore
{
    internal partial class SitecoreQueryWorker
    {
        internal static string ToSitecoreQuery<T>(Expression<Func<T, bool>> query, string path = null) where T : class
        {

            var expBody = query?.Body?.ToString();
            if (!string.IsNullOrEmpty(expBody))
            {
                var paramName = query.Parameters[0].Name;
                var paramTypeName = query.Parameters[0].Type.Name;

                expBody = ExpressionEvaluator.EvalToString(query.Body);

                var m = Regex.Replace(expBody.ToString(), @"^.+?=>(?<q1>.+)", "$1", RegexOptions.ExplicitCapture).Trim();
                m = Regex.Replace(m, @"(\.Contains\(.(?<g1>.+?).\))", " = '%$1%'", RegexOptions.ExplicitCapture);
                m = Regex.Replace(m, @"(\.Equals\(.(?<g1>.+?).\))", " = '$1'", RegexOptions.ExplicitCapture);
                m = Regex.Replace(m, @"(\.StartsWith\(.(?<g1>.+?).\))", " = '$1%'", RegexOptions.ExplicitCapture);
                m = Regex.Replace(m, @"(\.EndsWith\(.(?<g1>.+?).\))", " = '%$1'", RegexOptions.ExplicitCapture);
                m = Regex.Replace(m, @"Not\((?<g1>[^=]+?)\)", "($1 != 1)", RegexOptions.ExplicitCapture);
                m = Regex.Replace(m, @"Not\((?<g1>.+?)=(?<g2>.+?)\)", "($1!=$2)", RegexOptions.ExplicitCapture);
                m = Regex.Replace(m, @"(?<q1>\.[a-zA-Z0-9]+)(:?\)|\s\w+|$)", "$1 = 1", RegexOptions.ExplicitCapture);

                expBody = Regex.Replace(m, @"(?<q1>\.[a-zA-Z0-9]+)(:?\)|\s\w+|$)", "$1 = 1", RegexOptions.ExplicitCapture);


                expBody = expBody.Replace(paramName + ".", "@")
                     .Replace("AndAlso", "and")
                     .Replace("OrElse", "or")
                     .Replace("==", "=")
                     .Replace("\"", "'")
                     .Replace("True", "1")
                     .Replace("False", "0");

                expBody = expBody.Replace("@Name", "@@Name").Replace("@Id", "@@Id");

                var props =
                    typeof(T).GetProperties().Where(s => s.GetCustomAttributes<SitecoreFieldAttribute>().Any()).ToList();

                if (props.Any())
                {
                    foreach (var prop in props)
                    {
                        var scFieldName = prop.GetCustomAttributes<SitecoreFieldAttribute>().FirstOrDefault()?.Name;
                        if (!string.IsNullOrEmpty(scFieldName))
                        {
                            if (prop.PropertyType == typeof(bool))
                            {
                                expBody = expBody.Replace($"@!{prop.Name}", $"@{prop.Name} = 0");
                            }

                            expBody = expBody.Replace($"@{prop.Name}", $"@{scFieldName}");
                        }
                    }
                }


            }
            expBody = string.IsNullOrEmpty(expBody) ? string.Empty : $"and {expBody}";
            var tempId = GetTemplateIdFromType<T>();

            path = !string.IsNullOrEmpty(path) ? path : string.Empty;
            path = !string.IsNullOrEmpty(path) && !path.EndsWith("//*") ? $"{path}//*" : "/*";

            var scQuery = $"fast:/{path}[@@templateId='{tempId}' {expBody}]";
            if (tempId == ID.Null)
            {
                scQuery = $"fast:/{path}[@@templatename='{GetTemplateNameFromType<T>()}' {expBody}]";
            }

            return scQuery;
        }

        internal static ID GetTemplateIdFromType<T>() where T : class
        {
            var templateAttr =
                (SitecoreTemplateAttribute)
                    typeof(T).GetCustomAttributes(typeof(SitecoreTemplateAttribute), true).FirstOrDefault();
            return templateAttr?.TemplateId ?? ID.Null;
        }

        internal static string GetTemplateNameFromType<T>() where T : class
        {
            var templateAttr = typeof(T).Name;
            return templateAttr;
        }

        internal static PropertyInfo GetTemplateSystemProperty<T>(SitecoreSystemPropertyType type) where T : class
        {

            var prop = typeof(T).GetProperties().FirstOrDefault(s => s.GetCustomAttributes<SitecoreSystemPropertyAttribute>().Any(a => a.FieldType == type));
            return prop;
        }

    }
}
