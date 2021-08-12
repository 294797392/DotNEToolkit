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
                result.AddRange(JSONHelper.ParseFile<List<T>>(filePath));
            }

            return result;
        }

        public static IEnumerable<T> ParseFiles<T>(IEnumerable<string> filesPaths)
        {
            List<T> result = new List<T>();

            foreach (string filePath in filesPaths)
            {
                result.AddRange(JSONHelper.ParseFile<List<T>>(filePath));
            }

            return result;
        }

        public static TResult Parse<TResult>(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                return default(TResult);
            }

            try
            {
                return JsonConvert.DeserializeObject<TResult>(jsonText);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("解析JSON异常, json = {0}", jsonText), ex);
                return default(TResult);
            }
        }

        public static bool TryParse<TResult>(string jsonText, out TResult result)
        {
            result = Parse<TResult>(jsonText);
            return result != null;
        }

        /// <summary>
        /// 把一个json文件序列化成C#对象
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="jsonFile"></param>
        /// <returns>如果序列化失败，那么返回空</returns>
        public static TResult ParseFile<TResult>(string jsonFile)
        {
            if (!File.Exists(jsonFile))
            {
                return default(TResult);
            }

            string json = string.Empty;

            try
            {
                json = File.ReadAllText(jsonFile);

                return JsonConvert.DeserializeObject<TResult>(json);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("解析JSON异常, json = {0}", json), ex);
                return default(TResult);
            }
        }

        public static bool TryParseFile<TResult>(string jsonFile, out TResult result)
        {
            result = ParseFile<TResult>(jsonFile);
            return result != null;
        }




        public static IEnumerable<T> DeserializeJSONFile<T>(string searchDir, string pattern)
        {
            List<T> result = new List<T>();

            IEnumerable<string> expressionFiles = Directory.EnumerateFiles(searchDir, pattern, SearchOption.AllDirectories);

            foreach (string filePath in expressionFiles)
            {
                List<T> definitions;
                if (JSONHelper.DeserializeJSONFile<List<T>>(filePath, out definitions))
                {
                    result.AddRange(definitions);
                }
            }

            return result;
        }

        public static bool DeserializeJSONFile<T>(string jsonFile, JsonSerializerSettings settings, out T obj)
        {
            obj = default(T);
            if (string.IsNullOrEmpty(jsonFile))
            {
                return false;
            }

            if (!File.Exists(jsonFile))
            {
                return false;
            }

            try
            {
                string content = File.ReadAllText(jsonFile);
                if (settings == null)
                {
                    settings = new JsonSerializerSettings();
                }
                obj = JsonConvert.DeserializeObject<T>(content, settings);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("DeserializeJSONFile异常, {0}", jsonFile), ex);
                return false;
            }
            return true;
        }

        public static bool DeserializeJSONFile<T>(string jsonFile, out T obj)
        {
            return DeserializeJSONFile<T>(jsonFile, null, out obj);
        }

        /// <summary>
        /// 把JSON文件转换成一个对象
        /// 如果转换失败，那么返回deafultValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonFile"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T DeserializeJSONFile<T>(string jsonFile, T defaultValue)
        {
            T result;
            return JSONHelper.DeserializeJSONFile<T>(jsonFile, out result) ? result : defaultValue;
        }


        public static int Write<TSource>(string jsonFile, TSource obj) 
        {
            string jsonText = JsonConvert.SerializeObject(obj);

            try
            {
                File.WriteAllText(jsonFile, jsonText);

                return DotNETCode.SUCCESS;
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error("保存JSON数据异常", ex);
                return DotNETCode.FILE_PERMISSION_ERROR;
            }
            catch (Exception ex)
            {
                logger.Error("保存JSON数据异常", ex);
                return DotNETCode.FILE_WRITE_FAILED;
            }
        }

        public static List<T> JArray2List<T>(this IDictionary toConvert, string key)
        {
            if (!toConvert.Contains(key))
            {
                return null;
            }

            if (!(toConvert[key] is JArray))
            {
                return null;
            }

            JArray jArray = toConvert[key] as JArray;
            return jArray.ToObject<List<T>>();
        }
    }

    /// <summary>
    /// 把一个json文件当成数据库去用
    /// </summary>
    public static class JSONDatabase
    {
        /// <summary>
        /// 向一个集合里插入一条数据
        /// </summary>
        /// <typeparam name="T">集合里的类型</typeparam>
        /// <param name="jsonFile">json文件路径</param>
        /// <param name="obj">要插入的元素的实例</param>
        /// <returns></returns>
        public static int Insert<T>(string jsonFile, T obj)
        {
            if (obj == null)
            {
                return DotNETCode.INVALID_PARAMS;
            }

            List<T> list;
            if (!File.Exists(jsonFile))
            {
                list = new List<T>();
            }
            else 
            {
                if (!JSONHelper.DeserializeJSONFile<List<T>>(jsonFile, out list))
                {
                    // 无效的JSON格式
                    return DotNETCode.JSON_INVALID_FORMAT;
                }
            }

            list.Add(obj);

            return JSONHelper.Write<List<T>>(jsonFile, list);
        }

        public static int Select<TSource>(string jsonFile, Func<TSource, bool> predicate, out TSource item)
        {
            item = default(TSource);

            List<TSource> list;
            if (!JSONHelper.DeserializeJSONFile<List<TSource>>(jsonFile, out list))
            {
                // 无效的JSON格式
                return DotNETCode.JSON_INVALID_FORMAT;
            }

            item = list.FirstOrDefault(predicate);

            return item == null ? DotNETCode.FAILED : DotNETCode.SUCCESS;
        }

        public static List<TSource> SelectAll<TSource>(string jsonFile)
        {
            List<TSource> list;
            if (!JSONHelper.DeserializeJSONFile<List<TSource>>(jsonFile, out list))
            {
                // 无效的JSON格式
                return null;
            }

            return list;
        }

        public static int Delete<TSource>(string jsonFile, Func<TSource, bool> predicate)
        {
            List<TSource> list;
            if (!JSONHelper.DeserializeJSONFile<List<TSource>>(jsonFile, out list))
            {
                // 无效的JSON格式
                return DotNETCode.JSON_INVALID_FORMAT;
            }

            TSource toDelete = list.FirstOrDefault(predicate);
            if (toDelete != null)
            {
                list.Remove(toDelete);
            }

            return JSONHelper.Write<List<TSource>>(jsonFile, list);
        }

        public static int Update<TSource>(string jsonFile, Func<TSource, bool> predicate, TSource item)
        {
            List<TSource> list;
            if (!JSONHelper.DeserializeJSONFile<List<TSource>>(jsonFile, out list))
            {
                // 无效的JSON格式
                return DotNETCode.JSON_INVALID_FORMAT;
            }

            TSource exist = list.FirstOrDefault(predicate);
            if (exist == null)
            {
                return DotNETCode.FAILED;
            }

            int index = list.IndexOf(exist);
            list.Remove(exist);
            list.Insert(index, item);
            return DotNETCode.SUCCESS;
        }

        /// <summary>
        /// 保存所有对象，会覆盖JSON文件
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="jsonFile"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static int SaveAll<TSource>(string jsonFile, IEnumerable<TSource> items)
        {
            return JSONHelper.Write<List<TSource>>(jsonFile, new List<TSource>(items));
        }
    }
}