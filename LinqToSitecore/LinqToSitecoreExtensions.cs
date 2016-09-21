using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.DataProviders.Sql;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Data.SqlServer;
using Sitecore.Diagnostics;

namespace LinqToSitecore
{



    public static class LinqToSitecoreExtensions
    {
        #region HELPERS

        private static ID GetTemplateIdFromType<T>()
        {
            var templateAttr =
                (SitecoreTemplateAttribute)
                    typeof(T).GetCustomAttributes(typeof(SitecoreTemplateAttribute), true).FirstOrDefault();
            return templateAttr?.TemplateId ?? ID.Null;
        }

        private static string GetTemplateNameFromType<T>()
        {
            var templateAttr = typeof(T).Name;
            return templateAttr;
        }

        internal static PropertyInfo GetTemplateSystemProperty<T>(SitecoreSystemPropertyType type)
        {

            var prop = typeof(T).GetProperties().FirstOrDefault(s => s.GetCustomAttributes<SitecoreSystemPropertyAttribute>().Any(a => a.FieldType == type));
            return prop;
        }


        public static T Cast<T>(this Expression expr) where T : Expression
        {
            return (T)expr;
        }


        public static T GetFieldValue<T>(this Item item, string field)
        {

            field = field.ToLower();
            var f = item.Fields[field];
            if (!string.IsNullOrEmpty(f?.Value))
            {
                return (T)System.Convert.ChangeType(f.Value, typeof(T));
            }

            return default(T);

        }

        public static object GetFieldValue(this Item item, string field, Type type)
        {

            field = field.ToLower();
            var f = item.Fields[field];
            if (!string.IsNullOrEmpty(f?.Value))
            {
                return System.Convert.ChangeType(f.Value, type);
            }

            return type.IsValueType ? Activator.CreateInstance(type) : null;

        }


        public static bool IsOfType<T>(this Item item)
        {
            var tid = GetTemplateIdFromType<T>();
            if (!tid.IsNull)
            {
                return item.TemplateID == tid;
            }

            var ntname = GetTemplateNameFromType<T>();
            return item.TemplateName == ntname;
        }
        private static LinqToSitecoreQueryTranslator GetQueryTranslator(QueryContext contextNode, Database database)
        {
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var dataProviders = database.GetDataProviders();

            var dataProvider = dataProviders.FirstOrDefault(provider => provider is SqlServerDataProvider);
            if (dataProvider == null)
            {
                return null;
            }

            var field = typeof(SqlDataProvider).GetProperty("Api", BindingFlags.NonPublic | BindingFlags.Instance);

            var api = field?.GetValue(dataProvider, null) as SqlDataApi;

            if (api == null)
            {
                return null;
            }

            return new LinqToSitecoreQueryTranslator(api);
        }


        private static List<T> Query<T>(Database database, Expression<Func<T, bool>> query, bool lazyLoading, params string[] include)
        {
            var db = database ?? Context.Database;

            var opcode = LinqToSitecoreVisitor.GetCode(query, null, typeof(T));
            var context = new QueryContext(db.DataManager);

            var qTranslator = GetQueryTranslator(context, db);

            var contextResult = qTranslator.QueryFast(db, opcode);

            var data = contextResult;

            if (data is QueryContext[])
            {
                return ((QueryContext[])data).Select(x => x.GetQueryContextItem().ReflectTo<T>(lazyLoading, include))
                    .Where(query.Compile()).ToList();
            }

            return new List<T> { ((QueryContext)data).GetQueryContextItem().ReflectTo<T>(lazyLoading, include) };

        }

        private static List<T> Convert<T>(ICollection<Item> items, Expression<Func<T, bool>> query, bool lazyLoading, params string[] include)
        {
            if (items == null) return new List<T>();
            var coll = items.Select(x => x.ReflectTo<T>(lazyLoading, include)).Where(x => x != null).ToList();
            return query == null ? coll : coll.Where(query.Compile()).ToList();
        }
        #endregion


        #region WHERE DATABASE
        public static ICollection<T> Where<T>(this Database database, Expression<Func<T, bool>> query)
        {
            return Query(database, query, false, null);
        }
        public static ICollection<T> Where<T>(this Database database, Expression<Func<T, bool>> query, bool lazyLoading)
        {
            return Query(database, query, lazyLoading, null);
        }

        public static ICollection<T> Where<T>(this Database database, Expression<Func<T, bool>> query, params string[] include)
        {
            return Query(database, query, false, include);
        }
        #endregion

        #region LIST DATABASE
        public static ICollection<T> ToList<T>(this Database database)
        {
            return Query<T>(database, null, false, null);
        }
        public static ICollection<T> ToList<T>(this Database database, bool lazyLoading)
        {
            return Query<T>(database, null, lazyLoading, null);
        }

        public static ICollection<T> ToList<T>(this Database database, params string[] include)
        {
            return Query<T>(database, null, false, include);
        }
        #endregion

        #region FIRSTORDEFAULT DATABASE
        public static T FirstOrDefault<T>(this Database database)
        {
            return Query<T>(database, null, false, null).FirstOrDefault();
        }
        public static T FirstOrDefault<T>(this Database database, bool lazyLoading)
        {
            return Query<T>(database, null, lazyLoading, null).FirstOrDefault();
        }
        public static T FirstOrDefault<T>(this Database database, params string[] include)
        {
            return Query<T>(database, null, false, include).FirstOrDefault();
        }
        public static T FirstOrDefault<T>(this Database database, Expression<Func<T, bool>> query)
        {
            return Query(database, query, false, null).FirstOrDefault();
        }
        public static T FirstOrDefault<T>(this Database database, Expression<Func<T, bool>> query, bool lazyLoading)
        {
            return Query(database, query, lazyLoading, null).FirstOrDefault();
        }
        public static T FirstOrDefault<T>(this Database database, Expression<Func<T, bool>> query, params string[] include)
        {
            return Query(database, query, false, include).FirstOrDefault();
        }
        #endregion

        #region FIRSTORDEFAULT ITEMS

        public static T FirstOrDefault<T>(this IEnumerable<Item> items)
        {
            return Convert<T>(items.ToList(), null, false, null).FirstOrDefault();
        }
        public static T FirstOrDefault<T>(this IEnumerable<Item> items, bool lazyLoading)
        {
            return Convert<T>(items.ToList(), null, lazyLoading, null).FirstOrDefault();
        }
        public static T FirstOrDefault<T>(this IEnumerable<Item> items, params string[] include)
        {
            return Convert<T>(items.ToList(), null, false, include).FirstOrDefault();
        }
        public static T FirstOrDefault<T>(this IEnumerable<Item> items, Expression<Func<T, bool>> query)
        {
            return FirstOrDefault(items, query, false);
        }
        public static T FirstOrDefault<T>(this IEnumerable<Item> items, Expression<Func<T, bool>> query, bool lazyLoading)
        {
            return Convert(items.ToList(), query, lazyLoading, null).FirstOrDefault();
        }
        public static T FirstOrDefault<T>(this IEnumerable<Item> items, Expression<Func<T, bool>> query, params string[] include)
        {
            return Convert(items.ToList(), query, false, include).FirstOrDefault();
        }
        public static T FirstOrDefault<T>(this IEnumerable<Item> items, Expression<Func<T, bool>> query, bool lazyLoading, params string[] include)
        {
            return Convert(items.ToList(), query, lazyLoading, include).FirstOrDefault();
        }
        #endregion

        #region LIST ITEMS
        public static List<T> ToList<T>(this IEnumerable<Item> items)
        {
            return Convert<T>(items.ToList(), null, false, null);
        }
        public static ICollection<T> ToList<T>(this IEnumerable<Item> items, bool lazyLoading)
        {
            return Convert<T>(items.ToList(), null, lazyLoading, null);
        }
        public static ICollection<T> ToList<T>(this IEnumerable<Item> items, params string[] include)
        {
            return Convert<T>(items.ToList(), null, false, include);
        }
        public static ICollection<T> ToList<T>(this IEnumerable<Item> items, bool lazyLoading, params string[] include)
        {
            return Convert<T>(items.ToList(), null, lazyLoading, include);
        }
        #endregion

        #region WHERE ITEMS
        public static ICollection<T> Where<T>(this IEnumerable<Item> items, Expression<Func<T, bool>> query)
        {
            return Convert(items.ToList(), query, false, null);
        }
        public static ICollection<T> Where<T>(this IEnumerable<Item> items, Expression<Func<T, bool>> query, bool lazyLoading)
        {
            return Convert(items.ToList(), query, lazyLoading, null);
        }
        public static ICollection<T> Where<T>(this IEnumerable<Item> items, Expression<Func<T, bool>> query, params string[] include)
        {
            return Convert(items.ToList(), query, false, include);
        }
        #endregion



        public static T Parent<T>(this Item item, bool lazyLoading = false)
        {
            T parent = default(T);
            if (item != null && !item.ParentID.IsNull)
            {
                parent = item.Parent.ReflectTo<T>();
            }

            if (parent == null && lazyLoading)
            {
                item?.Database.GetItem(item.ParentID).ReflectTo<T>();
            }

            return parent;
        }

        public static List<T> Children<T>(this Item item, bool lazyLoading = false)
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

        public static T ReflectTo<T>(this Item item, bool lazyLoading = false, params string[] include)
        {

            if (item == null) return default(T);

            if (!item.IsOfType<T>()) return default(T);

            var o = (T)Activator.CreateInstance(typeof(T));

            var itemProp = GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Item);
            var idProp = GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Id);
            var nameProp = GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Name);
            var parentIdProp = GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.ParentId);
            var pathProp = GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Path);
            var templateIdProp = GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.TemplateId);
            var parentProp = GetTemplateSystemProperty<T>(SitecoreSystemPropertyType.Parent);

            idProp?.SetValue(o, item.ID.Guid);
            nameProp?.SetValue(o, item.Name);
            pathProp?.SetValue(o, item.Paths?.Path);
            parentIdProp?.SetValue(o, item?.ParentID?.Guid);
            templateIdProp?.SetValue(o, item?.TemplateID?.Guid);
            itemProp?.SetValue(o, item);

            if (item.Parent != null)
            {
                parentProp?.SetValue(o, item.Parent.ReflectTo<T>(lazyLoading));
            }

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
                    var lazyOrInclude = lazyLoading || (!lazyLoading && include != null && include.Contains(f.Name));
                    try
                    {
                        object value = field.Value;
                        try
                        {
                            if (f.PropertyType == typeof(bool))
                            {
                                f.SetValue(o, (string)value == "1");
                            }
                            else if (lazyOrInclude && f.PropertyType.IsGenericType &&
                                    typeof(List<>).IsAssignableFrom(f.PropertyType.GetGenericTypeDefinition()))
                            {

                                var multiField = (MultilistField)field;
                                var colgenType = f.PropertyType.GetGenericArguments().FirstOrDefault();
                                var subItems = multiField.GetItems();
                                if (subItems.Any())
                                {
                                    var result = typeof(LinqToSitecoreExtensions).GetMethod("ToList",
                                        new Type[] { typeof(Item[]), typeof(bool), typeof(string[]) })
                                        .MakeGenericMethod(colgenType)
                                        .Invoke(subItems, new object[] { subItems, lazyLoading, include });
                                    f.SetValue(o, result);
                                }
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
                            else if (field.TypeKey == "droplink" && lazyOrInclude)
                            {
                                var dropid = ID.Null;
                                if (ID.TryParse(field.Value, out dropid))
                                {
                                    //potential loop
                                    if (dropid != item.ID)
                                    {
                                        if (f.PropertyType != typeof(ID) && f.PropertyType != typeof(Guid) &&
                                            f.PropertyType != typeof(string))
                                        {
                                            var dropitem = item.Database.GetItem(dropid);
                                            var result = typeof(LinqToSitecoreExtensions)
                                                .GetMethod("ReflectTo")
                                                .MakeGenericMethod(f.PropertyType)
                                                .Invoke(dropitem, new object[] { dropitem, lazyLoading, include });
                                            f.SetValue(o, result);
                                        }
                                        else if (f.PropertyType == typeof(Guid))
                                        {
                                            f.SetValue(o, dropid.Guid);
                                        }
                                        else if (f.PropertyType == typeof(ID))
                                        {
                                            f.SetValue(o, dropid);
                                        }
                                    }
                                }

                            }
                            else if (field.TypeKey == "general link")
                            {
                                var linkField = (LinkField)field;
                                if (linkField != null)
                                {
                                    var url = linkField.GetFriendlyUrl();
                                    f.SetValue(o, url);
                                }

                            }
                            else if (field.TypeKey == "name value list")
                            {
                                var nmField = (NameValueListField)field;
                                if (f.PropertyType == typeof(NameValueCollection))
                                {
                                    f.SetValue(o, nmField.NameValues);
                                }
                                else if (f.PropertyType == typeof(Dictionary<string, string>))
                                {

                                    var dic = new Dictionary<string, string>();
                                    foreach (
                                        var nm in
                                        nmField.NameValues.AllKeys.SelectMany(nmField.NameValues.GetValues,
                                            (k, v) => new {key = k, value = v}))
                                    {
                                        if (!dic.ContainsKey(nm.key))
                                        {
                                            dic.Add(nm.key, HttpUtility.UrlDecode(nm.value));
                                        }
                                    }
                                    f.SetValue(o, dic);
                                }

                            }
                            else
                            {
                                if (field.TypeKey == "file" || field.TypeKey == "image")
                                {
                                    var fitem = (FileField)field;
                                    f.SetValue(o, fitem.Src);
                                }
                                else if (f.PropertyType.IsEnum)
                                {
                                    var intValue = -1;
                                    if (int.TryParse(value.ToString(), out intValue))
                                    {
                                        f.SetValue(o, intValue);
                                    }
                                    else
                                    {
                                        var enuValue = Enum.Parse(f.PropertyType, value.ToString().Replace(" ", string.Empty), true);
                                        f.SetValue(o, enuValue);
                                    }
                                }
                                else
                                {

                                    var convertedObj = System.Convert.ChangeType(value, f.PropertyType);
                                    f.SetValue(o, convertedObj);
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
