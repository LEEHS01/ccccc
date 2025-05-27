using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Networking;
namespace Onthesys.WebBuild
{
    public class DbManager : MonoBehaviour
    {
        internal static DbManager instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            instance = this;

            GetMeasureLogFunc(DateTime.Now, DateTime.Now, items => items.ForEach(item => 
                Debug.Log($"[{item.board_id} - {item.sensor_id}][{item.measured_time.ToString()}] : {item.measured_value}")
            ));

        }

        #region [공개된 인터페이스]
        /// <summary>
        /// 센서 제원을 가져오는 함수입니다.
        /// </summary>
        /// <param name="callback"></param>
        internal void GetSensorData(Action<List<SensorModel>> callback)
            => StartCoroutine(GetSensorDataFunc(callback));
        /// <summary>
        /// 기록된 계측값들을 시간범위를 지정해 가져오는 함수입니다.
        /// 트렌드라인 초기화에 사용됩니다.
        /// </summary>
        /// <param name="fromDt"></param>
        /// <param name="toDt"></param>
        /// <param name="callback"></param>
        internal void GetMeasureLog(DateTime fromDt, DateTime toDt, Action<List<MeasureModel>> callback)
            => StartCoroutine(GetMeasureLogFunc(fromDt, toDt, callback));
        /// <summary>
        /// 현재(가장 최근) 계측값들을 가져오는 함수입니다.
        /// </summary>
        /// <param name="callback"></param>
        internal void GetMeasureRecent(Action<List<MeasureModel>> callback)
            => StartCoroutine(GetMeasureRecentFunc(callback));
        /// <summary>
        /// 특정 센서가 가진 기록된 계측값들을 시간범위를 지정해 정해진 갯수만큼 가져오는 함수입니다. 
        /// 현재는 각 범위별 첫번째 값을 가져오지만, 후에는 평균값을 가져오도록 수정할 예정입니다.
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="sensorId"></param>
        /// <param name="fromDt"></param>
        /// <param name="toDt"></param>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        internal void GetMeasureHistoryTimeRange(int boardId, int sensorId, DateTime fromDt, DateTime toDt, int count, Action<List<MeasureModel>> callback)
            => StartCoroutine(GetMeasureHistoryFunc(boardId, sensorId, fromDt, toDt, count, callback));
        /// <summary>
        /// 현재 활성화된 알람 로그들을 가져오는 함수입니다.
        /// </summary>
        /// <param name="callback"></param>
        internal void GetAlarmLogs(Action<List<AlarmLogModel>> callback)
            => StartCoroutine(GetAlarmLogsFunc(callback));

        /// <summary>
        /// 상관관계 그리드 데이터를 가져오는 함수입니다.
        /// </summary>
        /// <param name="callback"></param>
        internal void GetCorrelations(Action<List<CorrelationModel>> callback)
            => StartCoroutine(GetCorrelationsFunc(callback));
        /// <summary>
        /// AI를 통해 추론된 측정값을 가져오는 함수입니다.
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="sensorId"></param>
        /// <param name="fromDt"></param>
        /// <param name="toDt"></param>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        internal void GetMeasureInferenceTimeRange(int boardId, int sensorId, DateTime fromDt, DateTime toDt, int count, Action<List<MeasureModel>> callback)
            => StartCoroutine(GetMeasureInferenceTimeRangeFunc(boardId, sensorId, fromDt, toDt, count, callback));
        /// <summary>
        /// 노이즈 제거를 통해 추출된 경향을 가져오는 함수입니다.
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="sensorId"></param>
        /// <param name="fromDt"></param>
        /// <param name="toDt"></param>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        internal void GetMeasureDenoiseTimeRange(int boardId, int sensorId, DateTime fromDt, DateTime toDt, int count, Action<List<MeasureModel>> callback)
            => StartCoroutine(GetMeasureDenoiseTimeRangeFunc(boardId, sensorId, fromDt, toDt, count, callback));
        /// <summary>
        /// SMS 서비스 목록을 가져오는 함수입니다.
        /// </summary>
        /// <param name="callback"></param>
        internal void GetSmsServiceList(Action<List<SmsServiceModel>> callback)
            => StartCoroutine(GetSmsServiceListFunc(callback));
        /// <summary>
        /// SMS 서비스 정보를 업데이트하는 함수입니다.
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="newModel"></param>
        /// <param name="callback"></param>
        internal void SetSmsServiceUpdate(int serviceId, SmsServiceModel updatedModel, Action callback)
            => StartCoroutine(SetSmsServiceUpdateFunc(serviceId, updatedModel, callback));
        /// <summary>
        /// SMS 서비스 정보를 생성하는 함수입니다.
        /// </summary>
        /// <param name="updatedModel"></param>
        /// <param name="callback"></param>
        internal void SetSmsServiceCreate(SmsServiceModel newModel, Action<SmsServiceModel> callback)
            => StartCoroutine(SetSmsServiceCreateFunc(newModel, callback));
        /// <summary>
        /// SMS 서비스 정보를 삭제하는 함수입니다.
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="callback"></param>
        internal void SetSmsServiceDelete(int serviceId, Action callback)
            => StartCoroutine(SetSmsServiceDeleteFunc(serviceId, callback));

        #endregion

        #region [DB 요청 및 처리문]
        IEnumerator GetSensorDataFunc(Action<List<SensorModel>> callback)
        {
            var query = @"SELECT * FROM WEB_DP.dbo.sensor";

            //Debug.Log("Get Func" + query);
            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                //Debug.Log("GetSensorDataFunc : " + result);
                var wrapper = JsonUtility.FromJson<SensorModelList>(result);
                callback(wrapper.items);
            });
        }
        IEnumerator GetMeasureLogFunc(DateTime fromDt, DateTime toDt, Action<List<MeasureModel>> callback)
        {
            //fromDt = toDt.AddMinutes(-5);
            var query = $@"EXEC GET_MEASURE_TIME_RANGE_WHOLE
                @table_name = 'measure_denoise',
                @start_time = '{fromDt:yyyy-MM-dd HH:mm:ss}',
                @end_time = '{toDt:yyyy-MM-dd HH:mm:ss}',
                @element_count = 26,
                @default_value = 0.0;";
            
            //Debug.Log("Get Func" + query);
            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                Debug.Log("GetMeasureLogFunc : " + result);
                var wrapper = JsonUtility.FromJson<MeasureModelList>(result);
                callback(wrapper.items);
            });
        }
        IEnumerator GetMeasureRecentFunc(Action<List<MeasureModel>> callback)
        {
            var query = @"SELECT * FROM WEB_DP.dbo.measure_recent";

            //Debug.Log("Get Func" + query);
            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                //Debug.Log("GetMeasureRecent : " + result);
                var wrapper = JsonUtility.FromJson<MeasureModelList>(result);
                callback(wrapper.items);
            });
        }
        IEnumerator GetMeasureHistoryFunc(int boardId, int sensorId, DateTime fromDt, DateTime toDt, int count, Action<List<MeasureModel>> callback)
        {

            var query = @$"EXEC WEB_DP.dbo.GET_MEASURE_TIME_RANGE_SENSOR 
                @table_name = 'measure_log',
                @board_id = {boardId},
                @sensor_id = {sensorId},
                @start_time = '{fromDt:yyyy-MM-dd HH:mm:ss}',
                @end_time = '{toDt:yyyy-MM-dd HH:mm:ss}',
                @element_count = {count},
                @default_value = 0.0;
            ";

            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                //Debug.Logr("GetMeasureHistoryFunc : " + result);
                var wrapper = JsonUtility.FromJson<MeasureModelList>(result);
                callback(wrapper.items);
            });
        }
        IEnumerator GetAlarmLogsFunc(Action<List<AlarmLogModel>> callback)
        {
            var query = @$"select * from WEB_DP.dbo.Alarm_Log where solved_time is null";

            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                //Debug.LogWarning("GetAlarmLogsFunc : " + result);
                var wrapper = JsonUtility.FromJson<AlarmLogModelList>(result);
                callback(wrapper.items);
            });

        }

        IEnumerator GetCorrelationsFunc(Action<List<CorrelationModel>> callback)
        {
            var query = @$"exec GET_CORRELATIONS;";

            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                //Debug.Logr("GetMeasureHistoryFunc : " + result);
                var wrapper = JsonUtility.FromJson<CorrelationModelList>(result);
                callback(wrapper.items);
            });
        }



        IEnumerator GetMeasureInferenceTimeRangeFunc(int boardId, int sensorId, DateTime fromDt, DateTime toDt, int count, Action<List<MeasureModel>> callback)
        {

            var query = @$"EXEC WEB_DP.dbo.GET_MEASURE_TIME_RANGE_SENSOR
                @table_name = 'measure_inference',
                @board_id = {boardId},
                @sensor_id = {sensorId},
                @start_time = '{fromDt:yyyy-MM-dd HH:mm:ss}',
                @end_time = '{toDt:yyyy-MM-dd HH:mm:ss}',
                @element_count = {count},
                @default_value = 0.0;
            ";

            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                //Debug.Logr("GetMeasureHistoryFunc : " + result);
                var wrapper = JsonUtility.FromJson<MeasureModelList>(result);
                callback(wrapper.items);
            });
        }
        IEnumerator GetMeasureDenoiseTimeRangeFunc(int boardId, int sensorId, DateTime fromDt, DateTime toDt, int count, Action<List<MeasureModel>> callback)
        {

            var query = @$"EXEC WEB_DP.dbo.GET_MEASURE_TIME_RANGE_SENSOR 
                @table_name = 'measure_denoise',
                @board_id = {boardId},
                @sensor_id = {sensorId},
                @start_time = '{fromDt:yyyy-MM-dd HH:mm:ss}',
                @end_time = '{toDt:yyyy-MM-dd HH:mm:ss}',
                @element_count = {count},
                @default_value = 0.0;
            ";

            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                //Debug.Logr("GetMeasureHistoryFunc : " + result);
                var wrapper = JsonUtility.FromJson<MeasureModelList>(result);
                callback(wrapper.items);
            });
        }
        IEnumerator GetSmsServiceListFunc(Action<List<SmsServiceModel>> callback)
        {
            var query = @"SELECT * FROM WEB_DP.dbo.sms_service";

            //Debug.Log("Get Func" + query);
            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                //Debug.Log("GetSensorDataFunc : " + result);
                var wrapper = JsonUtility.FromJson<SmsServiceModelList>(result);
                callback(wrapper.items);
            });
        }
        IEnumerator SetSmsServiceUpdateFunc(int serviceId, SmsServiceModel updatedModel, Action callback)
        {
            var query = $@"EXEC Update_SMS_Service 
                @service_id = {serviceId},
                @name = '{updatedModel.name}',
                @phone = '{updatedModel.phone}',
                @is_enabled = {updatedModel.is_enabled},
                @alarm_level = '{updatedModel.alarm_level}';";

            //Debug.Log("Get Func" + query);
            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                Debug.Log("SetSmsServiceUpdateFunc : " + result);
                callback();
            });

        }
        IEnumerator SetSmsServiceCreateFunc(SmsServiceModel newModel, Action<SmsServiceModel> callback)
        {

            var query = $@"EXEC Insert_SMS_Service  
                @name = '{newModel.name}',
                @phone = '{newModel.phone}',
                @is_enabled = {newModel.is_enabled},
                @alarm_level = '{newModel.alarm_level}';";

            //Debug.Log("Get Func" + query);
            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                Debug.Log("SetSmsServiceUpdateFunc : " + result);
                var item = JsonUtility.FromJson<SmsServiceModelList>(result);
                callback(item.items.First());
            });


        }
        IEnumerator SetSmsServiceDeleteFunc(int serviceId, Action callback)
        {

            var query = $@"EXEC DELETE_SMS_Service  
                @service_id = '{serviceId}';";

            //Debug.Log("Get Func" + query);
            yield return ResponseQuery(QueryType.SELECT.ToString(), query, result =>
            {
                Debug.Log("SetSmsServiceUpdateFunc : " + result);
                callback();
            });


        }

        #endregion


        /// <summary>
        /// DB서버를 연결해주는 API서버에 쿼리문을 전달한 뒤, 응답을 전달받는 함수입니다.
        /// </summary>
        /// <param name="type">쿼리 유형입니다. QueryType.SELECT.ToString() 같은 방식으로 사용합니다.</param>
        /// <param name="query">쿼리문 내용입니다.</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        IEnumerator ResponseQuery(string type, string query, System.Action<string> callback)
        {
            var data = new QueryPayload
            {
                SQLType = type,
                SQLquery = query
            };

            var json = JsonUtility.ToJson(data);
            // JSON 데이터를 바이트 배열로 변환
            byte[] jsonToSend = new UTF8Encoding().GetBytes(json);

            // UnityWebRequest를 POST 메서드로 생성
            UnityWebRequest request = new UnityWebRequest(Option.URL, "POST");
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 요청 보내기
            yield return request.SendWebRequest();

            // 응답 처리
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[QUERY] : " + query + "\n[RECIEVED] : " + request.downloadHandler.text);
                // 요청 성공 시 응답 본문 출력
                callback(request.downloadHandler.text);
            }
            else
            {
                // 오류 처리
                callback($"Error: {request.error}");
            }
        }

        IEnumerator ResponseAPI(string type, string route, string msg, System.Action<string> callback) 
        {
            throw new NotImplementedException();
        }


        enum QueryType
        {
            SELECT,
            UPDATE
        }
        [Serializable]
        public class QueryPayload
        {
            public string SQLType;
            public string SQLquery;
        }
    }
}
