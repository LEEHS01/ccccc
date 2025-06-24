using DMXOS;
using onthesys_alarm_process.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace onthesys_alarm_process.Process
{
    public class Application
    {
        public DbManager dbManager;
        //public FilterManager filterManager;
        //public UiManager uiManager;
        public SmsManager smsManager;

        public event Action OnInitiating;       //초기화 시작

        public bool IsQuiting { get; set; } = false;

        public Application()
        {
            Logger.WriteLineAndLog("[Application is Started]");
            //var sms = new SMSHandleTest();
            var sms = new SMSHandleBeta();
            dbManager = new DbManager(this);
            //filterManager = new FilterManager(this);
            //uiManager = new UiManager(this);
            smsManager = new SmsManager(this, sms);

            Thread.Sleep(100); // 1초 대기
            OnInitiating?.Invoke();
        }

        public void AwaitQuit()
        {
            while (!IsQuiting) Thread.Sleep(100);

            dbManager.Quit();
            //filterManager.Quit();
            //uiManager.Quit();
            smsManager.Quit();

            Logger.WriteLineAndLog("[Application is quitted]");
            Logger.Close();

            Console.ReadKey();
        }

    }
}
