using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNEToolkit.Extentions;

namespace DotNEToolkit.PhoneReader
{
    public class JuheAPIClient : IPhoneAPIClient
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("JuheAPIClient");

        private const string Key = "c3a09ff26c6f370c526a20506c3d02fb";

        private const string URI = "http://apis.juhe.cn/mobile/get?phone={0}&key={1}";

        public PhoneData Query(string number, PhoneData data, Dictionary<PhoneData, string> values)
        {
            PhoneData left = data;

            string uri = string.Format(URI, number, Key);

            byte[] bytes = HttpClient.GetData(uri);
            string json = Encoding.UTF8.GetString(bytes);
            JToken token = JsonConvert.DeserializeObject(json) as JToken;

            string resultcode = token["resultcode"].ToString();
            if (resultcode != "200")
            {
                string reason = token["reason"].ToString();
                logger.ErrorFormat("调用聚合API失败, resultcode = {0}, reason = {1}", resultcode, reason);
                return left;
            }

            if (data.HasFlag(PhoneData.City))
            {
                values[PhoneData.City] = token["result"]["city"].ToString();
                left &= ~PhoneData.City;
            }

            if (data.HasFlag(PhoneData.CurrentISP))
            {
                values[PhoneData.CurrentISP] = token["result"]["company"].ToString();
                left &= ~PhoneData.CurrentISP;
            }

            return left;
        }
    }
}
