using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;

namespace LinqToSitecore
{
    public static class LinqToSitecoreExtensions
    {

        public static ICollection<T> OfType<T>(this Database database, bool lazyLoading = false) where T : class, new()
        {
            var items = database.SelectItems(LambdaToSitecoreQuery<T>(null));

            var col = items.ToList<T>(lazyLoading);
            return col;
        }

        public static ICollection<T> OfType<T>(this Database database, Expression<Func<T, bool>> query, bool lazyLoading = false) where T : class, new()
        {
            var items = database.SelectItems(LambdaToSitecoreQuery(query));

            var col = items.ToList<T>(lazyLoading);
            return col;
        }
        public static ICollection<T> OfType<T>(this Database database, string path, Expression<Func<T, bool>> query, bool lazyLoading = false) where T : class, new()
        {
            var items = database.SelectItems(LambdaToSitecoreQuery(query, path));

            var col = items.ToList<T>(lazyLoading);
            return col;
        }



        public static bool Any<T>(this Database database, Expression<Func<T, bool>> query = null) where T : class, new()
        {
            return database.SelectItems(LambdaToSitecoreQuery(query)).ToList<T>().Any();
        }

        public static bool Any<T>(this Database database, string path = null, Expression<Func<T, bool>> query = null) where T : class, new()
        {
            return database.SelectItems(LambdaToSitecoreQuery(query, path)).ToList<T>().Any();
        }

        public static int Count<T>(this Database database) where T : class, new()
        {
            return database.SelectItems(LambdaToSitecoreQuery<T>(null, null)).Count();
        }

        public static int Count<T>(this Database database, Expression<Func<T, bool>> query) where T : class, new()
        {
            return database.SelectItems(LambdaToSitecoreQuery(query)).Count();
        }


        public static int Count<T>(this Database database, string path, Expression<Func<T, bool>> query = null) where T : class, new()
        {
            return database.SelectItems(LambdaToSitecoreQuery(query, path)).Count();
        }

        public static int Count<T>(this Item[] items) where T : class, new()
        {
            return items.ToList<T>().Count;
        }
        public static int Count<T>(this Item[] items, Expression<Func<T, bool>> query) where T : class, new()
        {
            return items.ToList<T>(query).Count;
        }

        public static int Count<T>(this ChildList items) where T : class, new()
        {
            return items.ToList<T>().Count;
        }
        public static int Count<T>(this ChildList items, Expression<Func<T, bool>> query) where T : class, new()
        {
            return items.ToList<T>(query).Count;
        }

        public static ICollection<T> Where<T>(this Database database, Expression<Func<T, bool>> query) where T : class, new()
        {
            return database.SelectItems(LambdaToSitecoreQuery(query)).ToList<T>()
                .Where(query.Compile()).ToList();
        }

        public static ICollection<T> Where<T>(this Item[] items, Expression<Func<T, bool>> query) where T : class, new()
        {
            return items.ToList<T>(query).ToList();
        }
        public static ICollection<T> Where<T>(this ChildList items, Expression<Func<T, bool>> query) where T : class, new()
        {
            return items.ToList<T>(query).ToList();
        }

        public static T FirstOrDefault<T>(this Database database) where T : class, new()
        {
            return database.SelectSingleItem(LambdaToSitecoreQuery<T>(null))?.ReflectTo<T>();
        }

        public static T FirstOrDefault<T>(this Database database, Expression<Func<T, bool>> query) where T : class, new()
        {
            return database.SelectSingleItem(LambdaToSitecoreQuery(query))?.ReflectTo<T>();
        }

        public static T FirstOrDefault<T>(this Item[] items) where T : class, new()
        {
            return items.ToList<T>()?.FirstOrDefault();
        }

        public static T FirstOrDefault<T>(this Item[] items, Expression<Func<T, bool>> query) where T : class, new()
        {
            return items.ToList(query)?.FirstOrDefault();
        }

        public static T FirstOrDefault<T>(this ChildList items) where T : class, new()
        {
            return items.ToList<T>()?.FirstOrDefault();
        }

        public static T FirstOrDefault<T>(this ChildList items, Expression<Func<T, bool>> query) where T : class, new()
        {
            return items.ToList(query)?.FirstOrDefault();
        }

        public static decimal Max<T>(this Item[] items, Expression<Func<T, decimal>> query) where T : class, new()
        {
            return items.ToList<T>().Max(query.Compile());
        }
        public static decimal Max<T>(this ChildList items, Expression<Func<T, decimal>> query) where T : class, new()
        {
            return items.ToList<T>().Max(query.Compile());
        }
        public static decimal Min<T>(this Item[] items, Expression<Func<T, decimal>> query) where T : class, new()
        {
            return items.ToList<T>().Max(query.Compile());
        }
        public static decimal Min<T>(this ChildList items, Expression<Func<T, decimal>> query) where T : class, new()
        {
            return items.ToList<T>().Max(query.Compile());
        }
        public static string LambdaToSitecoreQuery<T>(Expression<Func<T, bool>> query, string path = null) where T : class
        {

            var expBody = query?.Body?.ToString();
            if (!string.IsNullOrEmpty(expBody))
            {
                var paramName = query.Parameters[0].Name;
                var paramTypeName = query.Parameters[0].Type.Name;

                expBody = ExpressionToStringResolver(query.Body, expBody);

                var m = Regex.Replace(expBody, @"(\.Contains\(.(?<g1>.+?).\))", " = '%$1%'", RegexOptions.ExplicitCapture);
                var m2 = Regex.Replace(m, @"Not\((?<g1>[^=]+?)\)", "($1 != 1)", RegexOptions.ExplicitCapture);
                var m3 = Regex.Replace(m2, @"Not\((?<g1>.+?)\)", "($1)", RegexOptions.ExplicitCapture);

                expBody = Regex.Replace(m3, @"(?<q1>\.[a-zA-Z0-9]+)(:?\)|\s\w+|$)", "$1 = 1", RegexOptions.ExplicitCapture);


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



            if (!string.IsNullOrEmpty(path) && !path.EndsWith("//"))
            {
                path = $"{path}//";
            }
            var scQuery = $"fast://{path}*[@@templateId='{tempId}' {expBody}]";
            if (tempId == ID.Null)
            {
                scQuery = $"fast://{path}*[@@templatename='{GetTemplateNameFromType<T>()}' {expBody}]";
            }

            return scQuery;
        }

        private static ID GetTemplateIdFromType<T>() where T : class
        {
            var templateAttr =
                (SitecoreTemplateAttribute)
                    typeof(T).GetCustomAttributes(typeof(SitecoreTemplateAttribute), true).FirstOrDefault();
            return templateAttr?.TemplateId ?? ID.Null;
        }

        private static string GetTemplateNameFromType<T>() where T : class
        {
            var templateAttr = typeof(T).Name;
            return templateAttr;
        }

        private static PropertyInfo GetTemplateSystemProperty<T>(SitecoreSystemPropertyType type) where T : class
        {

            var prop = typeof(T).GetProperties().FirstOrDefault(s => s.GetCustomAttributes<SitecoreSystemPropertyAttribute>().Any(a => a.FieldType == type));
            return prop;
        }

        public static bool IsOfType<T>(this Item item) where T : class
        {
            var tid = GetTemplateIdFromType<T>();
            if (!tid.IsNull)
            {
                return item.TemplateID == tid;
            }

            var ntname = GetTemplateNameFromType<T>();
            return item.TemplateName == ntname;
        }

        public static ICollection<T> ToList<T>(this Item[] items, bool lazyLoading = false) where T : class, new()
        {
            return items.Select(s => s.ReflectTo<T>(lazyLoading)).Where(x => x != null).ToList();
        }
        public static ICollection<T> ToList<T>(this Item[] items, Expression<Func<T, bool>> query, bool lazyLoading = false) where T : class, new()
        {
            return items.Select(s => s.ReflectTo<T>(lazyLoading)).Where(x => x != null).Where(query.Compile()).ToList();
        }
        public static ICollection<T> ToList<T>(this ChildList items, bool lazyLoading = false) where T : class, new()
        {
            return items.Select(s => s.ReflectTo<T>(lazyLoading)).Where(x => x != null).ToList();
        }

        public static ICollection<T> ToList<T>(this ChildList items, Expression<Func<T, bool>> query, bool lazyLoading = false) where T : class, new()
        {
            return items.Select(s => s.ReflectTo<T>(lazyLoading)).Where(x => x != null).Where(query.Compile()).ToList();
        }

        public static T ReflectTo<T>(this Item item, bool lazyLoading = false)
             where T : class, new()
        {
            if (item == null) return default(T);

            if (!item.IsOfType<T>()) return null;

            var o = new T();


            var idProp = GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Id);
            var nameProp = GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Name);

            idProp?.SetValue(o, item.ID);
            nameProp?.SetValue(o, item.Name);


            foreach (var f in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty))
            {

                var field =
                    item.Fields.FirstOrDefault(s => s.Name.Replace(" ", string.Empty).ToLower() == f.Name.ToLower());
                if (field == null)
                {
                    var sfn = (SitecoreFieldAttribute)f.GetCustomAttributes(typeof(SitecoreFieldAttribute)).FirstOrDefault();
                    if (sfn != null)
                    {
                        field = item.Fields.FirstOrDefault(s => s.Name.ToLower() == sfn.Name.ToLower());

                    }
                }

                if (field != null)
                {
                    try
                    {
                        object value = field.Value;
                        try
                        {
                            if (f.PropertyType == typeof(bool))
                            {
                                f.SetValue(o, (string)value == "1");
                            }
                            else if (f.PropertyType == typeof(DateTime))
                            {
                                f.SetValue(o, ((DateField)field).DateTime);
                            }
                            else if (f.PropertyType == typeof(byte[]))
                            {
                                if (field.TypeKey == "file" || field.TypeKey == "image")
                                {
                                    var fitem = (FileField)field;
                                    var mitem = ((MediaItem)fitem.MediaItem).GetMediaStream();

                                    byte[] bytes;
                                    using (var memstream = new MemoryStream())
                                    {
                                        mitem.CopyTo(memstream);
                                        bytes = memstream.ToArray();
                                    }
                                    f.SetValue(o, bytes);
                                }
                            }
                            else if (f.PropertyType == typeof(ICollection<>))
                            {

                            }
                            else if (field.TypeKey == "droplink" && lazyLoading)
                            {
                                var dropid = ID.Null;
                                if (ID.TryParse(field.Value, out dropid))
                                {
                                    //potential loop
                                    if (dropid != item.ID)
                                    {
                                    }
                                    if (f.PropertyType != typeof(ID) && f.PropertyType != typeof(Guid) &&
                                        f.PropertyType != typeof(string))
                                    {
                                        var dropitem = item.Database.GetItem(dropid);
                                        var result = typeof(LinqToSitecoreExtensions)
                                            .GetMethod("ReflectTo")
                                            .MakeGenericMethod(f.PropertyType)
                                            .Invoke(dropitem, new object[] { dropitem, lazyLoading });
                                        f.SetValue(o, result);
                                    }
                                }

                            }
                            else if (lazyLoading && typeof(ICollection<>).IsAssignableFrom(f.PropertyType.GetGenericTypeDefinition()))
                            {
                                var multiField = (MultilistField)field;
                                var colgenType = f.PropertyType.GetGenericArguments().FirstOrDefault();
                                var subItems = multiField.GetItems();
                                if (subItems.Any())
                                {
                                    var result = typeof(LinqToSitecoreExtensions).GetMethod("ToList",
                                        new Type[] { typeof(Item[]), typeof(bool) })
                                        .MakeGenericMethod(colgenType)
                                        .Invoke(subItems, new object[] { subItems, lazyLoading });
                                    f.SetValue(o, result);
                                }
                            }
                            else
                            {
                                if (field.TypeKey == "file" || field.TypeKey == "image")
                                {
                                    var fitem = (FileField)field;
                                    f.SetValue(o, fitem.Src);
                                }
                                else
                                {

                                    var convertedObj = Convert.ChangeType(value, f.PropertyType);
                                    f.SetValue(o, field.TypeKey);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            f.SetValue(o, value);

                        }
                    }
                    catch (Exception)
                    {
                        //   p.SetValue(o, null);
                    }
                }
            }
            return o;
        }



        private static string ExpressionToStringResolver(Expression expr, string query)
        {
            if (expr is BinaryExpression)
            {
                var binaryExpr = expr as BinaryExpression;
                if (binaryExpr.Left.ToString().StartsWith("value(") && binaryExpr.Left is MemberExpression)
                {
                    var oldValue = binaryExpr.Left.ToString();
                    var memberExpr = binaryExpr.Left as MemberExpression;
                    var path = string.Join(".", MemberExpressionToPath(binaryExpr.Left as MemberExpression).ToArray().Reverse());
                    var value = SearchConstantExpression(memberExpr)?.Value;
                    var finalValue = GetConstantExpressionValue(value, path);
                    query = query.Replace(oldValue, $"'{finalValue}'");
                }

                if (binaryExpr.Right.ToString().StartsWith("value(") && binaryExpr.Right is MemberExpression)
                {
                    var oldValue = binaryExpr.Right.ToString();
                    var memberExpr = binaryExpr.Right as MemberExpression;
                    var path = string.Join(".", MemberExpressionToPath(binaryExpr.Right as MemberExpression).ToArray().Reverse());
                    var value = SearchConstantExpression(memberExpr)?.Value;
                    var finalValue = GetConstantExpressionValue(value, path);
                    query = query.Replace(oldValue, $"'{finalValue}'");
                }

                query = ExpressionToStringResolver(binaryExpr.Left, query);
                query = ExpressionToStringResolver(binaryExpr.Right, query);

            }
            return query;
        }

        private static object GetConstantExpressionValue(object value, string path)
        {
            var spl = path.Split('.');
            foreach (var p in spl)
            {
                var t = value.GetType();

                var info = t.GetField(p) ?? t.GetProperty(p) as MemberInfo;
                if (info is FieldInfo)
                {
                    var pt = (FieldInfo)info;
                    if (spl.Length == 1)
                    {
                        return pt.GetValue(value);

                    }
                    return GetConstantExpressionValue(pt.GetValue(value), string.Join(".", spl.Skip(1).ToArray()));
                }

                if (info is PropertyInfo)
                {
                    var pt = (PropertyInfo)info;
                    if (spl.Length == 1)
                    {
                        return pt.GetValue(value, null);

                    }
                    return GetConstantExpressionValue(pt.GetValue(value, null), string.Join(".", spl.Skip(1).ToArray()));
                }
            }
            return null;
        }

        private static ICollection<string> MemberExpressionToPath(MemberExpression expr, ICollection<string> path = null)
        {
            if (path == null)
            {
                path = new List<string>();
            }

            path.Add(expr.Member.Name);

            if (expr.Expression is MemberExpression)
            {
                path = MemberExpressionToPath(expr.Expression as MemberExpression, path);
            }

            return path;
        }

        private static ConstantExpression SearchConstantExpression(MemberExpression expr)
        {
            if (expr.Expression is ConstantExpression)
            {
                return (ConstantExpression)expr.Expression;
            }
            if (expr.Expression is MemberExpression)
            {
                return SearchConstantExpression(expr.Expression as MemberExpression);
            }
            return null;
        }
    }
}
