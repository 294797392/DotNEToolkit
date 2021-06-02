using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.PhoneReader
{
    public class AlibabaAPIClient : IPhoneAPIClient
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("AlibabaAPIClient");

        private const string URI = "http://sms04.market.alicloudapi.com/api/phone/transfer/arr?phone={0}";

        private const string APPCode = "";

        public PhoneData Query(string number, PhoneData data, Dictionary<PhoneData, string> values)
        {
            PhoneData left = data;

            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
            headers.Add(new KeyValuePair<string, string>("Authorization", string.Format("APPCODE {0}", APPCode)));
            headers.Add(new KeyValuePair<string, string>("X-Ca-Nonce", Guid.NewGuid().ToString()));

            string uri = string.Format(URI, number);

            byte[] bytes = HttpUtility.GetData(uri, headers);
            string json = Encoding.UTF8.GetString(bytes);

            JToken token = JsonConvert.DeserializeObject(json) as JToken;
            bool success = token["success"].Value<bool>();
            int code = token["code"].Value<int>();
            if (!success || code != 0)
            {
                logger.ErrorFormat("访问阿里云服务失败, code = {0}, success = {1}", code, success);
                return left;
            }

            if (data.HasFlag(PhoneData.CurrentISP))
            {
                JToken[] tokens = token["data"]["data"].ToArray();
                if (tokens.Length == 0)
                {
                    logger.InfoFormat("阿里云返回空手机号信息");
                    return left;
                }

                string state = tokens[0]["state"].Value<string>();
                if (state == "4")
                {
                    // 该号码没有转网记录
                    values[PhoneData.CurrentISP] = string.Empty;
                }
                else
                {
                    if (state == "1")
                    {
                        values[PhoneData.CurrentISP] = "移动";
                    }
                    else if (state == "2")
                    {
                        values[PhoneData.CurrentISP] = "联通";
                    }
                    else if (state == "3")
                    {
                        values[PhoneData.CurrentISP] = "电信";
                    }
                }
                left &= ~PhoneData.CurrentISP;
            }

            return left;
        }
    }
}
