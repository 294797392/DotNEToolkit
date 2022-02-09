using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    /// <summary>
    /// JSON函数
    /// </summary>
    public static class JSONHelper
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("JSONHelper");

        /// <summary>
        /// 把一个目录里所有的符合pattern的文件序列化成一个List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchDir"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static IEnumerable<T> ParseDirectory<T>(string searchDir, string pattern)
        {
            List<T> result = new List<T>();

            IEnumerable<string> expressionFiles = Directory.EnumerateFiles(searchDir, pattern, SearchOption.AllDirectories);

            foreach (string filePath in expressionFiles)
            {
                result.AddRange(JSONHelper.ParseFile<List<T>>(filePath, new List<T>()));
            }

            return result;
        }

        public static TResult Parse<TResult>(string jsonText, TResult defaultValue)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                return defaultValue;
            }

            try
            {
                return JsonConvert.DeserializeObject<TResult>(jsonText);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("解析JSON异常, json = {0}", jsonText), ex);
                return defaultValue;
            }
        }

        /// <summary>
        /// 把一个json文件序列化成C#对象
        /// 该函数不会对异常做处理，异常由调用者截获并处理
        /// 如果该函数对异常做处理，那么调用者可能不知道出现异常的详细原因
        /// </summary>
        /// <typeparam name="TResult">要序列化成的对象类型</typeparam>
        /// <param name="jsonFile">要序列化的json文件的路径</param>
        /// <returns>如果序列化失败，那么返回空</returns>
        public static TResult ParseFile<TResult>(string jsonFile)
        {
            string json = File.ReadAllText(jsonFile);
            return JsonConvert.DeserializeObject<TResult>(json);
        }

        public static TResult ParseFile<TResult>(string jsonFile, TResult defaultValue)
        {
            if (!File.Exists(jsonFile))
            {
                return defaultValue;
            }

            try
            {
                string json = File.ReadAllText(jsonFile);
                return JsonConvert.DeserializeObject<TResult>(json);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("解析JSON文件异常, path = {0}", jsonFile), ex);
                return defaultValue;
            }
        }

        /// <summary>
        /// 把一个C#对象序列化成JSON格式并写入到一个文件里
        /// 该函数不会返回错误码，由调用者去截取异常并处理
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="jsonFile"></param>
        /// <param name="obj"></param>
        public static void WriteFile<TSource>(string jsonFile, TSource obj)
        {
            string jsonText = JsonConvert.SerializeObject(obj);
            File.WriteAllText(jsonFile, jsonText);
        }

        /// <summary>
        /// 从一个字典里获取一个JSON格式的对象
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue GetValue<TValue>(this IDictionary parameters, string key, TValue defaultValue)
        {
            if (!parameters.Contains(key))
            {
                return defaultValue;
            }

            string json = parameters[key].ToString();
            return JsonConvert.DeserializeObject<TValue>(json);
        }
    }

    /// <summary>
    /// 把一个json文件当成数据库去用
    /// </summary>
    public static class JSONDatabase
    {
        private static string GenerateDbFile<T>()
        {
            return string.Format("{0}.json", typeof(T).Name.ToLower());
        }

        public static void Insert<T>(T obj)
        {
            Insert<T>(GenerateDbFile<T>(), obj);
        }

        public static TSource Select<TSource>(Func<TSource, bool> predicate)
        {
            return Select<TSource>(GenerateDbFile<TSource>(), predicate);
        }

        public static List<TSource> SelectAll<TSource>()
        {
            return SelectAll<TSource>(GenerateDbFile<TSource>());
        }

        public static List<TSource> SelectAll<TSource>(Func<TSource, bool> predicate)
        {
            return SelectAll<TSource>(GenerateDbFile<TSource>(), predicate);
        }

        public static void Delete<TSource>(Func<TSource, bool> predicate)
        {
            Delete<TSource>(GenerateDbFile<TSource>(), predicate);
        }

        public static int Update<TSource>(Func<TSource, bool> predicate, TSource item)
        {
            return Update<TSource>(GenerateDbFile<TSource>(), predicate, item);
        }

        public static void SaveAll<TSource>(IEnumerable<TSource> items)
        {
            SaveAll<TSource>(GenerateDbFile<TSource>(), items);
        }

        public static bool Exist<TSource>(Func<TSource, bool> predicate)
        {
            return Exist<TSource>(GenerateDbFile<TSource>(), predicate);
        }


        /// <summary>
        /// 向一个集合里插入一条数据
        /// </summary>
        /// <typeparam name="T">集合里的类型</typeparam>
        /// <param name="jsonFile">json文件路径</param>
        /// <param name="item">要插入的元素的实例</param>
        /// <returns></returns>
        public static void Insert<T>(string jsonFile, T item)
        {
            List<T> list = JSONHelper.Parse<List<T>>(jsonFile, new List<T>());
            list.Add(item);
            JSONHelper.WriteFile<List<T>>(jsonFile, list);
        }

        public static TSource Select<TSource>(string jsonFile, Func<TSource, bool> predicate)
        {
            List<TSource> list = JSONHelper.ParseFile<List<TSource>>(jsonFile, new List<TSource>());
            return list.FirstOrDefault(predicate);
        }

        public static List<TSource> SelectAll<TSource>(string jsonFile)
        {
            return JSONHelper.ParseFile<List<TSource>>(jsonFile, new List<TSource>());
        }

        public static List<TSource> SelectAll<TSource>(string jsonFile, Func<TSource, bool> predicate)
        {
            return SelectAll<TSource>(jsonFile).Where(predicate).ToList();
        }

        public static void Delete<TSource>(string jsonFile, Func<TSource, bool> predicate)
        {
            List<TSource> list = JSONHelper.ParseFile<List<TSource>>(jsonFile, new List<TSource>());
            TSource toDelete = list.FirstOrDefault(predicate);
            if (toDelete != null)
            {
                list.Remove(toDelete);
            }

            JSONHelper.WriteFile<List<TSource>>(jsonFile, list);
        }

        public static int Update<TSource>(string jsonFile, Func<TSource, bool> predicate, TSource item)
        {
            List<TSource> list = JSONHelper.Parse<List<TSource>>(jsonFile, new List<TSource>());

            TSource exist = list.FirstOrDefault(predicate);
            if (exist == null)
            {
                return DotNETCode.FAILED;
            }

            int index = list.IndexOf(exist);
            list.Remove(exist);
            list.Insert(index, item);

            JSONHelper.WriteFile<List<TSource>>(jsonFile, list);
            return DotNETCode.SUCCESS;
        }

        /// <summary>
        /// 保存所有对象，会覆盖JSON文件
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="jsonFile"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static void SaveAll<TSource>(string jsonFile, IEnumerable<TSource> items)
        {
            JSONHelper.WriteFile<List<TSource>>(jsonFile, new List<TSource>(items));
        }

        public static bool Exist<TSource>(string jsonFile, Func<TSource, bool> predicate)
        {
            TSource exist = Select<TSource>(jsonFile, predicate);
            return exist != null;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class JSONDBNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public JSONDBNameAttribute(string name)
        {
            this.Name = name;
        }
    }

    public class JSONDatabaseInstance
    {
        private string baseDir;

        private JSONDatabaseInstance()
        { }

        public static JSONDatabaseInstance Create(string baseDir)
        {
            JSONDatabaseInstance database = new JSONDatabaseInstance()
            {
                baseDir = baseDir
            };
            return database;
        }

        private string GenerateDbFile<T>()
        {
            JSONDBNameAttribute dbName = Reflections.GetClassAttribute<JSONDBNameAttribute, T>();
            string fileName = dbName == null ? string.Format("{0}.json", typeof(T).Name) : dbName.Name;
            return System.IO.Path.Combine(this.baseDir, fileName);
        }

        public void Insert<T>(T obj)
        {
            JSONDatabase.Insert<T>(GenerateDbFile<T>(), obj);
        }

        public TSource Select<TSource>(Func<TSource, bool> predicate)
        {
            return JSONDatabase.Select<TSource>(GenerateDbFile<TSource>(), predicate);
        }

        public List<TSource> SelectAll<TSource>()
        {
            return JSONDatabase.SelectAll<TSource>(GenerateDbFile<TSource>());
        }

        public List<TSource> SelectAll<TSource>(Func<TSource, bool> predicate)
        {
            return JSONDatabase.SelectAll<TSource>(GenerateDbFile<TSource>(), predicate);
        }

        public void Delete<TSource>(Func<TSource, bool> predicate)
        {
            JSONDatabase.Delete<TSource>(GenerateDbFile<TSource>(), predicate);
        }

        public int Update<TSource>(Func<TSource, bool> predicate, TSource item)
        {
            return JSONDatabase.Update<TSource>(GenerateDbFile<TSource>(), predicate, item);
        }

        public void SaveAll<TSource>(IEnumerable<TSource> items)
        {
            JSONDatabase.SaveAll<TSource>(GenerateDbFile<TSource>(), items);
        }

        public bool Exist<TSource>(Func<TSource, bool> predicate)
        {
            return JSONDatabase.Exist<TSource>(GenerateDbFile<TSource>(), predicate);
        }
    }
}