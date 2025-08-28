using DMXOS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

class Program
{

    static SerialPort port;
    // 테스트용 ICCID (더미)
    private readonly Dictionary<DEV_WQ_POS, string> floaterPhoneNumbers = new Dictionary<DEV_WQ_POS, string>()
    {
        //EX 서버는 01220414524
        { DEV_WQ_POS.UPPER, "01220414526" },
        { DEV_WQ_POS.LOWER, "01220414523" }
    };

    static void Main()
    {
        Program.port = new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One);

        port.ReadTimeout = 3000;
        port.WriteTimeout = 1000;
        port.Open();

        new Program().BridgeProcess();
        //new Program().SendMessageToServerPDU();

        new Program().TryParsePDU();


        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }


    public void BridgeProcess() 
    {
        Console.WriteLine("\n--- AT 명령을 입력하세요 (Ctrl+C로 종료, /z 입력 시 Ctrl+Z 전송) ---");

        while (true)
        {
            Console.Write("> ");
            string cmd = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(cmd))
                continue;

            // 일반 명령 입력
            if (cmd.StartsWith("AT+CMGS="))
            {
                port.Write(cmd + "\r");
                // '>' 프롬프트 대기 (간단히 Sleep/Read로 처리)
                System.Threading.Thread.Sleep(500);
                string prompt = port.ReadExisting();
                while (port.BytesToRead > 0)
                {
                    Console.WriteLine("Waiting for more data...");
                    prompt += port.ReadExisting();
                    System.Threading.Thread.Sleep(50);
                }
                Console.WriteLine("Data Returned.");


                if (prompt.Contains(">"))
                {
                    Console.Write("메시지(16진): ");
                    string msg = Console.ReadLine(); // 문자 데이터 입력
                    port.Write(msg); // \r, \n 없이 전송
                    port.Write(new byte[] { 0x1A }, 0, 1); // 바로 Ctrl+Z
                }
            }
            else if (cmd == "/z")
            {
                port.Write(new byte[] { 0x1A }, 0, 1);
            }
            else if (cmd == "/exit")
            {
                port.Write(new byte[] { 0x03 }, 0, 1); // Ctrl+C
                                                        // 또는
                port.Write(new byte[] { 0x1B }, 0, 1); // ESC
            }
            else if (cmd == "/freeze")
            {
                port.Close();
                Console.WriteLine("\n시리얼 포트 연결 중단. 아무키나 눌러 재연결...");

                Console.ReadKey();
                port.Open();
            }
            else if (cmd == "/send_pdu")
            {
                SendMessageToServerPDU();
            }
            else if (cmd == "/send_pduucs")
            {
                SendMessageToServerPDU_UCS();
            }
            else
            {
                port.Write(cmd + "\r");
            }

            System.Threading.Thread.Sleep(200); // 응답 대기

            string resp = port.ReadExisting();
            while (port.BytesToRead > 0)
            {
                Console.WriteLine("Waiting for more data...");
                resp += port.ReadExisting();
                System.Threading.Thread.Sleep(50);
            }
            Console.WriteLine("Data Returned.");
            Console.WriteLine(resp.Replace("\r", "\\r").Replace("\n", "\\n"));
        }
        
    }

    public void ServerCallProcess() 
    {
        List<WQ_Item> wQ_Items = new List<WQ_Item>();
        __SendGetCurrentValue(DEV_WQ_POS.UPPER, ref wQ_Items);

        foreach (WQ_Item item in wQ_Items)
            Console.WriteLine($"Index: {wQ_Items.IndexOf(item)} PV: {item.PV}, Timeout: {item.Timeout}");
    }

    public void SendMessageToServerPDU()
    {
        SerialWriteAndRead(port, "AT+CMGF=0\r");
        SerialWriteAndRead(port, "AT+CSCS=\"IRA\"\r\n");
        SerialWriteAndRead(port, "AT+CSMP=17,167,,4\r");


        string pdu = string.Empty;

        pdu = "0011000B81";
        pdu += PhoneNumberToBcd("01082437730");
        pdu += "000404" + "16";  //고정 길이 패킷임
        pdu += /*ConvertToUtf16Hex("#01\r")*/"3E30310D3E30310D3E30310D3E30310D" + (char)26;  //#01(cr)


        //      pdu =
        //"08" + "91" + "28010099122041F0" + // SMSC
        //"1100" +                 // TP-MTI + TP-MR
        //"0B91" +                 // TP-DA Length + TOA
        //PhoneNumberToBcd("01220414524") +         // TP-DA (수신자)
        //"00" +                   // PID
        //"04" +                   // DCS (8-bit)
        //"04" +                   // UDL
        //"3E30310D" + (char)26;              // UD

        SerialWriteAndRead(port, $"AT+CMGS={pdu.Length / 2 - 1}\r");
        SerialWriteAndRead(port, pdu);

        Thread.Sleep(1000);

        //_port.Write(pdu);
        SerialWriteAndRead(port, "");

    }
    public void SendMessageToServerPDU_UCS()
    {
        SerialWriteAndRead(port, "AT+CMGF=0\r");
        SerialWriteAndRead(port, "AT+CSCS=\"UCS2\"\r\n");
        SerialWriteAndRead(port, "AT+CSMP=17,167,,8\r");

        string dest = "01082437730";
        string msg = "가나다라"; // UCS2
        string ud = ConvertToUtf16Hex(msg); // AC00B098B2E4B77C
        int udl = ud.Length / 2;

        string pdu = "0011000B81";
        pdu += PhoneNumberToBcd(dest);        // 예: 100882437730 → "100882437730F0"
        pdu += "000408";                     // PID, DCS (UCS2), VP 없음
        pdu += udl.ToString("X2");           // UDL: UCS2 바이트 수
        pdu += ud + (char)26;                           // 메시지 내용

        SerialWriteAndRead(port, $"AT+CMGS={pdu.Length / 2 - 1}\r");
        SerialWriteAndRead(port, pdu);

        Thread.Sleep(1000);

        //_port.Write(pdu);
        SerialWriteAndRead(port, "");

    }
    public void SendMessageToServerTEXT()
    {
        SerialWriteAndRead(port, "AT+CMGF=1\r\n");  // 텍스트 모드
        SerialWriteAndRead(port, "AT+CSCS=\"IRA\"\r\n");  // 문자 인코딩 설정 (IRA: ASCII)
        SerialWriteAndRead(port, "AT+CSMP=17,167,,0\r");
        SerialWriteAndRead(port, "AT+CMGS=\"01220414524\"\r");  // 수신 번호
        //SerialWriteAndRead(port, ">01\r" + ((char)26).ToString());  // 메시지 본문 + Ctrl+Z


        SerialWriteAndRead(port, ">01\r" + (char)26);  // 수신 번호
        Thread.Sleep(200);
        SerialWriteAndRead(port, "");
    }
    public void SendMessageToServerTEXT_UCS2()
    {
        SerialWriteAndRead(port, "AT+CSCS=\"UCS2\"\r\n");           // 문자셋 = UCS2 (UTF-16BE)
        SerialWriteAndRead(port, "AT+CMGF=1\r\n");                  // 텍스트 모드
        SerialWriteAndRead(port, "AT+CSMP=,,,8\r\n");         // DCS=8 (UCS2)

        SerialWriteAndRead(port, $"AT+CMGS=\"{ConvertToUtf16Hex("01220414524")}\"\r");  // 수신 번호     // 수신 번호

        //// 바이트열: 3E 30 31 0D → UCS2 문자 2개 (16진수 문자코드로 인식)
        //SerialWriteAndRead(port, 0x3E30310D1A);

        byte[] data = new byte[] { 0x3E, 0x30, 0x31, 0x0D, 0x1A };

        //Console.WriteLine($"[SMSHandleTest] WM-300V <<< {BitConverter.ToString(data).Replace("-", "")}");
        //port.Write(data, 0, data.Length);
        //Thread.Sleep(100);
        //Console.WriteLine($"[SMSHandleTest] WM-300V >>> {port.ReadExisting()}");
        //Thread.Sleep(100);

        port.Write("3E30310D"); // \r, \n 없이 전송
        port.Write(new byte[] { 0x1A }, 0, 1); // 바로 Ctrl+Z

        System.Threading.Thread.Sleep(200); // 응답 대기

        string resp = port.ReadExisting();
        while (port.BytesToRead > 0)
        {
            Console.WriteLine("Waiting for more data...");
            resp += port.ReadExisting();
            System.Threading.Thread.Sleep(50);
        }
        Console.WriteLine("Data Returned.");
        Console.WriteLine(resp.Replace("\r", "\\r").Replace("\n", "\\n"));

        System.Threading.Thread.Sleep(1000); // 응답 대기
        SerialWriteAndRead(port, "");

        //전송 및 수령에는 성공했으나, UCS2 표현범위를 넘어서면 강제로 바이트값이 3F로 소멸됨
    }


    public bool __SendGetCurrentValue(DEV_WQ_POS devPos, ref List<WQ_Item> pvList)
    {
        try
        {
            //if (!ConnectToTarget(_iccids[devPos])) throw new Exception($"Connection failed for {devPos}");

            {
                string pdu, buffer = string.Empty, responseMsg;
                //센서 계측값 요청 패킷 제작
                {
                    pdu = "0011000BA1";
                    pdu += PhoneNumberToBcd(floaterPhoneNumbers[devPos]);
                    pdu += "000400" + "04";  //고정 길이 패킷임
                    pdu += "3E30310D" + (char)26;  //#01(cr)
                }

                //센서 계측값 요청 패킷 전송
                {
                    SerialWriteAndRead(port,"AT+CSCS=\"IRA\"\r\n");
                    SerialWriteAndRead(port,"AT+CMGF=0\r");
                    SerialWriteAndRead(port,"AT+CSMP=17,167,,240\r");
                    SerialWriteAndRead(port,$"AT+CMGS={pdu.Length / 2 - 1}\r");
                    port.Write(pdu);
                    Thread.Sleep(100);
                    Console.WriteLine($"[SMSHandleBeta] WM-300V <<< {pdu}");

                    Thread.Sleep(5000);
                    //TESTING
                    {
                        SerialWriteAndRead(port, $"AT+CMGS={pdu.Length / 2 - 1}\r");

                        pdu = "0011000BA1";
                        pdu += PhoneNumberToBcd("01082437730");
                        pdu += "000400" + "04";  //고정 길이 패킷임
                        pdu += "3E30310D" + (char)26;  //#01(cr)
                        port.Write(pdu);
                        Thread.Sleep(100);
                        Console.WriteLine($"[SMSHandleBeta] WM-300V <<< {pdu}");
                    }
                }

                //센서 계측값 응답 패킷 수형
                {
                    Thread.Sleep(5000);
                    Console.WriteLine($"[SMSHandleBeta] READ START");
                    Thread.Sleep(200);
                    while (port.BytesToRead != 0)
                    {
                        buffer += port.ReadLine();
                        Thread.Sleep(200);
                        Console.WriteLine($"[SMSHandleBeta] READING RESPONSE <<< {buffer}");
                    }
                }

                //센서 계측값 응답 패킷을 찾기
                {
                    var lines = buffer.Split('\r', '\n');
                    List<string> responses = new List<string>();
                    foreach (var line in lines) if (line.StartsWith("+CMTI:")) responses.Add(line);

                    Console.WriteLine($"[SMSHandleBeta] FINDING RESPONSE : " + responses.Count);

                    if (responses.Count == 0)
                    {
                        Console.WriteLine("[SMSHandleBeta] FAILURE : No response of 01 Commnad from device. TIMEOUT");
                        return false;
                    }

                    responseMsg = null;
                    List<int> msgIndexes = responses.Select(s => int.Parse(s.Replace("+CMTI: \"ME\",", string.Empty))).ToList();
                    foreach (int msgIdx in msgIndexes)
                    {
                        port.Write($"AT+CMGR={msgIdx}");
                        Thread.Sleep(300);
                        buffer = port.ReadLine();

                        Console.WriteLine($"[SMSHandleBeta] READING RESPONSE("+ msgIdx + ") : " + buffer);
                        buffer = buffer.Replace("\r", string.Empty).Replace("\n", string.Empty);

                        //해당 버퍼로부터 유효한 패킷이 나왔다면, 본문만 추출하고 break;
                        //TODO!
                        if (false)
                        {
                            responseMsg = "";
                            break;
                        }
                    }

                    if (responseMsg == null)
                    {
                        Console.WriteLine("[SMSHandleBeta] FAILURE : there were responses but, Can not Find a Valid Response of 01 Commnad");
                        return false;
                    }
                }

                responseMsg = "54000000005400000000540000000054000000005400000000540000000054000000005400000000540D";

                //센서 계측값 응답 패킷의 본문 파싱
                {
                    byte[] bytes =  HexStringToBytes(responseMsg);

                    Thread.Sleep(1000);
                    Console.WriteLine($" bytes.Length : {bytes.Length}");
                    foreach (var item in bytes)
                    {
                        Console.WriteLine($" item.ToString(\"X2\") : {item.ToString("X2")}");
                    }
                    Thread.Sleep(1000);

                    if (bytes[0] == 0x54) // 'T'로 시작하지 않으면 실패
                    {
                        Console.WriteLine("[SMSHandleBeta] FAILURE : there was a Valid Response of 01 Commnad but, its header was invalid");
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
        catch(Exception ex)
        {
            Console.WriteLine($"[SMSHandleBeta] {ex.Message} {ex.StackTrace}");
            Console.WriteLine($"[SMSHandleBeta] FAILURE : Unable to get current values for {devPos}");
            
            return false;
        }
    }

    public bool TryParsePDU()
    {
        string responseMsg = "", pdu = "";


        //TestMsg
        string buffer = "\r\n+CMTI: \"ME\",15\r\nAT\r\r\nOK\r\n";

        var lines = buffer.Split('\r', '\n');
        List<string> responses = new List<string>();
        foreach (var line in lines)
        {
            Console.WriteLine($"Line : {line} / {line.Contains("+CMTI:")}");

            if (line.Contains("+CMTI:")) responses.Add(line);
        }

        if (responses.Count == 0)
        {
            Logger.WriteLineAndLog("[SMSHandleBeta] FAILURE : No response of 01 Commnad from device. TIMEOUT");
            return false;
        }

        responseMsg = null;
        List<int> msgIndexes = responses.Select(s => int.Parse(s.Replace("+CMTI: \"ME\",", string.Empty))).ToList();
        foreach (int msgIdx in msgIndexes)
        {
            Console.WriteLine($"msgIdx : {msgIdx}");
            //port.Write($"AT+CMGR={msgIdx}");
            Thread.Sleep(300);
            //buffer = port.ReadLine();

            //TestMsg
            buffer = "0791280102194189440BA11080427337F00084527011903291630F0A22080B811080427337F02E2E2E2E";
            buffer = buffer.Replace("\r", string.Empty).Replace("\n", string.Empty);

            //해당 버퍼로부터 유효한 패킷이 나왔다면, 본문(UserData)만 추출하고 break;
            try
            {
                responseMsg = ExtractUserDataFromPDU(buffer);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMSHandleBeta] {ex.Message} {ex.StackTrace}");
            }

        }

        if (responseMsg == null)
        {
            Logger.WriteLineAndLog("[SMSHandleBeta] FAILURE : there were responses but, Can not Find a Valid Response of 01 Commnad");
            return false;
        }
        
        //TestMsg
        responseMsg = "540000000054000000005400000000540000000054000000005400000000540000000054DB0F4940540D";

        byte[] bytes = HexStringToBytes(responseMsg);


        Console.WriteLine($" bytes.Length : {bytes.Length}");
        foreach (var item in bytes)
        {
            Console.WriteLine($" item.ToString(\"X2\") : {item.ToString("X2")}");
        }


        if (bytes[0] != 0x54) // 'T'로 시작하지 않으면 실패
        {
            Logger.WriteLineAndLog("[SMSHandleBeta] FAILURE : there was a Valid Response of 01 Commnad but, its header was invalid");
            return false;
        }

        var pvList = new List<WQ_Item>();
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

        pvList.ForEach(item => Console.WriteLine(pvList.IndexOf(item) + " PV: " + item.PV + ", Timeout: " + item.Timeout));



        //string testPDU = "0011000B81" + PhoneNumberToBcd("01082437730") + "00040416" + "3E30310D3E30310D3E30310D3E30310D";

        Console.WriteLine($"[SMSHandleBeta] PDU Response: {responseMsg}");

        return false;
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


    public void SerialWriteAndRead(SerialPort _port , string msgString)
    {
        byte[] msgBytes = Encoding.ASCII.GetBytes(msgString);
        Console.WriteLine($"[SMSHandleTest] WM-300V <<< {msgString}");
        _port.Write(msgBytes, 0, msgBytes.Length);
        Thread.Sleep(100);
        Console.WriteLine($"[SMSHandleTest] WM-300V >>> {_port.ReadExisting()}");
        Thread.Sleep(100);
    }
    public void SerialWriteBEAndRead(SerialPort _port, string msgString)
    {
        byte[] msgBytes = Encoding.BigEndianUnicode.GetBytes(msgString);
        Console.WriteLine($"[SMSHandleTest] WM-300V <<< {BitConverter.ToString(msgBytes).Replace("-", "")}");
        _port.Write(msgBytes, 0, msgBytes.Length);
        Thread.Sleep(100);
        Console.WriteLine($"[SMSHandleTest] WM-300V >>> {_port.ReadExisting()}");
        Thread.Sleep(100);
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

}


//SerialWriteAndRead(port, "AT+CSCS="UCS2"\r\n");
//SerialWriteAndRead(port, "AT+CMGF=1\r");
//SerialWriteAndRead(port, "AT+CSMP=,,,8\r");
//SerialWriteAndRead(port, "AT+CMGS=\"00300031003000380032003400330037003700330030\"");
//port.Write("AC00B098B2E4D558\n");

//Thread.S

//            port.Write(new byte[] { 26 }, 0, 1); // Ctrl+Z

/*
AT+CSCS="UCS2"
AT+CMGF=1
AT+CSMP=,,,8
AT+CMGS="00300031003000380032003400330037003700330030"
AC00B098B2E4D558

 */

/*
 Data Returned.
\r\n+QIND: "POWER",1\r\n\r\n+CFUN: 1\r\n\r\n+CPIN: READY\r\n\r\n+QIND: "USIM",1\r\n\r\n+QIND: "SMS",1\r\n\r\n+QIND: "POWER",1\r\n\r\n+CFUN: 1\r\n\r\n+CPIN: READY\r\n\r\n+QIND: "USIM",1\r\n\r\n+QIND: "SMS",1\r\n\r\n+QIND: "PB",1\r\nAT+CSCS="UCS2"\r\r\nOK\r\n
> AT+CSCS="UCS2"
Data Returned.


 */