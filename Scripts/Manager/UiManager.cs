using Newtonsoft.Json;
using NUnit.Framework;
using Onthesys;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace Onthesys.ExeBuild
{
    internal class UiManager : MonoBehaviour
    {
        //테스트용 코드. 가짜 보드3 데이터를 입력하기 위한 조치로 DB에 해당 데이터 입력이 들어온다면
        //internal ModelProvider modelProvider => ModelManager.Instance;
        //위와 같이 복구할 예정
        internal ModelProvider mp = null;
        internal ModelProvider modelProvider
        {
            get
            {
                if (mp == null) mp = new ModelProviderTest(ModelManager.Instance);
                return mp;
            }
        }

        internal static UiManager Instance = null;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (!Option.ENABLE_DEBUG_CODE) return;
            //DEBUG
            if (Input.GetKeyDown(KeyCode.F1))
            {
                this.Invoke(UiEventType.ChangeAlarmList, new List<LogData>(){
                new(1, 1, "인천", "능내리", 5, "0", DateTime.Now, 0, 105f, -1, 100, 80),
                new(1, 1, "인천", "능내리", 5, "1", DateTime.Now, 2, 105f, -1, 100, 80),
                new(1, 1, "인천", "능내리", 5, "2", DateTime.Now, 1, 105f, -1, 100, 80),
            });
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                this.Invoke(UiEventType.ChangeAlarmList, new List<LogData>(){
                new(3, 2, "인천", "능내리", 5, "클로로포름", DateTime.Now, 1, 105f, -1, 100, 80),
            });

            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                this.Invoke(UiEventType.ChangeAlarmList, new List<LogData>(){
                new(5, 2, "인천", "능내리", 5, "클로로포름", DateTime.Now, 2, 105f, -1, 100, 80),
            });
            }


            if (Input.GetKeyDown(KeyCode.F4))
            {
                StartCoroutine(ResponseAPIString("SELECT", @"INSERT INTO TB_ALARM_DATA
                (HNSIDX, OBSIDX, BOARDIDX, ALAHIVAL, ALAHIHIVAL, CURRVAL, ALACODE, ALADT, TURNOFF_FLAG, TURNOFF_DT)
            VALUES
                (2, 10, 2, 500, 1000, 110.0, 2, GETDATE(), NULL, NULL);
            ", str => Debug.Log("API : 새로운 로그 생성 성공")));
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                StartCoroutine(ResponseAPIString("SELECT", @"SELECT TOP 1 *
                FROM TB_ALARM_DATA
                ORDER BY ALADT DESC;
            ", str => Debug.Log(str)));
            }
            if (Input.GetKeyDown(KeyCode.F6))
            {
                StartCoroutine(ResponseAPIString("SELECT", @"DELETE FROM TB_ALARM_DATA
                WHERE ALAIDX = (
                    SELECT TOP 1 ALAIDX
                    FROM TB_ALARM_DATA
                    ORDER BY ALADT DESC
                );
            ", str => Debug.Log("API : 새로운 로그 삭제 성공 (WARNING! 해제가 아니기에 UI에 반응 없음)")));
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                StartCoroutine(ResponseAPIString("SELECT", @"
                UPDATE TB_ALARM_DATA
                SET 
                    TURNOFF_FLAG = 'Y',
                    TURNOFF_DT = GETDATE()
                WHERE ALAIDX = (
                    SELECT TOP 1 ALAIDX
                    FROM TB_ALARM_DATA
                    ORDER BY ALADT DESC
                );
            ", str => Debug.Log("API : 가장 최근 알람 해제 완료")));
            }
            if (Input.GetKeyDown(KeyCode.F8))
            {
                StartCoroutine(ResponseAPIString("SELECT", @"IF OBJECT_ID('GET_UPDATED_ALARM_LOG', 'P') IS NOT NULL
    PRINT 'GET_UPDATED_ALARM_LOG 프로시저가 존재합니다.';
                ;
            ", str => Debug.Log(str)));
            }


            if (Input.GetKeyDown(KeyCode.Q)) Invoke(UiEventType.ChangeSensorStep, 1);
            if (Input.GetKeyDown(KeyCode.W)) Invoke(UiEventType.ChangeSensorStep, 2);
            if (Input.GetKeyDown(KeyCode.E)) Invoke(UiEventType.ChangeSensorStep, 3);
            if (Input.GetKeyDown(KeyCode.R)) Invoke(UiEventType.ChangeSensorStep, 4);
            if (Input.GetKeyDown(KeyCode.T)) Invoke(UiEventType.ChangeSensorStep, 5);
        }

        private Dictionary<UiEventType, Action<object>> eventHandlers = new();
        internal void Register(UiEventType eventType, Action<object> handler)
        {
            if (!eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType] = handler;
            }
            else
            {
                eventHandlers[eventType] += handler;
            }
        }

        internal void Unregister(UiEventType eventType, Action<object> handler)
        {
            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType] -= handler;
            }
        }

        internal void Invoke(UiEventType eventType, object payload = null)
        {
            if (eventHandlers.ContainsKey(eventType))
            {
                List<Delegate> delegates = eventHandlers[eventType]?.GetInvocationList().ToList();

                delegates.ForEach(del =>
                {
                    try
                    {
                        del.DynamicInvoke(payload);
                    }
                    catch (Exception ex)
                    {
                        //Debug.LogError($"UiManager - Invoke {ex.GetType()} : {ex.Message}");
                        //재귀 방지
                        if (eventType == UiEventType.PopupError)
                        {
                            Debug.LogError($"UiManager - Invoke 내부 오류 : eventType({eventType}) ({ex.InnerException.GetType()}) : {ex.InnerException.Message}");
                            return;
                        }
                        Invoke(UiEventType.PopupError, ex);
                    }
                });

                //eventHandlers[eventType]?.Invoke(payload);
            }
        }

        IEnumerator ResponseAPIString(string type, string query, System.Action<string> callback)
        {
            var data = new
            {
                SQLType = type,
                SQLquery = query
            };
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            // JSON 데이터를 바이트 배열로 변환
            byte[] jsonToSend = new UTF8Encoding().GetBytes(json);

            // UnityWebRequest를 POST 메서드로 생성
            UnityWebRequest request = new UnityWebRequest(Option.url, "POST");
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 요청 보내기
            yield return request.SendWebRequest();

            // 응답 처리
            if (request.result == UnityWebRequest.Result.Success)
            {
                // 요청 성공 시 응답 본문 출력
                callback(request.downloadHandler.text);
            }
            else
            {
                // 오류 처리
                callback($"Error: {request.error}");
            }
        }
    }
    internal enum UiEventType
    {
        Initiate,       //주요 컨트롤 객체들의 초기화 완료

        NavigateHome,   //시작 화면으로 이동
        NavigateArea,   //지역 화면으로 이동
        NavigateObs,    //관측소 화면으로 이동

        SelectAlarm,        //알람 선택
        SelectAlarmSensor,  //알람 내 센서 선택
        SelectCurrentSensor,    //센서 모니터링 중 센서 선택
        SelectCctv,     //관측소 내의 Cctv 선택
        SelectSettingObs,   //환경설정 창 내에서 관측소 선택

        Popup_AiTrend,          //독성+유해물질 상세 팝업(AI값)


        ChangeTrendLine,    //실시간 트렌드 갱신
        ChangeSummary,    //연간 지역 알람 요약본 업데이트 (기존 OnChangeSummary)
        ChangeAlarmList,            //알람리스트 변동 발생 (기존 OnAlarmUpdated)
        ChangeSensorList,           //센서리스트 변동 발생 (기존 OnLoadSetting)
        ChangeSettingSensorList,    //관측소 센서 리스트에 변동 발생
        ChangeAlarmSensorList,  //과거 알람 센서리스트에 변동 발생
        ChangeSensorStatus, //센서리스트 내 상태 변동 발생 (기존 OnToxinStatusChange)
        ChangeAlarmMonthly, //알람 월간 상위5개 변동 발생
        ChangeAlarmYearly,  //알람 연간 상위5개 변동 발생
        ChangeAlarmMap,     //지역별 알람 현황 변동 발생
        ChangeSensorStep,   //센서 진행단계 변동 발생

        CommitSensorUsing, //환경설정 - 센서 표시 변경
        CommitBoardFixing,      //환경설정 - 보드 수정 변경
        CommitCctvUrl,          //환경설정 - Cctv URL 변경
        CommitPopupAlarmCondition,      //환경설정 - 팝업 알람 조건 변경

        PopupMachineInfo,  //기계 제원 패널 표시
        PopupSetting,  //환경설정 패널 표시
        PopupError,     //에러 발생

    }
}