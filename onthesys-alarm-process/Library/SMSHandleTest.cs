using DMXOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace onthesys_alarm_process.Library
{
    internal class SMSHandleTest : ISMSHandle
    {
        public bool SendGetCurrentValue(DEV_WQ_POS devPos, ref List<WQ_Item> pvList)
        {
            throw new NotImplementedException();
        }

        public bool SendSMSToList(List<string> phoneNumList, string smsMessage)
        {
            throw new NotImplementedException();
        }

        public int SendSMSToOne(string phoneNumber, string smsMessage)
        {
            Console.WriteLine($"[SMSHandleTest] REQUEST : \n\t : Sending SMS to {phoneNumber} : {smsMessage}");

            if (!Regex.IsMatch(phoneNumber, @"\d{11}$")) {
                Console.WriteLine($"[SMSHandleTest] FAILURE : {-1}\n\t 입력 전화번호가 유효하지 않은 방식입니다.");
                return -1; //
            }

            int sizeMax = 130; // 130 Byte
            int sizeNow = Encoding.Default.GetByteCount(smsMessage);

            if (sizeNow > sizeMax)
            {
                Console.WriteLine($"[SMSHandleTest] FAILURE : {-2}\n\t 입력한 메세지의 길이가 전송 가능한 한도 [{sizeMax}] byte를 초과했습니다!");
                return -1;
            }

            Console.WriteLine($"[SMSHandleTest] SUCCEED : {1}\n\t 메세지 전송에 성공했습니다.");
            return 1; // Simulate success
        }
    }
}
