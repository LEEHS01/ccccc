using Newtonsoft.Json.Linq;
using Onthesys.WebBuild;
using onthesys_alarm_process.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onthesys_alarm_process.Process
{
    public class FilterManager : Manager
    {

        public const int windowSize = 11; // 필터 윈도우 크기
        public static int thresholdIndex => (windowSize-1)/2; // 임계값 인덱스 (윈도우 크기 / 2, 홀수여야 함)

        public event Action<List<MeasureModel>> OnDataProcessed;    //데이터 처리 종료

        private FirFilter fir;

        public FilterManager(Application app) : base(app) 
        {
            fir = new FirFilter(windowSize, 20f, 120);
        }

        protected override void OnInitiate()
        {
            app.dbManager.OnDataDownloaded += OnDataDownloaded;
            base.OnInitiate();
        }

        protected override void Process() { }

        void OnDataDownloaded(List<MeasureModel> datas)
        {
            float[] input = datas.Select(d => d.measured_value).ToArray();
            float[] output = fir.Apply(input);
            //Console.WriteLine("Raw Data: " + string.Join(", ", input));
            //Console.WriteLine("Filtered Data: " + string.Join(", ", output));

            List<MeasureModel> outputModels = new List<MeasureModel>();
            for (int i = 0; i < output.Length; i++) 
            {
                var inputModel = datas[i];
                var outputModel = new MeasureModel()
                {
                    sensor_id = inputModel.sensor_id,
                    board_id =  inputModel.board_id,
                    measured_value = output[i],
                    measured_time = inputModel.measured_time,
                };
                outputModels.Add(outputModel);
            }

            outputModels.ForEach(model => {
                string dt = $"{model.MeasuredTime:yyyy-MM-dd HH:mm:ss}";
                model.measured_time = dt.Substring(0, dt.Length - 1) + "0";
            });
            var deduplicated = outputModels
                .GroupBy(m => m.measured_time + m.sensor_id + m.board_id )
                .Select(g => g.First())
                .ToList();

            OnDataProcessed.Invoke(outputModels);
        }

    }
}
