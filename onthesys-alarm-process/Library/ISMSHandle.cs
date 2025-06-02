using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    internal interface ISMSHandle
    {
        /// <summary>
        /// sms 문자 발송 인터페이스 
        /// </summary>
        /// <param name="phoneNumber">  - 가 제거된 휴대폰 번호 </param>
        /// <param name="smsMessage">  최대 길이 130 Byte 문자열 </param>
        /// <returns>
        //                 1 : 성공
        //                -1 : 실패 
        //                     - 2 : ****
        //                     - 3 : ****
        //                     - 4 : ****
        //                     - 5 : ****
        /// </returns>

        int SendSMSToOne(string phoneNumber, string smsMessage);
        bool SendSMSToList(List<string> phoneNumList, string smsMessage);
        bool SendGetCurrentValue(DEV_WQ_POS devPos, ref List<WQ_Item> pvList);

    }
}
