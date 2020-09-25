using Newtonsoft.Json;
using System;
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
    }
}