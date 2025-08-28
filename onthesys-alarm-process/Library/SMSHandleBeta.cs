using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DMXOS
{
    public class SMSHandleBeta : ISmsHandle, IDisposable
    {
        private SerialPort _port;
        private readonly object _lock = new object();

        // 테스트용 ICCID (더미)
        private readonly Dictionary<DEV_WQ_POS, string> floaterPhoneNumbers = new Dictionary<DEV_WQ_POS, string>()
        {
            //EX 서버는 01220414524
            { DEV_WQ_POS.UPPER, "01220414526" },    
            { DEV_WQ_POS.LOWER, "01220414523" }    
        };

        public string TEST_PDU = "0791280102194189440BA11080427337F00084527001711100630F0A22080B811080427337F02E2E2E2E";
        public SMSHandleBeta()
        {
            try
            {
                _port = new SerialPort("COM10", 115200, Parity.None, 8, StopBits.One);
                _port.ReadTimeout = 1000;
                _port.WriteTimeout = 1000;

                if (!_port.IsOpen)
                {
                    _port.Open();

                    Logger.WriteLineAndLog($"[SMSHandleBeta] port opened ");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLineAndLog($"[SMSHandleBeta] Failed to open port: {ex.Message}");
            }
        }

        public bool SendSMSToOne(string phoneNumString, string smsMessage)
        {
            lock (_lock)
            {
                try
                {
                    // 전화번호 형식 검사 (11자리 숫자)
                    if (!Regex.IsMatch(phoneNumString, @"\d{11}$"))
                    {
                        Logger.WriteLineAndLog($"[SMSHandleBeta] INVALID PHONE NUMBER : {phoneNumString}");
                        return false;
                    }

                    // 인코딩 및 데이터 단위 지정, SMS 모드 설정
                    SerialWriteAndRead("AT+CSCS=\"UCS2\"\r\n");
                    SerialWriteAndRead("AT+CMGF=1\r");
                    SerialWriteAndRead("AT+CSMP=,,,8\r");

                    // SMS 메시지 길이 제한 (UTF-16 기준, 70자)
                    int sizeMax = 140;
                    int sizeNow = Encoding.ASCII.GetByteCount(smsMessage) * 2;
                    //Logger.WriteLineAndLog($"textSize = {sizeNow}/{sizeMax}");
                    if (sizeNow > sizeMax)
                    {
                        Logger.WriteLineAndLog($"[SMSHandleBeta] MESSAGE TOO LONG : {sizeNow}/{sizeMax} bytes");
                        return false;
                    }

                    //메시지 발송
                    SerialWriteAndRead($"AT+CMGS=\"{ConvertToUtf16Hex(phoneNumString)}\"\r");
                    SerialWriteAndRead(ConvertToUtf16Hex(smsMessage) + (char)26);

                    //성송 알람
                    Logger.WriteLineAndLog($"[SMSHandleTest] SUCCEED : Sending SMS to {phoneNumString} : {smsMessage}");

                    //지연
                    Thread.Sleep(200);
                    return true;
                }
                catch
                {
                    Logger.WriteLineAndLog($"[SMSHandleTest] FAILURE : Unable Sending SMS to {phoneNumString}");

                    return false;
                }
            }
        }
        private bool SendSMSToOnePDU(string phoneNumString, string smsMessage)
        {
            lock (_lock)
            {
                try
                {
                    if (!Regex.IsMatch(phoneNumString, @"\d{11}$"))
                    {
                        Logger.WriteLineAndLog($"[SMSHandleBeta] INVALID PHONE NUMBER : {phoneNumString}");
                        return false;
                    }

                    smsMessage = smsMessage.Substring(0, 30);

                    string data = "00F1FF0BA1";

                    if (phoneNumString.Length % 2 == 1)
                        phoneNumString += "F";

                    var charArray = phoneNumString.ToCharArray();
                    for (int i = 0; i < charArray.Length / 2; i++)
                    {
                        var temp = charArray[i * 2];
                        charArray[i * 2] = charArray[i * 2 + 1];
                        charArray[i * 2 + 1] = temp;
                    }
                    phoneNumString = new string(charArray);

                    data += phoneNumString;
                    data += "0008";
                    data += (("hi" + smsMessage).Length*2).ToString("D2");
                    data += "0500031A0201";
                    data += ConvertToUtf16Hex(("hi" + smsMessage)) + (char)26;


                    SerialWriteAndRead("AT+CMGF=0\r");
                    SerialWriteAndRead($"AT+CMGS={data.Length / 2 - 1}\r");
                    SerialWriteAndRead(data);
                    SerialWriteAndRead("");

                    Thread.Sleep(1000);

                    data = "00F1FF0BA1";
                    data += phoneNumString;
                    data += "0008";
                    data += (smsMessage.Length * 2).ToString("D2");
                    data += "0500031A0202";
                    data += ConvertToUtf16Hex(smsMessage) + (char)26;


                    SerialWriteAndRead("AT+CMGF=0\r");
                    SerialWriteAndRead($"AT+CMGS={data.Length / 2 - 1}\r");
                    SerialWriteAndRead(data);

                    Logger.WriteLineAndLog($"[SMSHandleTest] SUCCEED : Sending SMS to {phoneNumString} : {smsMessage}");

                    Thread.Sleep(1000);
                    return true;
                }
                catch(Exception e)
                {
                    Logger.WriteLineAndLog($"[SMSHandleTest] FAILURE : Unable Sending SMS to {phoneNumString}");
                    Logger.WriteLineAndLog($"[SMSHandleTest] Exception : {e}");

                    return false;
                }
            }
        }

        public void SerialWriteAndRead  (string msgString)
        {
            byte[] msgBytes = Encoding.ASCII.GetBytes(msgString);
            Logger.WriteLineAndLog($"[SMSHandleTest] WM-300V <<< {msgString}");
            _port.Write(msgBytes, 0, msgBytes.Length);
            Thread.Sleep(100);
            Logger.WriteLineAndLog($"[SMSHandleTest] WM-300V >>> {_port.ReadExisting()}");
            Thread.Sleep(100);
        }

        public bool SendSMSToList(List<string> phoneNumList, string smsMessage)
        {
            bool success = true;
            foreach (var phone in phoneNumList)
            {
                success &= SendSMSToOne(phone, smsMessage);
            }
            return success;
        }

        public bool __SendGetCurrentValue(DEV_WQ_POS devPos, ref List<WQ_Item> pvList)
        {
            try
            {
                //if (!ConnectToTarget(_iccids[devPos])) throw new Exception($"Connection failed for {devPos}");

                lock (_lock)
                {
                    string pdu, buffer, responseMsg;
                    //센서 계측값 요청 패킷 제작
                    {
                        pdu = "0011000BA1";
                        pdu += PhoneNumberToBcd(floaterPhoneNumbers[devPos]);
                        pdu += "000400" + "04";  //고정 길이 패킷임
                        pdu += "3E30310D" + (char)26;  //#01(cr)
                    }

                    //센서 계측값 요청 패킷 전송
                    {
                        SerialWriteAndRead("AT+CSCS=\"IRA\"\r\n");
                        SerialWriteAndRead("AT+CMGF=0\r");
                        SerialWriteAndRead("AT+CSMP=17,167,,240\r");
                        SerialWriteAndRead($"AT+CMGS={pdu.Length / 2 - 1}\r");
                        _port.Write(pdu);
                    }
                    
                    //센서 계측값 응답 패킷 수형
                    {
                        Thread.Sleep(5000);
                        buffer = _port.ReadLine();
                        Thread.Sleep(200);
                        while (_port.BytesToRead != 0)
                        {
                            buffer += _port.ReadLine();
                            Thread.Sleep(200);
                        }
                    }

                    //buffer = "0791280102194189440BA11080427337F00084527001711100630F0A22080B811080427337F02E2E2E2E";

                    //센서 계측값 응답 패킷을 찾기
                    {
                        var lines = buffer.Split('\r', '\n');
                        List<string> responses = new List<string>();
                        foreach (var line in lines) if (line.StartsWith("+CMTI:")) responses.Add(line);

                        if (responses.Count == 0)
                        {
                            Logger.WriteLineAndLog("[SMSHandleBeta] FAILURE : No response of 01 Commnad from device. TIMEOUT");
                            return false;
                        }

                        responseMsg = null;
                        List<int> msgIndexes = responses.Select(s => int.Parse(s.Replace("+CMTI: \"ME\",", string.Empty))).ToList();
                        foreach (int msgIdx in msgIndexes)
                        {
                            _port.Write($"AT+CMGR={msgIdx}");
                            Thread.Sleep(300);
                            buffer = _port.ReadLine();

                            buffer = buffer.Replace("\r", string.Empty).Replace("\n", string.Empty);

                            //해당 버퍼로부터 유효한 패킷이 나왔다면, 본문(UserData)만 추출하고 break;
                        
                            responseMsg = ExtractUserDataFromPDU(buffer);
                        
                        }

                        if (responseMsg == null)
                        {
                            Logger.WriteLineAndLog("[SMSHandleBeta] FAILURE : there were responses but, Can not Find a Valid Response of 01 Commnad");
                            return false;
                        }
                    }

                    //센서 계측값 응답 패킷의 본문 파싱
                    {
                        byte[] bytes = HexStringToBytes(responseMsg);

                        if (bytes[0] != 0x54) // 'T'로 시작하지 않으면 실패
                        {
                            Logger.WriteLineAndLog("[SMSHandleBeta] FAILURE : there was a Valid Response of 01 Commnad but, its header was invalid");
                            return false;
                        }

                        pvList.Clear();
                        for (int i = 0; i < 8; i++)
                        {
                            int offset = 1 + i * 5;

                            byte[] rawPV = bytes.Skip(offset).Take(4).ToArray();

                            // 시스템이 Big Endian일 경우 Little Endian으로 변환
                            if (!BitConverter.IsLittleEndian)
                                Array.Reverse(rawPV);

                            float val = BitConverter.ToSingle(rawPV, 0);
                            byte status = bytes[offset + 4];

                            pvList.Add(new WQ_Item
                            {
                                PV = val,
                                Timeout = (status == 0x54)
                            });
                        }
                    }

                    //성공 알람
                    return true;
                }
            }
            catch
            {
                Logger.WriteLineAndLog($"[SMSHandleBeta] FAILURE : Unable to get current values for {devPos}");
                return false;
            }
        }
        public bool SendGetCurrentValue(DEV_WQ_POS devPos, ref List<WQ_Item> pvList)
        {
            //if (random.Next(10) == 1) return false; // 10% 확률로 실패


            Logger.WriteLineAndLog($"[SMSHandleTest] : {devPos}");

            DateTime now = DateTime.Now; 
            float totalSeconds = (float)(now.Ticks / 10_000_000.0 % 10000.0) * 0.0003f;


            //테스트 데이터 생성용 코드니 무시
            Func<int, int> seed = sensorId => (int)(2 + devPos) * 911 + sensorId * 214807;

            Func<int, float, float> value = (sensorId, noiseSize) => (float)(
                (Math.Sin(seed(sensorId) + totalSeconds ) +
                Math.Cos((seed(sensorId) + totalSeconds ) * 1.41) +
                2 * Math.Sin((seed(sensorId) + totalSeconds) / 1.41) + (4 + noiseSize * 0.5f)) / (8 + noiseSize * 0.5)
                /*+ noiseSize*((float)random.NextDouble() - 0.5)*/);

            //Logger.WriteLineAndLog($"[SMSHandleTest] seed : {seed(1)}");
            Logger.WriteLineAndLog($"[SMSHandleTest] totalSeconds : {totalSeconds}({(now.Ticks- 638859431365390441f)/1_0000_0000f})");
            //Logger.WriteLineAndLog($"[SMSHandleTest] value : {value(1,0.1f)}");
            // 데이터 생성
            pvList = new List<WQ_Item> {
                new WQ_Item { Timeout = false, PV = value(1, 0.0f) * 200 },
                new WQ_Item { Timeout = false, PV = value(2, 0.0f) * 200  },
                new WQ_Item { Timeout = false, PV = value(3, 0.02f) * 200  },
                new WQ_Item { Timeout = false, PV = value(4, 0.01f) * 6  },
                new WQ_Item { Timeout = false, PV = value(5, 0.01f) * 1+5  },
                new WQ_Item { Timeout = false, PV = value(6, 0.01f) * 100  },
                new WQ_Item { Timeout = false, PV = value(7, 0.01f) * 30  },
                new WQ_Item { Timeout = false, PV = value(8, 0.01f) * 5  },
            };


            //if (random.Next(10) == 1)
            //{
            //    int toIdx = random.Next(7);
            //    for (int i = toIdx; i < pvList.Count; i++)
            //    {
            //        pvList[i].Timeout = true; // Randomly set one item to timeout
            //        Logger.WriteLineAndLog($"[SMSHandleTest] TIMEOUT : {pvList[i].PV}");
            //    }
            //}
            Logger.WriteLineAndLog($"[SMSHandleTest] : {pvList.Count} DATA");

            return true;
        }
       
        public static string ConvertToUtf16Hex(string input)
        {
            StringBuilder sb = new StringBuilder();

            // 입력된 문자열의 각 문자에 대해 UTF-16 값으로 변환
            foreach (char c in input)
            {
                sb.AppendFormat("{0:X4}", (int)c);  // 각 문자를 16진수로 변환하여 추가
            }

            return sb.ToString();
        }

        static string PhoneNumberToBcd(string phoneNumString) 
        {
            if (phoneNumString.Length % 2 == 1)
                phoneNumString += "F";

            var charArray = phoneNumString.ToCharArray();
            for (int i = 0; i < charArray.Length / 2; i++)
            {
                var temp = charArray[i * 2];
                charArray[i * 2] = charArray[i * 2 + 1];
                charArray[i * 2 + 1] = temp;
            }
            return new string(charArray);
        }
        public byte[] HexStringToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have even length");

            byte[] result = new byte[hex.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }

        public void Dispose()
        {
            _port.Dispose();
        }


        string ExtractUserDataFromPDU(string pdu)
        {
            try
            {
                // 1. SMSC 영역 길이
                int smscLen = Convert.ToInt32(pdu.Substring(0, 2), 16);
                int smscEnd = 2 + smscLen * 2;

                // 2. 발신자 번호 길이 및 바이트 수 계산
                int senderLen = Convert.ToInt32(pdu.Substring(smscEnd + 2, 2), 16);
                int senderBytes = (senderLen + 1) / 2;

                // 3. PID/DCS/타임스탬프 등을 포함한 User Data 시작 offset 계산
                int offset = smscEnd           // SMSC 끝
                           + 2                 // First octet
                           + 2                 // Sender number length
                           + 2                 // TOA
                           + senderBytes * 2  // Sender address
                           + 2                 // PID
                           + 2                 // DCS
                           + 14;               // Timestamp (7 bytes = 14 hex chars)

                // 4. User Data Length (UDL)
                int udl = Convert.ToInt32(pdu.Substring(offset, 2), 16);

                // 5. User Data 전체 (UDH 포함)
                string udRaw = pdu.Substring(offset + 2, udl * 2);

                // 6. UDH 존재 여부 확인
                int udhLen = Convert.ToInt32(udRaw.Substring(0, 2), 16);
                int udhTotalLen = (udhLen + 1) * 2; // UDH 길이 필드 포함

                // 7. 실제 본문 추출 (UDH 이후부터)
                string userDataHex = udRaw.Substring(udhTotalLen);
                return userDataHex;
            }
            catch
            {
                return null; // 에러 발생 시 null 반환
            }
        }

    }
}
