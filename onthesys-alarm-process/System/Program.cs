using DMXOS;
using onthesys_alarm_process.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onthesys_alarm_process
{ 
    class Program {

        static void Main() => new Application().AwaitQuit();

        //static void Main()
        //{

        //    using (var sms = new SMSHandleBeta())
        //    {
        //        sms.GetOwnUsimID();
        //        sms.SendSMSToOne("01082437730", "[ [] ]] [p][] [][ ][]]");
        //    }
        //}
    }
}
