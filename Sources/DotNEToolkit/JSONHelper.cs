using DotNEToolkit.Utility;
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

        /// <summary>
        /// 把一个JSON字符串转换成一个JSON对象
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="json"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TResult Parse<TResult>(string json, TResult defaultValue)
        {
            if (string.IsNullOrEmpty(json))
            {
                return defaultValue;
            }

            try
            {
                return JsonConvert.DeserializeObject<TResult>(json);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("JSON字符串转对象异常, {0}", json), ex);
                return defaultValue;
            }
        }

        public static TResult Parse<TResult>(string json)
        {
            return JsonConvert.DeserializeObject<TResult>(json);
        }

        /// <summary>
        /// 把一个json文件序列化成C#对象
        /// 该函数不会对异常做处理，异常由调用者截获并处理
        /// 如果该函数对异常做处理，那么调用者可能不知道出现异常的详细原因
        /// </summary>
        /// <typeparam name="TResult">要序列化成的对象类型</typeparam>
        /// <param name="filePath">要序列化的json文件的路径</param>
        /// <returns>如果序列化失败，那么返回空</returns>
        public static TResult ParseFile<TResult>(string filePath) where TResult : class
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<TResult>(json);
        }

        /// <summary>
        /// 把一个文件序列化成JSON对象
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TResult ParseFile<TResult>(string filePath, TResult defaultValue) where TResult : class
        {
            if (!File.Exists(filePath))
            {
                return defaultValue;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<TResult>(json);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("反序列化JSON文件异常, path = {0}", filePath), ex);
                return defaultValue;
            }
        }

        /// <summary>
        /// 把一个文件序列化成JSON对象
        /// 如果解析失败, 不会抛异常, 会返回defaultObject
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="defaultObject"></param>
        /// <returns></returns>
        public static TObject File2Object<TObject>(string filePath, TObject defaultObject) where TObject : class
        {
            if (!File.Exists(filePath))
            {
                logger.ErrorFormat("反序列化JSON文件失败, 文件不存在, {0}", filePath);
                return defaultObject;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<TObject>(json);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("反序列化JSON文件异常, path = {0}", filePath), ex);
                return defaultObject;
            }
        }

        /// <summary>
        /// 把一个json文件序列化成C#对象
        /// 该函数不会对异常做处理，异常由调用者截获并处理
        /// 如果该函数对异常做处理，那么调用者可能不知道出现异常的详细原因
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static TObject File2Object<TObject>(string filePath) where TObject : class
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<TObject>(json);
        }

        /// <summary>
        /// 把一个C#对象序列化成JSON格式并写入到一个文件里
        /// 该函数不会返回错误码，由调用者去截取异常并处理
        /// </summary>
        /// <typeparam name="TObject">要写入的对象类型</typeparam>
        /// <param name="filePath">要保存的文件路径</param>
        /// <param name="obj">要写入的对象实例</param>
        public static void Object2File<TObject>(string filePath, TObject obj) where TObject : class
        {
            string jsonText = JsonConvert.SerializeObject(obj);
            File.WriteAllText(filePath, jsonText);
        }

        /// <summary>
        /// 克隆一个对象
        /// 先把inputObject转换成JSON字符串，再把JSON字符串转成TOutput类型的对象
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="inputObject"></param>
        /// <returns></returns>
        public static TObject CloneObject<TObject>(TObject inputObject)
        {
            string json = JsonConvert.SerializeObject(inputObject);
            return JsonConvert.DeserializeObject<TObject>(json);
        }

        public static TOutput ConvertObject<TInput, TOutput>(TInput inputObject)
        {
            string json = JsonConvert.SerializeObject(inputObject);
            return JsonConvert.DeserializeObject<TOutput>(json);
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
            List<T> list = JSONHelper.ParseFile<List<T>>(jsonFile, new List<T>());
            list.Add(item);
            JSONHelper.Object2File<List<T>>(jsonFile, list);
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
            List<TSource> toDelete = list.Where(predicate).ToList();
            if (toDelete == null || toDelete.Count == 0)
            {
                return;
            }

            foreach (TSource source in toDelete)
            {
                list.Remove(source);
            }

            JSONHelper.Object2File<List<TSource>>(jsonFile, list);
        }

        public static int Update<TSource>(string jsonFile, Func<TSource, bool> predicate, TSource item)
        {
            List<TSource> list = JSONHelper.ParseFile<List<TSource>>(jsonFile, new List<TSource>());

            TSource exist = list.FirstOrDefault(predicate);
            if (exist == null)
            {
                return DotNETCode.FAILED;
            }

            int index = list.IndexOf(exist);
            list.Remove(exist);
            list.Insert(index, item);

            JSONHelper.Object2File<List<TSource>>(jsonFile, list);
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
            JSONHelper.Object2File<List<TSource>>(jsonFile, new List<TSource>(items));
        }

        public static bool Exist<TSource>(string jsonFile, Func<TSource, bool> predicate)
        {
            return SelectAll<TSource>(jsonFile, predicate).Count > 0;
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
            JSONDBNameAttribute dbName = ReflectionUtils.GetClassAttribute<JSONDBNameAttribute, T>();
            string fileName = dbName == null ? string.Format("{0}.json", typeof(T).Name) : dbName.Name;
            return System.IO.Path.Combine(this.baseDir, fileName);
        }

        public void Insert<T>(T obj)
        {
            JSONDatabase.Insert<T>(GenerateDbFile<T>(), obj);
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