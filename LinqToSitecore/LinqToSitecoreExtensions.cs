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

        #region OfType
        public static ICollection<T> OfType<T>(this Database database) where T : class, new()
        {
            return OfType<T>(database, null, null, false);

        }

        public static ICollection<T> OfType<T>(this Database database, Expression<Func<T, bool>> query) where T : class, new()
        {
            return OfType<T>(database, null, query, false);

        }
        public static ICollection<T> OfType<T>(this Database database, string path, bool lazyLoading) where T : class, new()
        {
            return OfType<T>(database, path, null, lazyLoading);
        }

        public static ICollection<T> OfType<T>(this Database database, string path, Expression<Func<T, bool>> query, bool lazyLoading) where T : class, new()
        {
            var items = database.SelectItems(SitecoreExpression.ToSitecoreQuery(query, path));

            var col = items.ToList<T>(lazyLoading);
            return col;
        }
        #endregion

        #region Any
        public static bool Any<T>(this Database database, string path) where T : class, new()
        {
            return Any<T>(database, path, null);
        }
        public static bool Any<T>(this Database database, Expression<Func<T, bool>> query) where T : class, new()
        {
            return Any<T>(database, null, query);
        }
        public static bool Any<T>(this Database database, string path, Expression<Func<T, bool>> query) where T : class, new()
        {
            return database.SelectSingleItem(SitecoreExpression.ToSitecoreQuery(query, path)) == null;
        }
        #endregion

        #region Count

        public static int Count<T>(this Database database) where T : class, new()
        {
            return Count<T>(database, null, null);
        }
        public static int Count<T>(this Database database, string path) where T : class, new()
        {
            return Count<T>(database, path, null);
        }
        public static int Count<T>(this Database database, Expression<Func<T, bool>> query) where T : class, new()
        {
            return Count<T>(database, null, query);
        }
        public static int Count<T>(this Database database, string path, Expression<Func<T, bool>> query) where T : class, new()
        {
            return database.SelectItems(SitecoreExpression.ToSitecoreQuery(query, path)).ToList<T>().Count();
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
        #endregion


        #region Where
        public static ICollection<T> Where<T>(this Database database, Expression<Func<T, bool>> query) where T : class, new()
        {
            return Where<T>(database, null, query);
        }

        public static ICollection<T> Where<T>(this Database database, string path, Expression<Func<T, bool>> query) where T : class, new()
        {
            return database
                .SelectItems(SitecoreExpression.ToSitecoreQuery(query, path))
                .ToList<T>()
                .Where(query.Compile()).ToList();
        }

        public static ICollection<T> Where<T>(this Item[] items, Expression<Func<T, bool>> query) where T : class, new()
        {
            return items.ToList(query);
        }
        public static ICollection<T> Where<T>(this ChildList items, Expression<Func<T, bool>> query) where T : class, new()
        {
            return items.ToList(query);
        }
        #endregion


        #region FirstOrDefault
        public static T FirstOrDefault<T>(this Database database) where T : class, new()
        {
            return FirstOrDefault<T>(database, null, null);
        }

        public static T FirstOrDefault<T>(this Database database, Expression<Func<T, bool>> query) where T : class, new()
        {
            return FirstOrDefault<T>(database, null, query);
        }
        public static T FirstOrDefault<T>(this Database database, string path) where T : class, new()
        {
            return FirstOrDefault<T>(database, path, null);
        }
        public static T FirstOrDefault<T>(this Database database, string path, Expression<Func<T, bool>> query) where T : class, new()
        {
            return database.SelectSingleItem(SitecoreExpression.ToSitecoreQuery<T>(query, path))?.ReflectTo<T>();
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

        #endregion

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


        public static bool IsOfType<T>(this Item item) where T : class
        {
            var tid = SitecoreExpression.GetTemplateIdFromType<T>();
            if (!tid.IsNull)
            {
                return item.TemplateID == tid;
            }

            var ntname = SitecoreExpression.GetTemplateNameFromType<T>();
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


            var idProp = SitecoreExpression.GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Id);
            var nameProp = SitecoreExpression.GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Name);

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



    }
}
