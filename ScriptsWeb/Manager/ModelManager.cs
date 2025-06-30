using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Onthesys.WebBuild
{
    public class ModelManager : MonoBehaviour, ModelProvider
    {
        #region [Singleton]
        internal static ModelProvider Instance = null;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }
        #endregion [Singleton]

        #region [Instantiating]
        DbManager dbManager;
        UiManager uiManager;

        private void Start()
        {
            //Get Core Components
            dbManager = GetComponent<DbManager>();
            uiManager = GetComponent<UiManager>();

            //Load sensors 
            bool[] isReady = new bool[4] { false, false, false, false };

            Action<int> completion = (callbackIdx) =>
            {
                isReady[callbackIdx] = true;
                if (isReady[0] && isReady[1] && isReady[2] && isReady[3]) { } else return;

                UiManager.Instance.Invoke(UiEventType.ChangeSensorData);
                UiManager.Instance.Invoke(UiEventType.ChangeAlarmLog);
                UiManager.Instance.Invoke(UiEventType.ChangeTrendLine);
                UiManager.Instance.Invoke(UiEventType.ChangeRecentValue);
            };

            dbManager.GetSensorData(sensors => {
                this.sensors.AddRange(sensors);
                completion(0);
            });

            dbManager.GetAlarmLogs(datas =>
            {
                this.alarmLogs.AddRange(datas);
                completion(1);
            });

            DateTime toDt = DateTimeKst.Now;
            DateTime fromDt = DateTimeKst.Now.AddMinutes(-Option.TREND_TIME_RANGE);
            dbManager.GetMeasureLog(fromDt, toDt, measureLogs =>
            {
                this.measureLogs.AddRange(measureLogs);
                groupedMeasureLogs = measureLogs
                    .GroupBy(meas => (meas.board_id, meas.sensor_id))
                    .ToDictionary(g => g.Key, g => g.ToList());
                completion(2);
            });

            dbManager.GetMeasureRecent(measureRecents => {
                this.measureRecents.AddRange(measureRecents);
                completion(3);
            });

            //Register Events

            uiManager.Register(UiEventType.Initiate, OnInitiate);
            uiManager.Register(UiEventType.PopupError, OnPopupError);
            
            uiManager.Register(UiEventType.RequestSearchHistory, OnRequestSearchHistory);
            //uiManager.Register(UiEventType.RequestSearchInference, OnRequestSearchInference);
            //uiManager.Register(UiEventType.RequestSearchDenoised, OnRequestSearchDenoised);

            uiManager.Register(UiEventType.ChangeTimespan, OnChangeTimeSpan);

            uiManager.Register(UiEventType.RequestThresholdUpdate, OnRequestThresholdUpdate);   //추가 0609

            uiManager.Register(UiEventType.RequestVerification, OnVerificationRequest);
            uiManager.Register(UiEventType.RequestSmsUpdate, OnRequestSmsUpdate);
            uiManager.Register(UiEventType.RequestSmsRegister, OnRequestSmsRegister);
            uiManager.Register(UiEventType.RequestSmsUnregister, OnRequestSmsUnregister);
            AwaitInitiating();
        }


        int initTryCount = 0;
        private void AwaitInitiating()
        {
            initTryCount++;
            if (initTryCount > 5)
            {
                UiManager.Instance.Invoke(UiEventType.PopupError,("서버 연결 실패", "DB서버와 연결하는데에 실패했습니다. 문제가 계속 발생할 시, 관리자에게 연락해주십시오."));
                UiManager.Instance.Invoke(UiEventType.PopupError, new Exception("서버 연결에 실패했습니다. 계속 문제가 발생할 시엔 관리자에게 연락하십시오."));
                return;
            }

            bool isInitiated = sensors.Count != 0 && measureRecents.Count != 0 /*&& correlations.Count != 0*/;

            Debug.Log("AwaitInitiating 작동 중 ");
            if (!isInitiated)
                DOVirtual.DelayedCall(1f, AwaitInitiating);
            else
                UiManager.Instance.Invoke(UiEventType.Initiate);
        }

        #endregion [Instantiating]

        #region [EventListener]

        private void OnInitiate(object obj)
        {
            DOVirtual.DelayedCall(Option.TREND_TIME_INTERVAL * 60, GetMeasureRecentProcess).SetLoops(-1);
            DOVirtual.DelayedCall(Option.TREND_TIME_INTERVAL * 60, GetAlarmLogProcess).SetLoops(-1);
            //Main페이지 트렌드 라인 최신화
            DOVirtual.DelayedCall(Option.TREND_TIME_INTERVAL * 60, ()=> OnChangeTimeSpan(isWeek)).SetLoops(-1);
        }

        private void OnPopupError(object obj)
        {
            if (obj is not Exception ex) return;

            Debug.LogError($"{ex.GetType()}\n{ex.Message}\n{ex.StackTrace}");
        }

        private void OnRequestSearchHistory(object obj)
        {
            if (obj is not (int sensorId, DateTime fromDt, DateTime toDt)) return;

            measureHistoryLogs.Clear();
            alarmStats.Clear();

            Action EndJob = () =>
            {
                uiManager.Invoke(UiEventType.ChangeTrendLineHistory);
            };
            int cMax = 3,c = 0;
            Action CheckJob = () => { if (++c >= cMax) EndJob(); };

            dbManager.GetMeasureHistoryTimeRange(1, sensorId, fromDt, toDt, 50, upperLogs =>
            {
                measureHistoryLogs.AddRange(upperLogs);
                CheckJob();
            });

            dbManager.GetMeasureHistoryTimeRange(2, sensorId, fromDt, toDt, 50, lowerLogs =>
            {
                measureHistoryLogs.AddRange(lowerLogs);
                CheckJob();
            });

            dbManager.GetAlarmStatisticTimeRange(fromDt, toDt, stats =>
            {
                alarmStats.AddRange(stats);
                CheckJob();
            });

        }

        private void OnVerificationRequest(object obj) 
        {
            if (obj is not string password) return;

            dbManager.GetCertification(password, result =>
            {
                if (result.is_succeed != true)
                {
                    Debug.LogError($"인증 실패: {result.message}");
                    uiManager.Invoke(UiEventType.ResponseVerification, (result.is_succeed, result.auth_code));
                    return;
                }

                verificatedPassword = password;
                verificatedCode = result.auth_code;

                dbManager.GetSmsServiceList(datas =>
                {
                    smsServices.Clear();
                    smsServices.AddRange(datas);
                    Debug.Log($"SMS 서비스 개수: {(result.is_succeed, result.auth_code)} / ({result.is_succeed}, {result.auth_code})");
                    uiManager.Invoke(UiEventType.ResponseVerification, (result.is_succeed, result.auth_code));
                });
            });
        }

        // 클래스 변수 추가
        List<MeasureModel> measureLogRaw = new();
        bool isWeek = false;
        private void OnChangeTimeSpan(object obj)
        {
            if (obj is not bool isWeek) return;
            this.isWeek = isWeek;

            DateTime toDt = DateTimeKst.Now;
            DateTime fromDt = DateTimeKst.Now.AddDays(isWeek ? -7 : -1);

            // 기존: 트렌드용 평균 데이터
            dbManager.GetMeasureLog(fromDt, toDt, measureLogs => {
                this.measureLogs.Clear();
                this.measureLogs.AddRange(measureLogs);
                groupedMeasureLogs = measureLogs
                    .GroupBy(meas => (meas.board_id, meas.sensor_id))
                    .ToDictionary(g => g.Key, g => g.ToList());

                UiManager.Instance.Invoke(UiEventType.ChangeTrendLine);
            });

            // 새로 추가: 툴팁용 원시 데이터
            dbManager.GetMeasureLogRaw(fromDt, toDt, rawLogs => {
                this.measureLogRaw.Clear();
                this.measureLogRaw.AddRange(rawLogs);
                Debug.Log($"[ModelManager] 원시 데이터 로드됨: {rawLogs.Count}개");
                if (rawLogs.Count > 0)
                {
                    Debug.Log($"[ModelManager] 첫 번째 원시 데이터: {rawLogs[0].MeasuredTime:yyyy-MM-dd HH:mm:ss}");
                    Debug.Log($"[ModelManager] 마지막 원시 데이터: {rawLogs[rawLogs.Count - 1].MeasuredTime:yyyy-MM-dd HH:mm:ss}");
                }
            });

        }

        private void OnRequestSmsUpdate(object obj)
        {
            if(obj is not (int serviceId, SmsServiceModel updatedModel)) return;
            dbManager.GetValidation(verificatedCode, result =>
            {
                if (result.is_succeed == false) 
                {
                    Debug.LogError($"인증 실패: {result.message}");
                    return;
                }

                dbManager.SetSmsServiceUpdate(serviceId, updatedModel, () =>
                {
                    var existingService = smsServices.Find(s => s.service_id == serviceId);
                    if (existingService != null)
                    {
                        existingService.name = updatedModel.name;
                        existingService.phone = updatedModel.phone;
                        existingService.sensor_id = updatedModel.sensor_id;
                        existingService.alarm_level = updatedModel.alarm_level;
                        existingService.is_enabled = updatedModel.is_enabled;
                        existingService.checked_time = updatedModel.checked_time;
                    }

                    UiManager.Instance.Invoke(UiEventType.ResponseSmsUpdate, (result.is_succeed, "수정 완료"));
                    UiManager.Instance.Invoke(UiEventType.PopupError, ("수정 성공", "SMS 서비스가 성공적으로 수정되었습니다."));
                });
            });
        }     
        private void OnRequestSmsRegister(object obj)
        {
            if (obj is not SmsServiceModel newModel) return;

            dbManager.SetSmsServiceCreate(newModel, model => {
                smsServices.Add(model);
                UiManager.Instance.Invoke(UiEventType.ResponseSmsRegister, (true, "성공적으로 수정되었습니다."));
                UiManager.Instance.Invoke(UiEventType.PopupError, ("추가 성공", "SMS 서비스가 성공적으로 추가되었습니다."));
            });
        }
        private void OnRequestSmsUnregister(object obj) 
        {
            if (obj is not List<int> serviceIds) return;

            Action EndJob = () =>
            {
                smsServices.RemoveAll(service => serviceIds.Contains(service.service_id));
                UiManager.Instance.Invoke(UiEventType.ResponseSmsUnregister, (true, "성공적으로 삭제되었습니다."));
                UiManager.Instance.Invoke(UiEventType.PopupError, ("삭제 성공", "SMS 서비스가 성공적으로 삭제되었습니다."));
            };
            int cMax = serviceIds.Count();
            int c = 0; Action CheckJob = () => { if (++c >= cMax) EndJob(); };

            foreach (var serviceId in serviceIds)
                dbManager.SetSmsServiceDelete(serviceId, CheckJob);
        }

        //0609 수정
        private void OnRequestThresholdUpdate(object obj)
         {
             if (obj is not List<SensorModel> updatedSensors) return;

             Debug.Log($"임계값 업데이트 요청: {updatedSensors.Count}개 센서");

             dbManager.UpdateSensorThresholds(updatedSensors, (isSuccess, message) =>
             {
                 if (!isSuccess)
                 {
                     uiManager.Invoke(UiEventType.ResponseThresholdUpdate, (false, message));
                     return;
                 }

                //로컬 센서 데이터 업데이트
                foreach (var updatedSensor in updatedSensors)
                {
                    var localSensors = sensors.FindAll(s =>
                        s.sensor_id == updatedSensor.sensor_id);

                    foreach (var localSensor in localSensors)
                        if (localSensor != null)
                        {
                            localSensor.threshold_warning = updatedSensor.threshold_warning;
                            localSensor.threshold_serious = updatedSensor.threshold_serious;
                            localSensor.is_fixing = updatedSensor.isFixing;
                        }
                }

                //UI에 성공 알림
                uiManager.Invoke(UiEventType.ResponseThresholdUpdate, (true, "임계값이 성공적으로 저장되었습니다."));

                //센서 데이터 변경 알림
                uiManager.Invoke(UiEventType.ChangeSensorData);
             });
         }
        #endregion [EventListener]

        #region [Process]
        void GetMeasureRecentProcess()
        {
            //Load measureRecents 
            dbManager.GetMeasureRecent(measureRecents => {
                this.measureRecents.Clear();
                this.measureRecents.AddRange(measureRecents);

                UiManager.Instance.Invoke(UiEventType.ChangeRecentValue);

                //메모리 누수로 인해 SetLoop()로 수정
                //Recursive Call
                //DOVirtual.DelayedCall(Option.TREND_TIME_INTERVAL * 60, GetMeasureRecentProcess);
            });
        }

        void GetAlarmLogProcess() 
        {
            //Load alarmLogs 
            dbManager.GetAlarmLogs(datas =>
            {
                this.alarmLogs.Clear();
                this.alarmLogs.AddRange(datas);
                uiManager.Invoke(UiEventType.ChangeAlarmLog);

                //메모리 누수로 인해 SetLoop()로 수정
                //Recursive Call
                //DOVirtual.DelayedCall(Option.TREND_TIME_INTERVAL * 60, GetAlarmLogProcess);
            });
        }


        #endregion [Process]


        #region [DataStructs]
        /// <summary>
        /// 센서 제원
        /// </summary>
        List<SensorModel> sensors = new();
        /// <summary>
        /// 최근 측정값
        /// </summary>
        List<MeasureModel> measureRecents = new();
        /// <summary>
        /// 메인 화면 트렌드에서 표시하기 위한 측정값들
        /// </summary>
        List<MeasureModel> measureLogs = new();

        Dictionary<(int boardId, int sensorId), List<MeasureModel>> groupedMeasureLogs = new();
        /// <summary>
        /// 기록 조회에서 표시하기 위한 측정값들
        /// </summary>
        List<MeasureModel> measureHistoryLogs = new();
        /// <summary>
        /// 임계치를 넘어 활성화된 알람들
        /// </summary>
        List<AlarmLogModel> alarmLogs = new();

        /// <summary>
        /// SMS 서비스 이용자 리스트
        /// </summary>
        List<SmsServiceModel> smsServices = new();

        /// <summary>
        /// 알람 통계 정보들
        /// </summary>
        List<AlarmStatisticModel> alarmStats = new();
        /// <summary>
        /// AI 추론 결과를 조회하기 위한 값들
        /// </summary>
        //List<MeasureModel> measureInference = new();
        /// <summary>
        /// 노이즈 필터링된 값들
        /// </summary>
        //List<MeasureModel> measureDenoised = new();
        /// <summary>
        /// 상관관계 값들
        /// </summary>
        //List<CorrelationModel> correlations = new();

        string verificatedPassword = "", verificatedCode = "";

        #endregion [Datastructs]

        #region [ModelProvider]
        public List<MeasureModel> GetMeasureLogRaw() => new(measureLogRaw); //0630

        public List<MeasureModel> GetMeasureLogBySensor(int boardId, int sensorId)
            => groupedMeasureLogs.TryGetValue((boardId, sensorId), out var measures) ? measures : new List<MeasureModel>();

        public List<MeasureModel> GetMeasureLogList() => measureLogs;

        public MeasureModel GetMeasureRecentBySensor(int boardId, int sensorId)
            => measureRecents.Find(meas => meas.board_id == boardId && meas.sensor_id == sensorId);

        public List<MeasureModel> GetMeasureRecentList() => new(measureRecents);

        public SensorModel GetSensor(int boardId, int sensorId)
            => sensors.Find(sen => sen.board_id == boardId && sen.sensor_id == sensorId);

        public List<SensorModel> GetSensors() => new(sensors);

        public StatusType GetStatusBySensor(int boardId, int sensorId)
        {
            var foundSensor = sensors.Find(item => item.board_id == boardId && item.sensor_id == sensorId);
            if (foundSensor is null) return StatusType.ERROR;

            float value = GetMeasureRecentBySensor(boardId, sensorId)?.measured_value ?? -1;

            //delta로 넘어오면서 0이하의 값은 오히려 정상이됨. 치명 Status 제외 적용
            //if (0f > value) return StatusType.ERROR;
            //if (foundSensor.threshold_critical < value) return StatusType.CRITICAL;
            if (foundSensor.threshold_warning < value) return StatusType.WARNING;
            if (foundSensor.threshold_serious < value) return StatusType.SERIOUS;
            return StatusType.NORMAL;
        }
        public StatusType GetStatusBySensorAndValue(int boardId, int sensorId, float value)
        {
            var foundSensor = sensors.Find(item => item.board_id == boardId && item.sensor_id == sensorId);
            if (foundSensor is null) return StatusType.ERROR;

            //delta로 넘어오면서 0이하의 값은 오히려 정상이됨. 치명 Status 제외 적용
            //if (0f > value) return StatusType.ERROR;
            //if (foundSensor.threshold_critical < value) return StatusType.CRITICAL;
            if (foundSensor.threshold_warning < value) return StatusType.WARNING;
            if (foundSensor.threshold_serious < value) return StatusType.SERIOUS;
            return StatusType.NORMAL;
        }

        public List<MeasureModel> GetMeasureHistoryList() => new(measureHistoryLogs);

        public List<AlarmLogModel> GetAlarmLogList() => new(alarmLogs);

        public List<SmsServiceModel> GetSmsServices() => new(smsServices);

        public SmsServiceModel GetSmsServiceById(int serviceId) => smsServices.Find(service => service.service_id == serviceId);
        public List<AlarmStatisticModel> GetAlarmStatistics() => new(alarmStats);

        //public List<CorrelationModel> GetCorrelations() => correlations;

        //public List<CorrelationModel> GetCorrelationBySensor(int boardId, int sensorId) => correlations.Where(item => item.base_sensor_name == GetSensor(boardId,sensorId).sensor_name).ToList();


        //public List<MeasureModel> GetMeasureInferenceList() => measureInference;
        //public List<MeasureModel> GetMeasureDenoisedList()=> measureDenoised;
        #endregion [ModelProvider]




    }
}
