using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMXOS
{
    public enum DEV_WQ_POS
    {
        UPPER = 0x0,
        LOWER
    }

    public class WQ_Item
    {
        public bool Timeout { get; set; }
        public float PV { get; set; }
    }


    public interface ISmsHandle
    {
        bool SendSMSToOne(string phoneNumString, string smsMessage);
        bool SendSMSToList(List<string> phoneNumList, string smsMessage);
        bool SendGetCurrentValue(DEV_WQ_POS devPos, ref List<WQ_Item> pvList);
    }
}

