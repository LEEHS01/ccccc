using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace onthesys_alarm_process.Process
{
    public abstract class Manager
    {
        /// <summary>
        /// Process() 반복 주기
        /// </summary>
        protected int interval = 1000; // 1초

        /// <summary>
        /// 스레드 반복 여부
        /// </summary>
        protected bool isRunning = true;
        /// <summary>
        /// 스레드 객체
        /// </summary>
        protected Thread thread;
        /// <summary>
        /// Application 객체
        /// </summary>
        protected Application app;

        internal Manager(Application app)
        {
            this.app = app;
            app.OnInitiating += OnInitiate;
        }

        protected virtual void OnInitiate() 
        {
            thread = new Thread(ProcessFunc);
            thread.IsBackground = true;
            thread.Start();
        }

        private void ProcessFunc()
        {
            while (isRunning) 
            {
                Thread.Sleep(interval);
                Process();
            }
        }

        /// <summary>
        /// 매니저 객체가 주기적으로 반복할 함수.
        /// </summary>
        protected abstract void Process();

        /// <summary>
        /// Application에 종료 요청을 보냄.
        /// </summary>
        public void Quit()
        {
            app.IsQuiting = true;
            isRunning = false;
        }
    }
}
