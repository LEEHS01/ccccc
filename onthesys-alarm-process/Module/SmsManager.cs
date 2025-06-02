using DMXOS;
using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace onthesys_alarm_process.Process
{
    public class SmsManager : Manager
    {
        public event Action OnSmsSended;        //SMS 전송
        public event Action<AlarmLogModel> OnAlarmOccured;     //알람 발생
        public event Action<AlarmLogModel> OnAlarmSolved;     //알람 발생

        internal ISMSHandle smsHandle;

        public List<SensorModel> sensors = new List<SensorModel>();     //센서 제원 
        public Dictionary<(int boardId, int sensorId), List<MeasureModel>> filteredDatas 
            = new Dictionary<(int boardId, int sensorId), List<MeasureModel>>();  //필터링된 데이터
        public List<AlarmLogModel> alarmLogs = new List<AlarmLogModel>();   //기존 알람
        public List<SmsServiceModel> smsServices  = new List<SmsServiceModel>();    //SMS 서비스

        internal SmsManager(Application app, ISMSHandle smsHandle) : base(app)
        {
            this.smsHandle = smsHandle;
            interval = 15000;
        }

        protected override void OnInitiate()
        {
            app.dbManager.OnSensorsDownloaded += sensors => this.sensors = sensors;
            app.dbManager.OnSensorsDownloaded += sensors =>
            {
                if (!filteredDatas.ContainsKey((sensors.First().board_id, sensors.First().sensor_id)))
                    filteredDatas.Add((sensors.First().board_id, sensors.First().sensor_id), new List<MeasureModel>());
            };
            app.dbManager.OnAlarmDownloaded += alarmLogs => this.alarmLogs = alarmLogs;
            app.dbManager.OnSmsServicesDownloaded += smsServices => this.smsServices = smsServices;
            app.filterManager.OnDataProcessed += OnDataProcessed;
            OnAlarmOccured += OnAlarmOccuredSendSms;

            base.OnInitiate();
        }

        //이벤트 시, 데이터 수령
        private void OnDataProcessed(List<MeasureModel> list)
        {
            if (list.Count == 0) throw new Exception("OnDataProcessed - No data to process. data length is 0");
            (int boardId, int sensorId) address = (list.First().board_id, list.First().sensor_id);

            filteredDatas[address] = list;
        }

        protected override void Process()
        {
            List<AlarmLogModel> newAlarmLogs = new List<AlarmLogModel>();
            foreach (var sensor in sensors)
            {
                if (!sensor.isUsing || sensor.isFixing) continue;
                
                StatusType status = EstimateAlarms(sensor, filteredDatas[(sensor.board_id, sensor.sensor_id)]);
                List<AlarmLogModel> logs = ValidateAlarms((sensor.board_id, sensor.sensor_id), status);
                newAlarmLogs.AddRange(logs);
                Console.WriteLine($"[SmsManager] - ({sensor.board_id},{sensor.sensor_id}) = {status.ToString()}");
            }

            this.alarmLogs = newAlarmLogs;

            CollectWaterQualityData();
        }

        // test
        void CollectWaterQualityData()
        {
            List<WQ_Item> upperData = new List<WQ_Item>();
            if (app.smsManager.smsHandle.SendGetCurrentValue(DEV_WQ_POS.UPPER, ref upperData))
            {
                Console.WriteLine($"Water Quality Data Collected - UPPER: {upperData.Count} items");
            }

            List<WQ_Item> lowerData = new List<WQ_Item>();
            if (app.smsManager.smsHandle.SendGetCurrentValue(DEV_WQ_POS.LOWER, ref lowerData))
            {
                Console.WriteLine($"Water Quality Data Collected - LOWER: {lowerData.Count} items");
            }
        }

        //센서의 데이터 > 센서의 현재 상태
        StatusType EstimateAlarms(SensorModel sensor ,List<MeasureModel> data)
        {
            //패딩이 포함되지 않는 위치를 골라 임계값으로 사용
            float latestValue = data[data.Count - FilterManager.windowSize].measured_value;
            if (latestValue > sensor.threshold_critical)
                return StatusType.CRITICAL;
            else if(latestValue > sensor.threshold_warning)
                return StatusType.WARNING;
            else if(latestValue > sensor.threshold_serious)
                return StatusType.SERIOUS;
            else
                return StatusType.NORMAL;

            return StatusType.ERROR;
        }

        //센서의 현재 상태 > 알람 발생?
        List<AlarmLogModel> ValidateAlarms ((int boardId, int sensorId) address, StatusType status)
        {
            List<AlarmLogModel> alarmLogs = this.alarmLogs.Where(x => x.board_id == address.boardId && x.sensor_id == address.sensorId && x.solved_time == null).ToList();

            for (int i = 0; i < alarmLogs.Count; i++)
            {
                //변화 확인
                AlarmLogModel alarmLog = alarmLogs[i];
                if (alarmLog == null && status == StatusType.NORMAL) continue;
                if (alarmLog != null && status == alarmLog.GetAlarmLevel()) continue;

                //변화 발생 및 새로운 알람인지 확인
                StatusType previousStatus = alarmLog.GetAlarmLevel();
                bool isAlarmLevelUp = previousStatus < status;
                bool isNewStatement = alarmLogs.Where(item => item.GetAlarmLevel() == status).Count() == 0;

                //일림 상승에 대한 처리 : 새 알람을 추가
                if (isAlarmLevelUp && isNewStatement)
                {
                    AlarmLogModel newAlarm = new AlarmLogModel()
                    {
                        alarm_id = -1,
                        board_id = address.boardId,
                        sensor_id = address.sensorId,
                        alarm_level = status.ToStringAsStatus(),
                        occured_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        solved_time = null,
                    };

                    OnAlarmOccured?.Invoke(newAlarm);

                    alarmLogs.Add(newAlarm);

                }

                //알람 해제에 대한 처리 : 해소된 알람을 수정
                if (!isAlarmLevelUp && previousStatus > status)
                {
                    alarmLog.solved_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    OnAlarmSolved?.Invoke(alarmLog);
                }
            }

            if (alarmLogs.Count == 0 && status != StatusType.NORMAL)
            {
                AlarmLogModel newAlarm = new AlarmLogModel()
                {
                    alarm_id = -1,
                    board_id = address.boardId,
                    sensor_id = address.sensorId,
                    alarm_level = status.ToStringAsStatus(),
                    occured_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    solved_time = null,
                };

                OnAlarmOccured?.Invoke(newAlarm);

                alarmLogs.Add(newAlarm);
            }

            return alarmLogs;
        }

        //알람 발생 시, SMS 전송
        void OnAlarmOccuredSendSms(AlarmLogModel alarm)
        {
            Console.WriteLine($"{alarm.alarm_id}\t {alarm.occured_time:yyyy-MM-dd HH:mm:ss}\t {alarm.GetAlarmLevel().ToStringAsStatus()}\t{alarm.board_id}-{alarm.sensor_id}");

            if (alarm == null) throw new Exception("OnAlarmOccured - No data to process. data length is 0");

            SensorModel sensor = sensors.FirstOrDefault(item => item.board_id == alarm.board_id && item.sensor_id == alarm.sensor_id);

            List<SmsServiceModel> tServices = smsServices.Where(item => item.GetAlarmLevel() <= alarm.GetAlarmLevel()).ToList();

            if (tServices.Count == 0) {
                Console.WriteLine("OnAlarmOccured - No SMS service found.");
                return; 
            }
            foreach (var smsService in tServices)
            {
                string message = $"[{alarm.alarm_level}] {smsService.name}님, {alarm.occured_time} 부로 {sensor.sensor_name}에 알람 발생했습니다.";

                if (smsService.phone == null) throw new Exception("OnAlarmOccured - No phone number found.");

                smsHandle.SendSMSToOne(smsService.phone, message);
                //OnSmsSended?.Invoke();
            }
        }

    }
}
