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
        //DEV_WQ_POS.UPPER 또는 LOWER 위치의 센서 데이터 수집
        //pvList에 측정값들을 담아서 반환
        public bool SendGetCurrentValue(DEV_WQ_POS devPos, ref List<WQ_Item> pvList)
        {
            Console.WriteLine($"[SMSHandleTest] : {devPos}");

            // 진짜 구현할 부분 - 임시 데이터
            pvList = new List<WQ_Item> {
                new WQ_Item { Timeout = false, PV = 7.2f },
                new WQ_Item { Timeout = false, PV = 8.5f },
                new WQ_Item { Timeout = false, PV = 18.3f }
             };

            Console.WriteLine($"[SMSHandleTest] : {pvList.Count} DATA");
            return true;
        }

        public bool SendSMSToList(List<string> phoneNumList, string smsMessage)
        {
            Console.WriteLine($"[SMSHandleTest] REQUEST : GROUP SEND  {phoneNumList} : {smsMessage}");

            if (phoneNumList.Count == 0) return true;

            bool allSuccess = true;
            int successCount = 0;

            foreach (string phone in phoneNumList)
            {
                int result = SendSMSToOne(phone, smsMessage);
                if (result == 1)
                    successCount++;
                else
                {
                    Console.WriteLine($"[SMSHandleTest] FAIL : {phone}");
                    allSuccess = false;
                }
            }

            Console.WriteLine($"[SMSHandleTest] GROUP RESULT : {successCount}/{phoneNumList.Count} success");
            return allSuccess;
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
