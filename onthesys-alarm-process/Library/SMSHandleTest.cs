using DMXOS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace onthesys_alarm_process.Library
{
    internal class SMSHandleTest : ISmsHandle
    {
        static Random random = new Random();
        public bool SendGetCurrentValue(DEV_WQ_POS devPos, ref List<WQ_Item> pvList)
        {
            if(random.Next(10) == 1) return false; // 10% 확률로 실패


            Logger.WriteLineAndLog($"[SMSHandleTest] : {devPos}");

            DateTime now = DateTime.Now;
            long totalSeconds = now.Ticks / 10_000_000;

            //테스트 데이터 생성용 코드니 무시
            Func<int, int> seed = sensorId => (int)(2+devPos) * 1241 + sensorId * 21414512;

            Func< int, float,float> value = (sensorId, noiseSize) =>  (float)(
                (Math.Sin(seed(sensorId) + totalSeconds / 60f / 60f) + 
                Math.Cos((seed(sensorId) + totalSeconds / 60f / 60f) * 1.41) + 
                2 * Math.Sin((seed(sensorId) + totalSeconds / 60f / 60f) / 1.41) + (4 + noiseSize * 0.5f)) / (8 + noiseSize*0.5)
                /*+ noiseSize*((float)random.NextDouble() - 0.5)*/);

            // 데이터 생성
            pvList = new List<WQ_Item> {
                new WQ_Item { Timeout = false, PV = value(1, 0.1f) * 100 },
                new WQ_Item { Timeout = false, PV = value(2, 0.1f) * 140  },
                new WQ_Item { Timeout = false, PV = value(3, 0.02f) * 1000  },
                new WQ_Item { Timeout = false, PV = value(4, 0.01f) * 20  },
                new WQ_Item { Timeout = false, PV = value(5, 0.01f) * 1 + 6  },
                new WQ_Item { Timeout = false, PV = value(6, 0.01f) * 10000  },
                new WQ_Item { Timeout = false, PV = value(7, 0.01f) * 100  },
                new WQ_Item { Timeout = false, PV = value(8, 0.01f) * 20  },
            };


            if (random.Next(10) == 1) 
            {
                int toIdx = random.Next(7);
                for (int i = toIdx; i < pvList.Count; i++)
                {
                    pvList[i].Timeout = true; // Randomly set one item to timeout
                    Logger.WriteLineAndLog($"[SMSHandleTest] TIMEOUT : {pvList[i].PV}");
                }
            }
            Logger.WriteLineAndLog($"[SMSHandleTest] : {pvList.Count} DATA");

            return true;
        }

        public bool SendSMSToList(List<string> phoneNumList, string smsMessage)
        {
            Logger.WriteLineAndLog($"[SMSHandleTest] REQUEST : GROUP SEND  {phoneNumList} : {smsMessage}");

            if (phoneNumList.Count == 0) return true;

            bool allSuccess = true;
            int successCount = 0;

            foreach (string phone in phoneNumList)
            {
                bool result = SendSMSToOne(phone, smsMessage);
                if (result == true)
                    successCount++;
                else
                {
                    Logger.WriteLineAndLog($"[SMSHandleTest] FAIL : {phone}");
                    allSuccess = false;
                }
            }

            Logger.WriteLineAndLog($"[SMSHandleTest] GROUP RESULT : {successCount}/{phoneNumList.Count} success");
            return allSuccess;
        }

        public bool SendSMSToOne(string phoneNumString, string smsMessage)
        {
            Logger.WriteLineAndLog($"[SMSHandleTest] REQUEST : \n\t : Sending SMS to {phoneNumString} : {smsMessage}");

            if (!Regex.IsMatch(phoneNumString, @"\d{11}$")) {
                Logger.WriteLineAndLog($"[SMSHandleTest] FAILURE : {-1}\n\t 입력 전화번호가 유효하지 않은 방식입니다.");
                return false; //
            }

            int sizeMax = 130; // 130 Byte
            int sizeNow = Encoding.Default.GetByteCount(smsMessage);

            if (sizeNow > sizeMax)
            {
                Logger.WriteLineAndLog($"[SMSHandleTest] FAILURE : {-2}\n\t 입력한 메세지의 길이가 전송 가능한 한도 [{sizeMax}] byte를 초과했습니다!");
                return false;
            }

            Logger.WriteLineAndLog($"[SMSHandleTest] SUCCEED : {1}\n\t 메세지 전송에 성공했습니다.");
            return true; // Simulate success
        }
    }
}
