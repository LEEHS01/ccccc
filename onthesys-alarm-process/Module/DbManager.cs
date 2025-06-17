using DMXOS;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Onthesys.WebBuild;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static onthesys_alarm_process.Process.UiManager;

namespace onthesys_alarm_process.Process
{
    public class DbManager : Manager
    {
        #region 이벤트 리스트
        public event Action<List<MeasureModel>, List<MeasureModel>> OnDataDownloaded;   //데이터 다운로드
        public event Action<List<AlarmLogModel>> OnAlarmDownloaded;   //알람 다운로드
        public event Action<List<SensorModel>> OnSensorsDownloaded;   //센서 제원 다운로드
        public event Action<List<SmsServiceModel>> OnSmsServicesDownloaded;   //SMS 서비스 다운로드
        
        public event Action<string> OnDataUploaded;     //데이터 업로드

        public const string url = "http://192.168.10.236:8080";
        #endregion


        public DbManager(Application app) : base(app)
        {
            interval = 50*60*1000;
        }
        protected override void OnInitiate()
        {
            base.OnInitiate();
            //app.filterManager.OnDataProcessed += RequestUploadMeasureDenoise;
            app.smsManager.OnAlarmSolved += RequestUpdateAlarm;
            app.smsManager.OnAlarmOccured += RequestInsertAlarm;
            app.smsManager.OnSensorDataReceived += RequestUploadMeasureRecent;
            app.smsManager.OnSmsChecked += RequestSetSmsCheckedTime;

            new Thread(() =>
            {
                Thread.Sleep(1000);
                RequestRefreshDatas();
            }).Start();
        }

        protected override Task Process()
        {
            try
            {
                RequestMeasureLogBySensor(1);
                RequestMeasureLogBySensor(2);
                RequestMeasureLogBySensor(3);

                RequestRefreshDatas();

                RequestMakeMeasureLogFromRecent();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " +  ex);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 수신받은 계측값을 DB에 업로드하는 함수
        /// </summary>
        /// <param name="datas"></param>
        void RequestUploadMeasureRecent(List<MeasureModel> datas)
        {
            if (datas.Count == 0) return;

            //datas.ForEach(model =>
            //    model.measured_time = model.MeasuredTime.AddHours(-9).ToString("yyyy-MM-dd HH:mm:ss"));

            //result.ForEach(model => model.measured_time = $"{model.MeasuredTime.AddHours(9):yyyy-MM-dd HH:mm:ss}");
            string jsonData = JsonConvert.SerializeObject(datas);

            string query = $@"EXEC WEB_DP.dbo.UPSERT_MEASURE_recent @json = '{jsonData}';";

            ResponseAPIString("SELECT", query).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine("Error: " + t.Exception.InnerException.Message);
                }
                else
                {
                    OnDataUploaded?.Invoke($"measure_recent : {datas.Count} 개");
                }
            });
        }

        /// <summary>
        /// 센서 제원, 알람 로그, SMS 서비스 초기화
        /// </summary>
        void RequestRefreshDatas()
        {
            string query;

            try
            {
                //센서 제원 초기화
                query = $@"Select * from sensor;";
                ResponseAPIString("SELECT", query).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine("Error: " + t.Exception.InnerException.Message);
                    }
                    else
                    {
                        //Console.WriteLine("Result: " + t.Result);
                        var wrapper = JsonConvert.DeserializeObject<SensorModelList>(t.Result);
                        var result = wrapper.items;
                        OnSensorsDownloaded?.Invoke(result);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex);
            }

            try
            {
                //활성화된 알람들로 초기화
                query = $@"select * from alarm_log where solved_time IS NULL;";
                ResponseAPIString("SELECT", query).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine("Error: " + t.Exception.InnerException.Message);
                    }
                    else
                    {
                        //Console.WriteLine("Result: " + t.Result);
                        var wrapper = JsonConvert.DeserializeObject<AlarmLogModelList>(t.Result);
                        var result = wrapper.items;
                        OnAlarmDownloaded?.Invoke(result);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex);
            }

            try
            {
                //sms 서비스 초기화 
                query = $@"select * from sms_service where is_enabled = 1;";
                ResponseAPIString("SELECT", query).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine("Error: " + t.Exception.InnerException.Message);
                    }
                    else
                    {
                        //Console.WriteLine("Result: " + t.Result);
                        var wrapper = JsonConvert.DeserializeObject<SmsServiceModelList>(t.Result);
                        var result = wrapper.items;
                        OnSmsServicesDownloaded?.Invoke(result);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex);
            }
        }
        
        /// <summary>
        /// 센서별 측정 로그 요청
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="sensorId"></param>
        void RequestMeasureLogBySensor(int sensorId)
        {
            DateTime startTime = DateTime.Now.AddSeconds(-30);
            string upperQuery = $@"EXEC WEB_DP.dbo.GET_MEASURE_TIME_RANGE_SENSOR
                @table_name = 'measure_log',
                @board_id = {1},
                @sensor_id = {sensorId},
                @start_time = '{startTime.AddMinutes(-30):yyyy-MM-dd HH:mm:ss}',
                @end_time = '{startTime:yyyy-MM-dd HH:mm:ss}',
                @element_count = {120},
                @default_value = 0.0;";

            string lowerQuery = $@"EXEC WEB_DP.dbo.GET_MEASURE_TIME_RANGE_SENSOR
                @table_name = 'measure_log',
                @board_id = {2},
                @sensor_id = {sensorId},
                @start_time = '{startTime.AddMinutes(-30):yyyy-MM-dd HH:mm:ss}',
                @end_time = '{startTime:yyyy-MM-dd HH:mm:ss}',
                @element_count = {120},
                @default_value = 0.0;";

            //쿼리 결과를 측정 로그로 파싱한 후, 누락된 데이터 구간들을 보간하여 반환하는 함수
            Func<Task<string>, List<MeasureModel>> parseAndLerpingMeasures = t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine("Error: " + t.Exception.InnerException.Message);
                }
                else
                {
                    //Console.WriteLine("Result: " + t.Result);
                    var wrapper = JsonConvert.DeserializeObject<MeasureModelList>(t.Result);
                    var result = wrapper.items;

                    //누락된 데이터 구간들을 양측 값으로 보간함
                    for (int i = 0; i < result.Count; i++)
                    {
                        if (result[i].measured_value == 0)
                        {
                            if (i == 0)
                                result[i].measured_value = result[i + 1].measured_value;
                            else if (i == result.Count - 1)
                            {
                                result[i].measured_value = result[i - 1].measured_value;
                            }
                            else
                            {
                                int j = 1;
                                while (i + j < result.Count)
                                {
                                    if (result[i + j].measured_value == 0f)
                                    {
                                        j++;
                                        continue;
                                    }
                                    result[i].measured_value = (result[i - 1].measured_value + result[i + j].measured_value) / 2f;
                                    break;
                                }
                            }
                        }
                    }

                    return result;
                }
                return null;
            };

            List<MeasureModel> uppperMeasures = new List<MeasureModel>(), lowerMeasures = new List<MeasureModel>();

            try
            {
                Task upperTask = ResponseAPIString("SELECT", upperQuery).ContinueWith(parseAndLerpingMeasures).ContinueWith(items => uppperMeasures.AddRange(items.Result));
                Task lowerTask = ResponseAPIString("SELECT", lowerQuery).ContinueWith(parseAndLerpingMeasures).ContinueWith(items => lowerMeasures.AddRange(items.Result));

                upperTask.Wait();
                lowerTask.Wait();

                //데이터 로그 다운로드
                OnDataDownloaded?.Invoke(uppperMeasures, lowerMeasures);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex);
            }
        }

        /// <summary>
        /// 알람 로그 수정 요청
        /// </summary>
        /// <param name="alarmLog"></param>
        void RequestUpdateAlarm(List<AlarmLogModel> alarmLogs, StatusType from, StatusType to) 
        {
            foreach (var alarmLog in alarmLogs)
            {
                string query = $@"Update alarm_log
                SET solved_time = '{alarmLog.solved_time/*():yyyy-MM-dd HH:mm:ss*/}'
                where alarm_id = {alarmLog.alarm_id};";
                ResponseAPIString("SELECT", query).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine("Error: " + t.Exception.InnerException.Message);
                    }
                    else
                    {
                        OnDataUploaded?.Invoke($"Solve Alarm : [{alarmLog.alarm_id}] : {alarmLog.GetAlarmLevel().ToString()}");
                    }
                });
            }
        }

        /// <summary>
        /// 알람 로그 삽입 요청
        /// </summary>
        /// <param name="alarmLog"></param>
        void RequestInsertAlarm(List<AlarmLogModel> alarmLogs, StatusType from, StatusType to)
        {
            foreach (var alarmLog in alarmLogs)
            {
                string query = $@"insert into alarm_log(sensor_id, alarm_level, occured_time, solved_time)
                values(
                {alarmLog.sensor_id},
                '{alarmLog.GetAlarmLevel().ToStringAsStatus()}',
                '{alarmLog.OccuredTime:yyyy-MM-dd HH:mm:ss}',
                NULL);";
                ResponseAPIString("SELECT", query).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine("Error: " + t.Exception.InnerException.Message);
                    }
                    else
                    {
                        OnDataUploaded?.Invoke($"Occured Alarm : [{alarmLog.sensor_id}] : {alarmLog.GetAlarmLevel().ToString()}");
                    }
                });
            }
            }

        /// <summary>
        /// 측정 로그를 최근으로 설정하는 요청
        /// </summary>
        void RequestMakeMeasureLogFromRecent()
        {
            string query = $@"exec UPSERT_MEASURE_LOG_FROM_RECENT;";
            ResponseAPIString("SELECT", query).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine("Error: " + t.Exception.InnerException.Message);
                }
                else
                {
                    OnDataUploaded?.Invoke($"made measureLog as recent");
                }
            });
            
        }

        void RequestSetSmsCheckedTime(List<SmsServiceModel> services)
        {
            string jsonData = JsonConvert.SerializeObject(services);
            string query = $@"EXEC UPDATE_SMS_CHECKED @json = '{jsonData}';";

            ResponseAPIString("SELECT", query).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine("Error: " + t.Exception.InnerException.Message);
                }
                else
                {
                    OnDataUploaded?.Invoke($"SET SMS CHECKED TIME : {services.Count}");
                }
            });
        }


        #region 기본 통신 코드
        static async Task<string> ResponseAPIString(string type, string query)
        {
            var data = new QueryPayload
            {
                SQLType = type,
                SQLquery = query
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                try
                {
                    string url = DbManager.url + "/query/";
                    var response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode(); // 예외 throw if not 2xx

                    string responseBody = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine("[QUERY] : " + query + "\n[RECEIVED] : " + responseBody);
                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                    return $"Error: {e.Message}";
                }
            }
        }
        class QueryPayload
        {
            public string SQLType { get; set; }
            public string SQLquery { get; set; }
        }
        #endregion
    }
}
