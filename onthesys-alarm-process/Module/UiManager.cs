using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace onthesys_alarm_process.Process
{
    public class UiManager : Manager
    {

        public UiManager(Application app) : base(app)
        {
            interval = 25;
        }

        protected override void OnInitiate()
        {
            //app.smsManager.OnAlarmOccured += alarm =>
            //{
            //    lock (buffer)
            //        buffer.Enqueue(DateTime.Now.ToString("[yyMMdd_HHmmss] 알람 발생"));
            //};
            //app.smsManager.OnSmsSended += () =>
            //{
            //    lock (buffer)
            //        buffer.Enqueue(DateTime.Now.ToString("[yyMMdd_HHmmss] ") + "SMS 발송");
            //};
            //app.dbManager.OnDataDownloaded += (upper) =>
            //{
            //    lock (buffer)
            //        buffer.Enqueue(DateTime.Now.ToString("[yyMMdd_HHmmss] 데이터 로드됨") + $"[Data] {upper.Count}개");
            //};
            //app.dbManager.OnDataUploaded += msg =>
            //{
            //    lock (buffer)
            //        buffer.Enqueue(DateTime.Now.ToString("[yyMMdd_HHmmss] 데이터 업로드됨") + " - " + msg);
            //};
            //app.filterManager.OnDataProcessed += datas =>
            //{
            //    lock (buffer)
            //        buffer.Enqueue(DateTime.Now.ToString("[yyMMdd_HHmmss] 데이터 처리 완료"));
            //};

            base.OnInitiate();
        }

        Queue<string> buffer = new Queue<string>();

        protected override Task Process()
        {
            lock (buffer)
                while (buffer.Count > 0)
                    Logger.WriteLineAndLog(buffer.Dequeue());

            return Task.CompletedTask;
        }



    }
}
