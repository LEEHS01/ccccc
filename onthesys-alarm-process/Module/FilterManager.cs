using Newtonsoft.Json.Linq;
using Onthesys.WebBuild;
using onthesys_alarm_process.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace onthesys_alarm_process.Process
{



    public class FilterManager : Manager
    {
        //const int winSize = 11;
        //public static int filterLatestIndex = (winSize - 1) / 2;
        public event Action<List<MeasureModel>> OnDataProcessed;    //데이터 처리 종료

        //private FirFilter fir;

        public FilterManager(Application app) : base(app) 
        {
            //fir = new FirFilter(winSize, 20f, 120);
        }

        protected override void OnInitiate()
        {
            //app.dbManager.OnDataDownloaded += OnDataDownloaded;
            base.OnInitiate();
        }

        protected override Task Process() => Task.CompletedTask;


        void OnDataDownloaded(List<MeasureModel> sensorData) 
        {
            List<MeasureModel> outputModels = new List<MeasureModel>();

            foreach (int sensorId in new int[] { 1, 2, 3 })
            {
                MeasureModel upper = sensorData.FirstOrDefault(d => d.sensor_id == sensorId && d.board_id == 1);
                MeasureModel lower = sensorData.FirstOrDefault(d => d.sensor_id == sensorId && d.board_id == 2);

                if (upper == null && lower == null)
                {
                    Logger.WriteLineAndLog($"[FilterManager] Data is null");
                    return;
                }

                if (upper.measured_value * lower.measured_value == 0f)
                {
                    Logger.WriteLineAndLog("[FilterManager] upper or lower Value is 0f! it has possibility to data not defined. so, put a moratorium on decise alarm.");
                    return;
                }

                outputModels.Add(new MeasureModel() {
                    measured_time = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss"),
                    sensor_id = upper.sensor_id,
                    board_id = upper.board_id, // 사실 상 의미 없는 값
                    measured_value = lower.measured_value - upper.measured_value
                });
            }
            //OnDataProcessed?.Invoke(outputModels);
        }


        void OnDataDownloadedLegacy(List<MeasureModel> upperData, List<MeasureModel> lowerData)
        {
            //추출
            List<float> upperInput = upperData.Select(d => d.measured_value).ToList();
            List<float> lowerInput = lowerData.Select(d => d.measured_value).ToList();

            if (upperInput.Count != lowerInput.Count) 
            {
                Logger.WriteLineAndLog($"[FilterManager] Data count mismatch: Upper({upperInput.Count}) != Lower({lowerInput.Count})");
                return;
            }

            if(upperInput.Last() * lowerInput.Last() == 0f)
            {
                Logger.WriteLineAndLog("[FilterManager] upper or lower Value is 0f! it has possibility to data not defined. so, put a moratorium on decise alarm.");
                return;
            }


            //차 계산 한쪽이 0이라면 데이터를 읽어오는데 실패한 것이므로 -1로 처리해 알람 처리를 방지
            List<float> diff = upperInput
                .Zip(lowerInput, (u, l) => l - u)
                .ToList();

            // FIR 필터 적용
            //List<float> output = fir.ApplyFilter(diff);
            //Logger.WriteLineAndLog("Raw Data: " + string.Join(", ", input));
            //Logger.WriteLineAndLog("Filtered Data: " + string.Join(", ", output));

            // 결과 모델 생성
            List<MeasureModel> outputModels = new List<MeasureModel>();
            for (int i = 0; i < diff.Count; i++) 
            {
                MeasureModel inputModel = upperData[i];
                var outputModel = new MeasureModel()
                {
                    sensor_id = inputModel.sensor_id,
                    board_id =  inputModel.board_id,    //사실 상 의미 없는 값
                    measured_value = diff[i],
                    measured_time = inputModel.measured_time,
                };
                outputModels.Add(outputModel);
            }

            //outputModels.ForEach(model => {
            //    string dt = $"{model.MeasuredTime:yyyy-MM-dd HH:mm:ss}";
            //    model.measured_time = dt.Substring(0, dt.Length - 1) + "0";
            //});
            //var deduplicated = outputModels
            //    .GroupBy(m => m.measured_time + m.sensor_id + m.board_id )
            //    .Select(g => g.First())
            //    .ToList();

            //반환
            OnDataProcessed?.Invoke(outputModels);
        }

    }
}
