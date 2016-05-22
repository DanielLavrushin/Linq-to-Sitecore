using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace LinqToSitecore
{
    public static class LinqToSitecoreExtensions
    {
        public static ICollection<T> OfType<T>(this Database database, string path = null) where T : class, new()
        {
            var coll = new List<T>();
            var tempId = GetTemplateIdFromType<T>();

            if (!string.IsNullOrEmpty(path) && !path.EndsWith("//"))
            {
                path = $"{path}//";
            }

            var q = $"fast://{path}*[@@templateId='{tempId}']";
            if (tempId == ID.Null)
            {
                q = $"fast://{path}*[@@templatename='{GetTemplateNameFromType<T>()}']";
            }
            var items = database.SelectItems(q);

            var col = items.ToList<T>();
            return col;

        }

        public static ICollection<T> Where<T>(this Database database, Expression<Func<T, bool>> query) where T : class, new()
        {
            return database.SelectItems(LambdaToSitecoreQuery(query)).ToList<T>()
                .Where(query.Compile()).ToList();
        }
        public static T FirstOrDefault<T>(this Database database, Expression<Func<T, bool>> query) where T : class, new()
        {
            return database.SelectSingleItem(LambdaToSitecoreQuery(query))?.ReflectTo<T>();
        }
        private static string LambdaToSitecoreQuery<T>(Expression<Func<T, bool>> query) where T : class
        {
            string expBody = query.Body.ToString();

            var paramName = query.Parameters[0].Name;
            var paramTypeName = query.Parameters[0].Type.Name;

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

            var tempId = GetTemplateIdFromType<T>();


            var scQuery = $"fast://*[@@templateId='{tempId}' and {expBody}]";
            if (tempId == ID.Null)
            {
                scQuery = $"fast://*[@@templatename='{GetTemplateNameFromType<T>()}' and {expBody}]";
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

        public static ICollection<T> ToList<T>(this Item[] items) where T : class, new()
        {
            return items.Select(s => s.ReflectTo<T>()).ToList();
        }
        public static ICollection<T> ToList<T>(this ChildList items) where T : class, new()
        {
            return items.Select(s => s.ReflectTo<T>()).ToList();
        }



        public static T ReflectTo<T>(this Item item, params Item[] relatedItems)
             where T : class, new()
        {
            if (item == null) return default(T);

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
                        if (ID.IsID(field.Value))
                        {
                            var relatedItem = relatedItems.FirstOrDefault(s => s.ID == new ID(field.Value));

                            if (relatedItem != null)
                            {
                                var method = typeof(LinqToSitecoreExtensions).GetMethod("ReflectTo").MakeGenericMethod(f.PropertyType);

                                value = method.Invoke(null, new object[] { relatedItem, relatedItems });

                            }
                        }

                        try
                        {
                            if (f.PropertyType == typeof(bool))
                            {
                                f.SetValue(o, (string)value == "1");
                            }
                            else
                            {
                                var convertedObj = Convert.ChangeType(value, f.PropertyType);
                                f.SetValue(o, convertedObj);
                            }
                        }
                        catch (Exception)
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
    }
}
