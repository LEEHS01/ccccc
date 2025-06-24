using System;
using System.Collections.Generic;
using System.IO.Ports;
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
        private readonly Dictionary<DEV_WQ_POS, string> _iccids = new Dictionary<DEV_WQ_POS, string>()
        {
            { DEV_WQ_POS.UPPER, "DUMMY_ICCID_UPPER" },
            { DEV_WQ_POS.LOWER, "DUMMY_ICCID_LOWER" }
        };

        public SMSHandleBeta()
        {
            _port = new SerialPort("COM8", 9600, Parity.None, 8, StopBits.One);
            _port.ReadTimeout = 1000;
            _port.WriteTimeout = 1000;

            if (!_port.IsOpen)
            {
                _port.Open();

                Logger.WriteLineAndLog($"[SMSHandleBeta] port opened ");
            }
        }

        public bool SendSMSToOne(string phoneNumString, string smsMessage)
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

                    //SerialWriteAndRead("AT+CSCS=\"IRA\"\r");
                    //SerialWriteAndRead("AT+CMGF=1\r");
                    //SerialWriteAndRead("AT+CSMP=17,167,0,0\r");
                    //SerialWriteAndRead("AT+CSCA?\r");

                    int sizeMax = 130;
                    int sizeNow = Encoding.Default.GetByteCount(smsMessage);
                    if (sizeNow > sizeMax)
                    {
                        Logger.WriteLineAndLog($"[SMSHandleBeta] MESSAGE TOO LONG : {sizeNow}/{sizeMax} bytes");
                        return false;
                    }


                    SerialWriteAndRead($"AT+CMGS=\"{phoneNumString}\"\r");
                    SerialWriteAndRead(smsMessage + (char)26);

                    Logger.WriteLineAndLog($"[SMSHandleTest] SUCCEED : Sending SMS to {phoneNumString} : {smsMessage}");

                    Thread.Sleep(1000);
                    return true;
                }
                catch
                {
                    Logger.WriteLineAndLog($"[SMSHandleTest] FAILURE : Unable Sending SMS to {phoneNumString}");

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
                if (!ConnectToTarget(_iccids[devPos])) throw new Exception($"Connection failed for {devPos}");

                lock (_lock)
                {
                    // #01 명령어 전송
                    byte[] request = Encoding.ASCII.GetBytes("#01\r"); ;
                    _port.Write(request, 0, request.Length);

                    // 응답 42바이트 수신
                    byte[] buffer = new byte[42];
                    int totalRead = 0;
                    while (totalRead < 42)
                    {
                        int bytesRead = _port.Read(buffer, totalRead, 42 - totalRead);
                        totalRead += bytesRead;
                    }

                    if (buffer[0] == 0x54) // 'T'로 시작하지 않으면 실패
                        return false;

                    pvList.Clear();
                    for (int i = 0; i < 8; i++)
                    {
                        int offset = 1 + i * 5;
                        float val = BitConverter.ToSingle(buffer, offset);
                        byte status = buffer[offset + 4];

                        pvList.Add(new WQ_Item
                        {
                            PV = val,
                            Timeout = (status == 0x54)
                        });
                    }

                    return true;
                }
            }
            catch
            {
                Logger.WriteLineAndLog($"[SMSHandleBeta] FAILURE : Unable to get current values for {devPos}");
                return false;
            }
        }
        static Random random = new Random();
        public bool SendGetCurrentValue(DEV_WQ_POS devPos, ref List<WQ_Item> pvList)
        {
            if (random.Next(10) == 1) return false; // 10% 확률로 실패


            Logger.WriteLineAndLog($"[SMSHandleTest] : {devPos}");

            DateTime now = DateTime.Now; 
            float totalSeconds = (float)(now.Ticks / 10_000_000.0 % 10000.0);


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
                new WQ_Item { Timeout = false, PV = value(1, 0.0f) * 300 },
                new WQ_Item { Timeout = false, PV = value(2, 0.0f) * 300  },
                new WQ_Item { Timeout = false, PV = value(3, 0.02f) * 1500  },
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
        public bool SendDeviceReset(DEV_WQ_POS devPos, out string errorCode)
        {
            errorCode = "";

            lock (_lock)
            {
                try
                {
                    // 대상 장치 연결
                    if (!ConnectToTarget(_iccids[devPos])) throw new Exception($"Connection failed for {devPos}");


                    // 초기화 명령어 전송
                    byte[] request = Encoding.ASCII.GetBytes("#02\r");
                    _port.Write(request, 0, request.Length);

                    // 응답 첫 바이트 판단용
                    int firstByte = _port.ReadByte();
                    if (firstByte == 0x3E) // '>'
                    {
                        // 성공 응답: 3바이트 추가 수신
                        byte[] rest = ReadBytes(_port, 3);
                        if (rest[0] == 0x79 && rest[1] == 0x75 && rest[2] == 0x0D)
                            return true;

                        Logger.WriteLineAndLog("[SMSHandleBeta] MALFORMED SUCCESS RESPONSE");
                        return false;
                    }
                    else if (firstByte == 0x3F) // '?'
                    {
                        // 실패 응답: 에러코드 5바이트 수신
                        byte[] err = ReadBytes(_port, 5);
                        errorCode = Encoding.ASCII.GetString(err);
                        Logger.WriteLineAndLog($"[SMSHandleBeta] DEVICE RESET FAIL: Code={errorCode}");
                        return false;
                    }
                    else
                    {
                        Logger.WriteLineAndLog($"[SMSHandleBeta] UNKNOWN RESPONSE PREFIX: 0x{firstByte:X2}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLineAndLog($"[SMSHandleBeta] EXCEPTION in SendDeviceReset: {ex.Message}");
                    return false;
                }
            }
        }

        

        public string GetOwnUsimID()
        {
            lock (_lock)
            {
                try
                {
                    _port.DiscardInBuffer(); // 이전 수신 버퍼 제거

                    SerialWriteAndRead($"AT+CCID\r");
                   
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.WriteLineAndLog($"[SMSHandleBeta] EXCEPTION in GetOwnUsimID: {ex.Message}");
                    return null;
                }
            }
        }
        bool ConnectToTarget(string targetIccid)
        {
            lock (_lock)
            {
                try
                {
                    _port.DiscardInBuffer(); // 응답 뒤섞임 방지

                    SerialWriteAndRead($"AT+ROUTE={targetIccid}\r");

                    Logger.WriteLineAndLog($"[SMSHandleBeta] ConnectToTarget Succeed");
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.WriteLineAndLog($"[SMSHandleBeta] EXCEPTION in ConnectToTarget: {ex}");
                    return false;
                }
            }
        }

        private byte[] ReadBytes(SerialPort port, int length, int timeoutMs = 1000)
        {
            byte[] buffer = new byte[length];
            int totalRead = 0;
            int startTick = Environment.TickCount;

            while (totalRead < length)
            {
                if (port.BytesToRead > 0)
                {
                    int toRead = Math.Min(port.BytesToRead, length - totalRead);
                    totalRead += port.Read(buffer, totalRead, toRead);
                }
                else
                {
                    Thread.Sleep(1);
                }

                if (Environment.TickCount - startTick > timeoutMs)
                    throw new TimeoutException($"Read timed out: {totalRead}/{length} bytes received.");
            }

            return buffer;
        }

        public void Dispose()
        {
            _port.Dispose();
        }
    }
}
