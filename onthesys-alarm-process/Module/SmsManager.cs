using DMXOS;
using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace onthesys_alarm_process.Process
{
    public class SmsManager : Manager
    {
        public event Action OnSmsSended;        //SMS 전송
        public event Action<List<AlarmLogModel>, StatusType, StatusType> OnAlarmOccured;     //알람 발생
        /// <summary>
        /// 알람 해소 이벤트.
        ///  List<AlarmLogModel>해소된  = 알람 목록
        ///  StatusType from > StatusType to
        /// </summary>
        public event Action<List<AlarmLogModel>,StatusType, StatusType> OnAlarmSolved;     //알람 해소
        public event Action<List<MeasureModel>> OnSensorDataReceived;
        public event Action<List<SmsServiceModel>> OnSmsChecked;

        bool isCdmaOn = true; //CDMA 활성화 여부
        ISmsHandle smsHandle;

        List<SensorModel> sensors = new List<SensorModel>();     //센서 제원 
        Dictionary<int, List<MeasureModel>> filteredDatas = new Dictionary<int, List<MeasureModel>>();  //sensorId에 따라 필터링된 데이터
        List<AlarmLogModel> alarmLogs = new List<AlarmLogModel>();   //기존 알람
        List<SmsServiceModel> smsServices  = new List<SmsServiceModel>();    //SMS 서비스

        internal SmsManager(Application app, ISmsHandle smsHandle) : base(app)
        {
            this.smsHandle = smsHandle;
            interval = 10*60*1000;
        }
        protected override Task Process()
        {
            try
            {
                while (sensors.Count == 0)
                    Thread.Sleep(100);


                List<MeasureModel> models = WaterQualityData();
                AlarmProcess(models);

            }
            catch (Exception e) 
            {
               Logger.WriteLineAndLog("" + e.Message +"\n"+ e.StackTrace);
            }
                //Thread.Sleep(100);
            return Task.CompletedTask;
        }


        #region [이벤트]
        protected override void OnInitiate()
        {
            app.dbManager.OnSensorsDownloaded += sensors => this.sensors = sensors.Where(s => s.board_id == 1 && s.sensor_id <= 3).ToList();
            app.dbManager.OnSensorsDownloaded += sensors =>
            {
                sensors.ForEach(sensor =>
                { 
                    if (!filteredDatas.ContainsKey(sensor.sensor_id))
                        filteredDatas.Add(sensor.sensor_id, new List<MeasureModel>());
                });
            };
            app.dbManager.OnAlarmDownloaded += alarmLogs => this.alarmLogs = alarmLogs;
            app.dbManager.OnSmsServicesDownloaded += smsServices => this.smsServices = smsServices;
            //app.filterManager.OnDataProcessed += OnDataProcessed;
            OnAlarmOccured += OnAlarmOccuredSendSms;
            OnAlarmSolved += OnAlarmSolvedSendSms;

            base.OnInitiate();
        }


        //데이터 처리 후, 데이터 수령
        private void OnDataProcessed(List<MeasureModel> list)
        {
            if (list.Count == 0) throw new Exception("OnDataProcessed - No data to process. data length is 0");
            Logger.WriteLineAndLog($"SmsManager - OnDataProcessed - Processed Data Recieved (sid : {list.First().sensor_id})({list.Count} ea)");
            filteredDatas[list.First().sensor_id] = list;
        }
        //알람 발생 시, SMS 전송
        void OnAlarmOccuredSendSms(List<AlarmLogModel> alarmLogs, StatusType from, StatusType to)
        {
            if (alarmLogs.Count == 0)
            {
                Logger.WriteLineAndLog("OnAlarmOccured - No data to process. data is null");
                return;
            }
            var alarm = alarmLogs.Last();

            //일단 상류만 가져올 듯
            SensorModel sensor = sensors.FirstOrDefault(item =>  item.sensor_id == alarm.sensor_id);

            //해당 센서의 알람인지?
            List<SmsServiceModel> tServices = smsServices.Where(item => item.sensor_id == alarm.sensor_id).ToList();
            //임계 등급 이상의 알람인지?
            tServices = tServices.Where(item => item.GetAlarmLevel() <= alarm.GetAlarmLevel()).ToList();
            //유효한 전화번호를 갖고 활성화된 상태인지?
            tServices = tServices.Where(item => item.is_enabled && IsValidPhoneNumber(item.phone)).ToList();
            //전화번호 추출
            List<string> phoneNumbers = tServices.Select(s => s.phone).ToList();

            float threshold = sensor.GetThresholdByStatus(alarm.GetAlarmLevel());

            string groupMessage = $"[{sensor.sensor_name}] Status rise: {from} => {to} at {alarm.occured_time:HH:mm:ss}, over {threshold} {sensor.unit}";

            smsHandle.SendSMSToList(phoneNumbers, groupMessage);
            OnSmsSended?.Invoke();
        }

        private void OnAlarmSolvedSendSms(List<AlarmLogModel> alarmLogs, StatusType from, StatusType to)
        {
            if (alarmLogs.Count == 0)
            {
                Logger.WriteLineAndLog("OnAlarmSolved - No data to process. data is null");
                return;
            }
            var alarm = alarmLogs.Last();

            //일단 상류만 가져올 듯
            SensorModel sensor = sensors.FirstOrDefault(item => item.sensor_id == alarm.sensor_id);

            //해당 센서의 알람인지?
            List<SmsServiceModel> tServices = smsServices.Where(item => item.sensor_id == alarm.sensor_id).ToList();
            //임계 등급 이상의 알람인지?
            tServices = tServices.Where(item => item.GetAlarmLevel() <= alarm.GetAlarmLevel()).ToList();
            //유효한 전화번호를 갖고 활성화된 상태인지?
            tServices = tServices.Where(item => item.is_enabled && IsValidPhoneNumber(item.phone)).ToList();
            //전화번호 추출
            List<string> phoneNumbers = tServices.Select(s => s.phone).ToList();

            float threshold = sensor.GetThresholdByStatus(alarm.GetAlarmLevel());
            string groupMessage = $"[{sensor.sensor_name}] Status drop: {from} => {to} at {alarm.solved_time:HH:mm:ss}, below {threshold} {sensor.unit}";

            smsHandle.SendSMSToList(phoneNumbers, groupMessage);
            OnSmsSended?.Invoke();
        }




        bool IsValidPhoneNumber(string phone)
        {
            phone = phone.Replace("-", "");
            phone = phone.Replace(".", "");
            phone = phone.Replace(" ", "");
            if (phone.All(char.IsDigit) && phone.Length == 11) 
                return true;
            
            return false;
        }

        #endregion

        #region [알람 제어]
        private void AlarmProcess(List<MeasureModel> models) 
        {
            if (!isCdmaOn) throw new Exception("SMS Manager - AlarmProcess - cdma is not ready");

            //foreach (var sensor in sensors)
            //    if (filteredDatas[sensor.sensor_id].Count == 0) throw new Exception("SMS Manager - AlarmProcess - Data not yet provided");//데이터가 아직 준비되지 않음

            List<AlarmLogModel> newAlarmLogs = new List<AlarmLogModel>();
            foreach (var sensor in sensors.Where(s => new int[] { 1, 2, 3 }.Contains(s.sensor_id)))
            {
                if (sensor.isFixing)
                {
                    Logger.WriteLineAndLog($"[SmsManager] - ({sensor.sensor_name}) - Sensor is fixing, skip alarm process");
                    continue;
                }

                MeasureModel upper = models.Find(m => m.sensor_id == sensor.sensor_id && m.board_id == 1);
                MeasureModel lower = models.Find(m => m.sensor_id == sensor.sensor_id && m.board_id == 2);

                if(lower == null || upper == null)
                {
                    Logger.WriteLineAndLog($"[SmsManager] - ({sensor.sensor_name}) - No data for sensor {sensor.sensor_id} (upper: {upper?.measured_value}, lower: {lower?.measured_value})");
                    continue;
                }

                MeasureModel differ = new MeasureModel()
                {
                    sensor_id = sensor.sensor_id,
                    board_id = 1,
                    measured_value = lower.measured_value - upper.measured_value,
                    measured_time = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss")
                };

                StatusType status = EstimateAlarms(sensor, differ);
                List<AlarmLogModel> logs = ValidateAlarms(sensor.sensor_id, status);
                newAlarmLogs.AddRange(logs);

                Logger.WriteLineAndLog($"[SmsManager] - ({sensor.sensor_name}) = {status.ToString()}");
            }

            this.alarmLogs = newAlarmLogs;
            smsServices.ForEach(sms => sms.checked_time = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd HH:mm:ss"));
            OnSmsChecked?.Invoke(smsServices);
        }
        
        //센서의 데이터 > 센서의 현재 상태
        StatusType EstimateAlarms(SensorModel sensor, MeasureModel data)
        {
            //---패딩의 영향을 받지 않아 외곡이 없는 지점을 선택---
            //int tIdx = data.Count - 1/* - FilterManager.filterLatestIndex*/;
            float latestValue = data.measured_value;

            Logger.WriteLineAndLog($"[{sensor.sensor_name}] latestValue : " + latestValue);
            //if (latestValue > sensor.threshold_critical)
            //    return StatusType.CRITICAL;
            //else
            if(latestValue > sensor.threshold_warning)
                return StatusType.WARNING;
            else if(latestValue > sensor.threshold_serious)
                return StatusType.SERIOUS;
            else
                return StatusType.NORMAL;

            return StatusType.ERROR;
        }

        //센서의 현재 상태 > 알람 발생?
        List<AlarmLogModel> ValidateAlarms (int sensorId, StatusType status)
        {
            List<AlarmLogModel> alarmLogs = this.alarmLogs.Where(x => x.sensor_id == sensorId && x.solved_time == null).ToList();

            AlarmLogModel prevWarn = alarmLogs.Find(alarm => alarm.GetAlarmLevel() == StatusType.WARNING);
            AlarmLogModel prevSeri = alarmLogs.Find(alarm => alarm.GetAlarmLevel() == StatusType.SERIOUS);

            List<AlarmLogModel> items = null;
            switch (status) 
            {
                case StatusType.NORMAL:
                    if (prevSeri != null)
                    {
                        prevSeri.solved_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        //WARNING > NORMAL
                        if (prevWarn != null)
                        {
                            prevWarn.solved_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            items = new List<AlarmLogModel>() { prevSeri, prevWarn };
                            OnAlarmSolved?.Invoke(items, StatusType.WARNING, StatusType.NORMAL);
                        }
                        //SERIOUS > NORMAL
                        else
                        {
                            items = new List<AlarmLogModel>() { prevSeri};
                            OnAlarmSolved?.Invoke(items, StatusType.SERIOUS, StatusType.NORMAL);
                        }
                    }
                break;
                case StatusType.SERIOUS:
                    //WARNING > SERIOUS
                    if (prevWarn != null)
                    {
                        prevWarn.solved_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        items = new List<AlarmLogModel>() { prevWarn };
                        OnAlarmSolved?.Invoke(items, StatusType.WARNING, StatusType.SERIOUS);
                    }
                    //NORMAL > SERIOUS
                    if (prevSeri == null)
                    {
                        AlarmLogModel newSeri = new AlarmLogModel()
                        {
                            alarm_id = -1,
                            sensor_id = sensorId,
                            alarm_level = StatusType.SERIOUS.ToStringAsStatus(),
                            occured_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            solved_time = null,
                        };

                        items = new List<AlarmLogModel>() { newSeri };
                        alarmLogs.Add(newSeri);
                        OnAlarmOccured?.Invoke(items, StatusType.NORMAL, StatusType.SERIOUS);
                    }
                    break;
                case StatusType.WARNING:

                    if (prevWarn == null)
                    {
                        AlarmLogModel newWarn = new AlarmLogModel()
                        {
                            alarm_id = -1,
                            sensor_id = sensorId,
                            alarm_level = StatusType.WARNING.ToStringAsStatus(),
                            occured_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            solved_time = null,
                        };

                        //NORMAL > WARNING
                        if (prevSeri == null)
                        {
                            AlarmLogModel newSeri = new AlarmLogModel()
                            {
                                alarm_id = -1,
                                sensor_id = sensorId,
                                alarm_level = StatusType.SERIOUS.ToStringAsStatus(),
                                occured_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                solved_time = null,
                            };
                            items = new List<AlarmLogModel>() { newSeri, newWarn };
                            alarmLogs.Add(newWarn);
                            alarmLogs.Add(newSeri);

                            OnAlarmOccured?.Invoke(items, StatusType.NORMAL, StatusType.WARNING);
                        }
                        //SERIOUS > WARNING
                        else
                        {
                            items = new List<AlarmLogModel>() { newWarn };
                            alarmLogs.Add(newWarn);
                            OnAlarmOccured?.Invoke(items, StatusType.SERIOUS, StatusType.WARNING);
                        }
                    }
                    break;
                default:
                    Logger.WriteLineAndLog("ValidateAlarms - 예상 범위 밖의 인자가 제시됐습니다.처리 할 수 없는 알람 유형입니다");
                    break;
            }


            return alarmLogs;
        }
        #endregion

        #region [데이터 수신]
        //수질 데이터 수신 및 처리
        List<MeasureModel> WaterQualityData()
        {
            List<MeasureModel> models = new List<MeasureModel>();
            foreach (DEV_WQ_POS position in Enum.GetValues(typeof(DEV_WQ_POS)))
            {
                List<WQ_Item> data = new List<WQ_Item>();

                //아마 동기 방식으로 작동하는 것으로 보임
                if (smsHandle.SendGetCurrentValue(position, ref data))
                {
                    Logger.WriteLineAndLog($"Water Quality Data - {position}: {data.Count} items");

                    //TODO CDMA 활성화 여부를 결정하는 코드
                    //isCdmaOn = data[0];

                    if (isCdmaOn == false) continue;

                    // WQ_Item → MeasureModel 변환
                    var measureData = ConvertToMeasureModels(data, position, DateTime.Now);

                    // DbManager로 이벤트 발생
                    OnSensorDataReceived?.Invoke(measureData);
                    models.AddRange(measureData);
                }
                else
                    Logger.WriteLineAndLog($"Water Quality Data - {position}: Failed to get data from CDMA!");

            }
            Logger.WriteLineAndLog($"Water Quality Data - gethering ends");

            return models;
        }
        // WQ_Item 리스트를 MeasureModel 리스트로 변환
        List<MeasureModel> ConvertToMeasureModels(List<WQ_Item> wqItems, DEV_WQ_POS position, DateTime dateTime)
        {
            int boardId = position == DEV_WQ_POS.UPPER ? 1 : 2;
            var measureModels = new List<MeasureModel>();

            for (int i = 0; i < wqItems.Count; i++)
            {
                var wqItem = wqItems[i];
                if (!wqItem.Timeout)
                {
                    measureModels.Add(new MeasureModel
                    {
                        board_id = boardId,
                        sensor_id = i + 1,
                        measured_value = wqItem.PV,
                        measured_time = dateTime.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
                else
                    Logger.WriteLineAndLog($"Water Quality Data - {position}-{i + 1}: Timeout error from CDMA!");
            }

            return measureModels;
        }
        #endregion

    }
}
