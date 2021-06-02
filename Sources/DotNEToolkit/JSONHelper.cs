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

        public static TResult Parse<TResult>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default(TResult);
            }

            try
            {
                return JsonConvert.DeserializeObject<TResult>(json);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("解析JSON异常, json = {0}", json), ex);
                return default(TResult);
            }
        }

        public static bool TryParse<TResult>(string json, out TResult result)
        {
            result = Parse<TResult>(json);
            return result != null;
        }

        public static TResult ParseFile<TResult>(string path)
        {
            if (!File.Exists(path))
            {
                return default(TResult);
            }

            string json = string.Empty;

            try
            {
                json = File.ReadAllText(path);

                return JsonConvert.DeserializeObject<TResult>(json);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("解析JSON异常, json = {0}", json), ex);
                return default(TResult);
            }
        }

        public static bool TryParseFile<TResult>(string path, out TResult result)
        {
            result = ParseFile<TResult>(path);
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

        public static bool DeserializeJSONFile<T>(string filePath, JsonSerializerSettings settings, out T obj)
        {
            obj = default(T);
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                string content = File.ReadAllText(filePath);
                if (settings == null)
                {
                    settings = new JsonSerializerSettings();
                }
                obj = JsonConvert.DeserializeObject<T>(content, settings);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("DeserializeJSONFile异常, {0}", filePath), ex);
                return false;
            }
            return true;
        }

        public static bool DeserializeJSONFile<T>(string filePath, out T obj)
        {
            return DeserializeJSONFile<T>(filePath, null, out obj);
        }

        /// <summary>
        /// 把JSON文件转换成一个对象
        /// 如果转换失败，那么返回deafultValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T DeserializeJSONFile<T>(string filePath, T defaultValue)
        {
            T result;
            return JSONHelper.DeserializeJSONFile<T>(filePath, out result) ? result : defaultValue;
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
}