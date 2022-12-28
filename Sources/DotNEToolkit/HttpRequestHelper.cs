using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 提供发送HTTP请求的函数
    /// </summary>
    public class HttpRequestHelper
    {
        public const string METHOD_POST = "POST";
        public const string METHOD_GET = "GET";

        public const string TYPE_XML = "text/xml";
        public const string TYPE_URLENCODE = "application/x-www-form-urlencoded";

        private const int BufferLen = 1024;

        public static HttpWebRequest GetHttpRequest(string url, string method,
            IEnumerable<KeyValuePair<string, string>> headers = null, CookieContainer cookieContainer = null, int? timeout = null)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = method;

            if (timeout.HasValue)
            {
                httpRequest.Timeout = timeout.Value;
            }

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> pair in headers)
                {
                    httpRequest.Headers.Add(pair.Key, pair.Value);
                }
            }

            return httpRequest;
        }

        public static HttpWebRequest GetHttpRequest(string url, string contentType, string method,
            IEnumerable<KeyValuePair<string, string>> headers = null, CookieContainer cookieContainer = null, int? timeout = null)
        {
            HttpWebRequest httpRequest = GetHttpRequest(url, method, headers, cookieContainer, timeout);
            httpRequest.ContentType = contentType;
            return httpRequest;
        }

        #region Post方法

        public static byte[] PostData(string url, byte[] content, string contentType, out HttpStatusCode statusCode,
            IEnumerable<KeyValuePair<string, string>> headers = null, CookieContainer cookieContainer = null, int? timeout = null)
        {
            HttpWebRequest httpRequest = GetHttpRequest(url, contentType, METHOD_POST, headers, cookieContainer, timeout);
            if (content != null && content.Length > 0)
            {
                Stream stream = httpRequest.GetRequestStream();
                stream.Write(content, 0, content.Length);
                stream.Close();
            }
            else
            {
                httpRequest.ContentLength = 0;
            }

            return SendRequest(httpRequest, out statusCode);
        }

        public static string PostDataEx(string url, byte[] content, string contentType,
            IEnumerable<KeyValuePair<string, string>> headers = null, CookieContainer cookieContainer = null, int? timeout = null)
        {
            HttpWebRequest httpRequest = GetHttpRequest(url, contentType, METHOD_POST, headers, cookieContainer, timeout);
            if (content != null && content.Length > 0)
            {
                Stream stream = httpRequest.GetRequestStream();
                stream.Write(content, 0, content.Length);
                stream.Close();
            }
            else
            {
                httpRequest.ContentLength = 0;
            }

            return SendRequestEx(httpRequest);
        }

        public static string PostObject<T>(string url, T toPost, IEnumerable<KeyValuePair<string, string>> headers = null,
            CookieContainer cookieContainer = null, int? timeout = null)
        {
            HttpWebRequest httpRequest = GetHttpRequest(url, TYPE_XML, METHOD_POST, headers, cookieContainer, timeout);
            Stream stream = httpRequest.GetRequestStream();
            XmlHelper.ToXmlStream<T>(toPost, stream);

            return SendRequestEx(httpRequest);
        }

        public static T2 PostObjectEx<T1, T2>(string url, T1 toPost, IEnumerable<KeyValuePair<string, string>> headers = null,
            CookieContainer cookieContainer = null, int? timeout = null)
        {
            HttpWebRequest httpRequest = GetHttpRequest(url, TYPE_XML, METHOD_POST, headers, cookieContainer, timeout);
            Stream stream = httpRequest.GetRequestStream();
            XmlHelper.ToXmlStream<T1>(toPost, stream);

            return SendRequest<T2>(httpRequest);
        }

        public static byte[] Post(string url, IEnumerable<KeyValuePair<string, string>> headers = null,
            CookieContainer cookieContainer = null, int? timeout = null)
        {
            HttpWebRequest request = GetHttpRequest(url, METHOD_POST, headers, cookieContainer, timeout);
            return SendRequest(request);
        }

        #endregion

        #region Get方法

        public static byte[] GetData(string url, string contentType, IEnumerable<KeyValuePair<string, string>> headers = null,
    CookieContainer cookieContainer = null, int? timeout = null)
        {
            HttpWebRequest request = GetHttpRequest(url, contentType, METHOD_GET, headers, cookieContainer, timeout);
            return SendRequest(request);
        }

        public static byte[] GetData(string url, IEnumerable<KeyValuePair<string, string>> headers = null,
            CookieContainer cookieContainer = null, int? timeout = null)
        {
            HttpWebRequest request = GetHttpRequest(url, METHOD_GET, headers, cookieContainer, timeout);
            return SendRequest(request);
        }

        public static string GetDataEx(string url, IEnumerable<KeyValuePair<string, string>> headers = null,
            CookieContainer cookieContainer = null, int? timeout = null)
        {
            HttpWebRequest request = GetHttpRequest(url, METHOD_GET, headers, cookieContainer, timeout);
            return SendRequestEx(request);
        }

        public static T GetData<T>(string url, IEnumerable<KeyValuePair<string, string>> headers = null,
            CookieContainer cookieContainer = null, int? timeout = null)
        {
            HttpWebRequest request = GetHttpRequest(url, METHOD_GET, headers, cookieContainer, timeout);
            return SendRequest<T>(request);
        }

        #endregion

        #region 私有方法

        private static byte[] ReadFull(Stream stream, int size, out int readed)
        {
            readed = 0;

            byte[] result = new byte[size];

            int left = (int)size;
            int offset = 0;

            while (left > 0)
            {
                int readLen = left > 1024 ? BufferLen : left;
                int actual = stream.Read(result, offset, readLen);
                readed += actual;
                if (actual == 0)
                {
                    break;
                }

                offset += actual;
                left = result.Length - offset;
            }

            return result;
        }

        private static byte[] SendRequest(HttpWebRequest request)
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.Headers[HttpResponseHeader.TransferEncoding] == "chunked")
                {
                    // 读取chunked数据

                    List<byte> result = new List<byte>();

                    using (Stream responseStream = response.GetResponseStream())
                    {
                        while (true)
                        {
                            int readed;
                            byte[] buffer = ReadFull(responseStream, BufferLen, out readed);
                            if (readed == 0)
                            {
                                return result.ToArray();
                            }
                            else if (readed < BufferLen)
                            {
                                byte[] full = new byte[readed];
                                Array.Copy(buffer, full, readed);
                                result.AddRange(full);
                                return result.ToArray();
                            }
                            else
                            {
                                result.AddRange(buffer);
                            }
                        }
                    }
                }
                else
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        int readed;
                        return ReadFull(responseStream, (int)response.ContentLength, out readed);
                    }
                }
            }
        }

        private static byte[] SendRequest(HttpWebRequest request, out HttpStatusCode statusCode)
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                statusCode = response.StatusCode;

                if (response.Headers[HttpResponseHeader.TransferEncoding] == "chunked")
                {
                    // 读取chunked数据

                    List<byte> result = new List<byte>();

                    using (Stream responseStream = response.GetResponseStream())
                    {
                        while (true)
                        {
                            int readed;
                            byte[] buffer = ReadFull(responseStream, BufferLen, out readed);
                            if (readed == 0)
                            {
                                return result.ToArray();
                            }
                            else if (readed < BufferLen)
                            {
                                byte[] full = new byte[readed];
                                Array.Copy(buffer, full, readed);
                                result.AddRange(full);
                                return result.ToArray();
                            }
                            else
                            {
                                result.AddRange(buffer);
                            }
                        }
                    }
                }
                else
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        int readed;
                        return ReadFull(responseStream, (int)response.ContentLength, out readed);
                    }
                }
            }
        }

        private static string SendRequestEx(HttpWebRequest request)
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader responseStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return responseStream.ReadToEnd();
                }
            }
        }

        private static T SendRequest<T>(HttpWebRequest request)
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    return XmlHelper.ParseFromStream<T>(responseStream);
                }
            }
        }

        #endregion
    }
}
