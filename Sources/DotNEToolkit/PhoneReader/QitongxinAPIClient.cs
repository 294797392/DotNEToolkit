using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.PhoneReader
{
    public class QitongxinAPIClient : IPhoneAPIClient
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("QitongxinAPIClient");

        private const string URI = "http://xhzw.market.alicloudapi.com/isp?mobile={0}";

        private const string APPCode = "";

        public PhoneData Query(string number, PhoneData data, Dictionary<PhoneData, string> values)
        {
            PhoneData left = data;

            string uri = string.Format(URI, number);

            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
            headers.Add(new KeyValuePair<string, string>("Authorization", string.Format("APPCODE {0}", APPCode)));

            byte[] bytes = HttpUtility.Post(uri, headers);
            string json = Encoding.UTF8.GetString(bytes);

            JToken token = JsonConvert.DeserializeObject(json) as JToken;
            int code = token["code"].Value<int>();
            if (code != 0)
            {
                string reason = token["reason"].ToString();
                logger.ErrorFormat("访问阿里云服务失败, code = {0}, reason = {1}", code, reason);
                return left;
            }

            if (data.HasFlag(PhoneData.City))
            {
                values[PhoneData.City] = token["result"]["Area"].ToString();
                left &= ~PhoneData.City;
            }

            if (data.HasFlag(PhoneData.CurrentISP))
            {
                values[PhoneData.CurrentISP] = token["result"]["Now_isp"].ToString();
                left &= ~PhoneData.CurrentISP;
            }

            if (data.HasFlag(PhoneData.IsVirtual))
            {
                string res = token["result"]["res"].ToString();
                if (res == "0")
                {
                    // 没转网
                    values[PhoneData.IsVirtual] = "false";
                }
                else if (res == "3" || res == "4")
                {
                    values[PhoneData.IsVirtual] = "true";
                }
                else if (res == "1")
                {
                    values[PhoneData.IsVirtual] = "false";
                }

                left &= ~PhoneData.IsVirtual;
            }

            if (data.HasFlag(PhoneData.OrignalISP) || data.HasFlag(PhoneData.Transfered))
            {
                string res = token["result"]["res"].ToString();
                if (res == "1")
                {
                    // 转网后才有原始运营商
                    values[PhoneData.OrignalISP] = token["result"]["Init_isp"].ToString();
                    values[PhoneData.Transfered] = "true";
                }
                else
                {
                    values[PhoneData.OrignalISP] = token["result"]["Init_isp"].ToString();
                    values[PhoneData.Transfered] = "false";
                }
                left &= ~PhoneData.OrignalISP;
                left &= ~PhoneData.Transfered;
            }

            return left;
        }
    }
}
