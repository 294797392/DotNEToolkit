using DotNEToolkit.Extentions;
using DotNEToolkit.PhoneReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.PhoneReaders
{
    public class PhoneReader
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger("PhoneReader");

        private List<IPhoneAPIClient> phoneClients;

        public PhoneReader()
        {
            this.phoneClients = new List<IPhoneAPIClient>()
            {
                new QitongxinAPIClient(),
                //new JuheAPIClient(),
                //new AlibabaAPIClient()
            };
        }

        public Phone Read(string number)
        {
            Dictionary<PhoneData, string> values = new Dictionary<PhoneData, string>();

            PhoneData toQuery = PhoneData.City | PhoneData.CurrentISP | PhoneData.OrignalISP | PhoneData.IsVirtual | PhoneData.Transfered;

            foreach (IPhoneAPIClient client in this.phoneClients)
            {
                try
                {
                    toQuery = client.Query(number, toQuery, values);

                    // 全部的数据都查询完了
                    if (toQuery == 0)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("查询手机卡数据异常", ex);
                }
            }

            return new Phone()
            {
                Number = number,
                City = values.GetValue(PhoneData.City, string.Empty),
                CurrentISP = values.GetValue(PhoneData.CurrentISP, string.Empty),
                OriginalISP = values.GetValue(PhoneData.OrignalISP, string.Empty),
                Transfered = values.GetValue(PhoneData.Transfered, string.Empty) == "true",
                IsVirtual = values.GetValue(PhoneData.IsVirtual, false)
            };
        }
    }
}
