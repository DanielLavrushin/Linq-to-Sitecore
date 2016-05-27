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

        public static List<T> OfType<T>(this Database database,
            Expression<Func<T, bool>> query) where T : class, new()
        {
            return OfType<T>(database, null, query);
        }
        public static List<T> OfType<T>(this Database database, bool lazyLoading) where T : class, new()
        {
            return OfType<T>(database, null, null, lazyLoading);
        }
        public static List<T> OfType<T>(this Database database, Expression<Func<T, bool>> query, bool lazyLoading) where T : class, new()
        {
            return OfType<T>(database, null, query, lazyLoading);
        }
        public static List<T> OfType<T>(this Database database, string path = null, Expression<Func<T, bool>> query = null, bool lazyLoading = false) where T : class, new()
        {
            var items = database.SelectItems(SitecoreQueryWorker.ToSitecoreQuery(query, path));

            var col = items.ToList<T>(lazyLoading);
            return col;
        }
        #endregion

        #region Any

        public static bool Any<T>(this Database database, string path = null, Expression<Func<T, bool>> query = null, bool lazyLoading = false) where T : class, new()
        {
            return database.SelectSingleItem(SitecoreQueryWorker.ToSitecoreQuery(query, path)).ReflectTo<T>(lazyLoading) == null;
        }
        #endregion

        #region Count


        public static int Count<T>(this Database database, string path = null, Expression<Func<T, bool>> query = null, bool lazyLoading = false) where T : class, new()
        {
            return database.SelectItems(SitecoreQueryWorker.ToSitecoreQuery(query, path)).ToList<T>(lazyLoading).Count();
        }



        public static int Count<T>(this Item[] items, Expression<Func<T, bool>> query = null, bool lazyLoading = false) where T : class, new()
        {
            return items.ToList<T>(query, lazyLoading).Count;
        }


        public static int Count<T>(this ChildList items, Expression<Func<T, bool>> query = null, bool lazyLoading = false) where T : class, new()
        {
            return items.ToList<T>(query, lazyLoading).Count;
        }
        #endregion


        #region Where

        public static List<T> Where<T>(this Database database, Expression<Func<T, bool>> query, string path = null, bool lazyLoading = false) where T : class, new()
        {
            return database
                .SelectItems(SitecoreQueryWorker.ToSitecoreQuery(query, path))
                .ToList<T>(lazyLoading).ToList();
        }

        public static List<T> Where<T>(this Item[] items, Expression<Func<T, bool>> query, bool lazyLoading = false) where T : class, new()
        {
            return items.ToList(query, lazyLoading);
        }

        public static List<T> Where<T>(this ChildList items, Expression<Func<T, bool>> query, bool lazyLoading) where T : class, new()
        {
            return items.ToList(query, lazyLoading);
        }
        #endregion


        #region FirstOrDefault

        public static T FirstOrDefault<T>(this Database database, string path = null, Expression<Func<T, bool>> query = null, bool lazyLoading = false) where T : class, new()
        {
            return database.SelectSingleItem(SitecoreQueryWorker.ToSitecoreQuery<T>(query, path))?.ReflectTo<T>(lazyLoading);
        }

        public static T FirstOrDefault<T>(this Item[] items, string path = null, Expression<Func<T, bool>> query = null, bool lazyLoading = false) where T : class, new()
        {
            return items.ToList(query, lazyLoading)?.FirstOrDefault();
        }
        public static T FirstOrDefault<T>(this ChildList items, string path = null, Expression<Func<T, bool>> query = null, bool lazyLoading = false) where T : class, new()
        {
            return items.ToList(query, lazyLoading)?.FirstOrDefault();
        }
        #endregion

        public static decimal Max<T>(this Item[] items, Expression<Func<T, decimal>> query, bool lazyLoading = false) where T : class, new()
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
            var tid = SitecoreQueryWorker.GetTemplateIdFromType<T>();
            if (!tid.IsNull)
            {
                return item.TemplateID == tid;
            }

            var ntname = SitecoreQueryWorker.GetTemplateNameFromType<T>();
            return item.TemplateName == ntname;
        }

        public static List<T> ToList<T>(this Item[] items, bool lazyLoading = false) where T : class, new()
        {
            return items.Select(s => s.ReflectTo<T>(lazyLoading)).Where(x => x != null).ToList();
        }
        public static List<T> ToList<T>(this Item[] items, Expression<Func<T, bool>> query, bool lazyLoading = false) where T : class, new()
        {
            return items.Select(s => s.ReflectTo<T>(lazyLoading)).Where(x => x != null).Where(query.Compile()).ToList();
        }
        public static List<T> ToList<T>(this ChildList items, bool lazyLoading = false) where T : class, new()
        {
            return items.Select(s => s.ReflectTo<T>(lazyLoading)).Where(x => x != null).ToList();
        }

        public static List<T> ToList<T>(this ChildList items, Expression<Func<T, bool>> query, bool lazyLoading = false) where T : class, new()
        {
            return items.Select(s => s.ReflectTo<T>(lazyLoading)).Where(x => x != null).Where(query.Compile()).ToList();
        }

        public static T Parent<T>(this Item item, bool lazyLoading = false) where T : class, new()
        {
            T parent = default(T);
            if (item != null && !item.ParentID.IsNull)
            {
                parent = item.Parent.ReflectTo<T>();
            }

            if (parent == null && lazyLoading)
            {
                item?.Database.GetItem(item.ParentID)?.ReflectTo<T>();
            }

            return parent;
        }

        public static List<T> Children<T>(this Item item, bool lazyLoading = false) where T : class, new()
        {
            List<T> coll = null;
            if (item?.Children != null && item.Children.Any())
            {
                coll = item.Children.ToList<T>();

                return coll;
            }

            if (lazyLoading)
            {
                coll = item?.Database.SelectItems($"{item.Paths.Path}//*").ToList<T>();
            }

            //foreach (var c in coll)
            //{
            //    var itemProp = SitecoreQueryWorker.GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Item);
            //    if (itemProp != null)
            //    {
            //        var subItem = (Item)itemProp.GetValue(c, null);
            //        Children<T>(c, lazyLoading);
            //    }
            //}


            return coll ?? new List<T>();
        }

        public static T ReflectTo<T>(this Item item, bool lazyLoading = false)
             where T : class, new()
        {
            if (item == null) return default(T);

            if (!item.IsOfType<T>()) return null;

            var o = new T();

            var itemProp = SitecoreQueryWorker.GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Item);
            var idProp = SitecoreQueryWorker.GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Id);
            var nameProp = SitecoreQueryWorker.GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Name);
            var parentIdProp = SitecoreQueryWorker.GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.ParentId);
            var pathProp = SitecoreQueryWorker.GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Path);
            var templateIdProp = SitecoreQueryWorker.GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.TemplateId);
            var parentProp = SitecoreQueryWorker.GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Parent);

            idProp?.SetValue(o, item.ID.Guid);
            nameProp?.SetValue(o, item.Name);
            pathProp?.SetValue(o, item.Paths?.Path);
            parentIdProp?.SetValue(o, item?.ParentID?.Guid);
            templateIdProp?.SetValue(o, item?.TemplateID?.Guid);
            itemProp?.SetValue(o, item);

            parentProp?.SetValue(o, item?.Parent?.ReflectTo<T>(lazyLoading));


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
                            else if (f.PropertyType == typeof(ICollection<>) || f.PropertyType == typeof(List<>) || f.PropertyType == typeof(IEnumerable<>))
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
                            else if (lazyLoading && typeof(List<>).IsAssignableFrom(f.PropertyType.GetGenericTypeDefinition()))
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
                                    f.SetValue(o, field.Value);
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
