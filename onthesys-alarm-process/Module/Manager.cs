using System;
using System.Threading;
using System.Threading.Tasks;

namespace onthesys_alarm_process.Process
{
    public abstract class Manager : IDisposable
    {
        protected int interval = 1000; // ms 단위 주기
        protected readonly Application app;

        private CancellationTokenSource cts;
        private Task loopTask;

        internal Manager(Application app)
        {
            this.app = app;
            app.OnInitiating += OnInitiate;
        }

        protected virtual void OnInitiate()
        {
            cts = new CancellationTokenSource();
            loopTask = Task.Run(() => ProcessLoopAsync(cts.Token));
        }

        private async Task ProcessLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Process(); // async가 아니면 Task.Run(() => Process())로 래핑
                    await Task.Delay(interval, token);
                }
            }
            catch (TaskCanceledException)
            {
                // 정상 종료
            }
            catch (Exception ex)
            {
                Logger.WriteLineAndLog($"{ex} {ex.Source} {ex.Message} {ex.StackTrace} ");
                // 로깅 또는 예외처리
            }
        }

        protected abstract Task Process();

        public void Quit()
        {
            app.IsQuiting = true;
            cts?.Cancel();
        }

        public void Dispose()
        {
            Quit();
            loopTask?.Wait(); // 안전하게 종료 기다림
            cts?.Dispose();
            app.OnInitiating -= OnInitiate;
        }
    }
}
