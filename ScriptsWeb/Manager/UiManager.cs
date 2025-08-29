using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Onthesys.WebBuild
{
    internal class UiManager : MonoBehaviour
    {
        internal ModelProvider modelProvider => ModelManager.Instance as ModelProvider;

        public static UiManager Instance = null;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
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
                        for (; ex.InnerException != null;)
                            ex = ex.InnerException;

                        //Debug.LogError($"UiManager - Invoke {ex.GetType()} : {ex.Message}");
                        //재귀 방지
                        if (eventType == UiEventType.PopupError)
                        {

                            Debug.LogError($"UiManager - Invoke 내부 오류 : eventType({eventType}) ({ex.GetType()}) : {ex.Message}");
                            return;
                        }
                        Invoke(UiEventType.PopupError, ex);
                    }
                });

                //eventHandlers[eventType]?.Invoke(payload);
            }
        }
    }

    internal enum UiEventType
    {
        //초기화 등 시스템 이벤트
        Initiate,
        PopupError,
        
        //센서 제원 관련
        ChangeSensorData,

        //실시간 조회 관련
        ChangeTrendLine,
        ChangeRecentValue,

        //현재 알람 로드
        ChangeAlarmLog,

        //기록 조회 관련
        ChangeTrendLineHistory,
        RequestSearchHistory,

        ////추론 조회 관련
        //ChangeTrendLineInference,
        //RequestSearchInference,
        
        ////경향 조회 관련
        //ChangeTrendLineDenoised,
        //RequestSearchDenoised,

        //탭 내 센서 선택
        SelectSensorWithinTab,

        //메인화면 트랜드 출력간격 변경
        ChangeTimespan,
        
        //페이지 이동
        NavigateSms,
        NavigateMain,


        //인스펙터 할당 및 해제
        InspectorApply,
        InspectorRelease,
        
        //SMS 서비스 관련
        RequestVerification,
        ResponseVerification,
        RequestThresholdUpdate,     //추가

        ChangeSmsServiceList,

        RequestSmsRegister,
        ResponseSmsRegister,
        ResponseSmsUnregister,
        RequestSmsUnregister,
        ResponseSmsUpdate,
        RequestSmsUpdate,
        RequestPasswordChange,      // 비밀번호 변경 요청
        ResponsePasswordChange,     // 비밀번호 변경 응답


        //임계값 관련
        ResponseThresholdUpdate,
    }
}
