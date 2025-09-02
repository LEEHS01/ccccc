using Onthesys.WebBuild;
using UnityEngine;
using TMPro;
using System.Linq;

public class IndicatorStatusDisplay : MonoBehaviour
{
    [Header("텍스트 UI 참조")]
    public GameObject txtInfo;  // 컨테이너 역할하는 txtInfo 오브젝트
    public TextMeshProUGUI deviationText;      // "편차:" 텍스트
    public TextMeshProUGUI deviationValueText;  // Text (TMP) (1) - 편차 값
    public TextMeshProUGUI statusLabelText;     // Text (TMP) (2) - "경계 기준:" 또는 "경보 기준:"
    public TextMeshProUGUI thresholdValueText;  // Text (TMP) (3) - 임계값

    [Header("센서 설정")]
    public int sensorId;

    void Start()
    {
        // 초기화 시 텍스트 비우기
        HideAllTexts();

        // 서버 알람로그 업데이트 이벤트 등록
        if (UiManager.Instance != null)
        {
            UiManager.Instance.Register(UiEventType.ChangeAlarmLog, OnAlarmLogChanged);
            UiManager.Instance.Register(UiEventType.ChangeRecentValue, OnRecentValueChanged);
        }
    }

    void OnDestroy()
    {
        // 이벤트 등록 해제
        if (UiManager.Instance != null)
        {
            UiManager.Instance.Unregister(UiEventType.ChangeAlarmLog, OnAlarmLogChanged);
            UiManager.Instance.Unregister(UiEventType.ChangeRecentValue, OnRecentValueChanged);
        }
    }

    // 서버 알람로그 변경 이벤트 핸들러
    private void OnAlarmLogChanged(object obj)
    {
        UpdateStatusFromServerAlarmLog();
    }

    // 실시간 측정값 변경 이벤트 핸들러 (편차값 업데이트용)
    private void OnRecentValueChanged(object obj)
    {
        UpdateStatusFromServerAlarmLog();
    }

    // 서버 알람로그 기반으로 상태 업데이트
    private void UpdateStatusFromServerAlarmLog()
    {
        if (UiManager.Instance?.modelProvider == null)
            return;

        try
        {
            // 현재 편차값 계산 (UI 표시용)
            var upstreamValue = UiManager.Instance.modelProvider.GetMeasureRecentBySensor(1, sensorId)?.measured_value ?? 0f;
            var downstreamValue = UiManager.Instance.modelProvider.GetMeasureRecentBySensor(2, sensorId)?.measured_value ?? 0f;
            float deviation = downstreamValue - upstreamValue; // 하류 - 상류

            // 센서 정보 가져오기
            var sensor = UiManager.Instance.modelProvider.GetSensors()
                .Find(s => s.sensor_id == sensorId);

            if (sensor == null)
            {
                HideAllTexts();
                return;
            }

            // 서버에서 전달받은 알람 로그 확인
            var alarmLogs = UiManager.Instance.modelProvider.GetAlarmLogList();
            var activeAlarm = alarmLogs
                .Where(log => log.sensor_id == sensorId && string.IsNullOrEmpty(log.solved_time))
                .OrderBy(log => log.alarm_level == "Warning" ? 0 : 1)  // Warning 우선
                .ThenByDescending(log => log.occured_time)
                .FirstOrDefault();

            if (activeAlarm != null)
            {
                // 서버에서 판정된 알람이 있으면 그에 따라 표시
                switch (activeAlarm.alarm_level)
                {
                    case "Warning": // 경보
                        ShowAlertStatus(deviation, sensor.threshold_warning);
                        Debug.Log($"[서버 알람] 센서 {sensorId} 경보 상태 표시");
                        break;
                    case "Serious": // 경계
                        ShowWarningStatus(deviation, sensor.threshold_serious);
                        Debug.Log($"[서버 알람] 센서 {sensorId} 경계 상태 표시");
                        break;
                    default:
                        HideAllTexts();
                        break;
                }
            }
            else
            {
                // 활성화된 알람이 없으면 텍스트 숨기기
                HideAllTexts();
                Debug.Log($"[서버 알람] 센서 {sensorId} 정상 상태 (알람 없음)");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"서버 알람로그 기반 상태 업데이트 중 에러: {e.Message}");
        }
    }

    private void ShowWarningStatus(float value, float threshold)
    {
        // txtInfo 활성화
        if (txtInfo != null)
            txtInfo.SetActive(true);

        Color yellowColor = new Color(1f, 1f, 0f, 1f); // 노란색 (경계)

        if (deviationText != null)
        {
            deviationText.text = "하류-상류 차이:";
            deviationText.color = yellowColor;
        }
        if (deviationValueText != null)
        {
            deviationValueText.text = value.ToString("F1");
            deviationValueText.color = yellowColor;
        }
        if (statusLabelText != null)
        {
            statusLabelText.text = "경계 기준:";
            statusLabelText.color = yellowColor;
        }
        if (thresholdValueText != null)
        {
            thresholdValueText.text = threshold.ToString("F1");
            thresholdValueText.color = yellowColor;
        }
    }

    private void ShowAlertStatus(float value, float threshold)
    {
        // txtInfo 활성화
        if (txtInfo != null)
            txtInfo.SetActive(true);

        Color redColor = new Color(1f, 0f, 0f, 1f); // 빨간색 (경보)

        if (deviationText != null)
        {
            deviationText.text = "하류-상류 차이:";
            deviationText.color = redColor;
        }
        if (deviationValueText != null)
        {
            deviationValueText.text = value.ToString("F1");
            deviationValueText.color = redColor;
        }
        if (statusLabelText != null)
        {
            statusLabelText.text = "경보 기준:";
            statusLabelText.color = redColor;
        }
        if (thresholdValueText != null)
        {
            thresholdValueText.text = threshold.ToString("F1");
            thresholdValueText.color = redColor;
        }
    }

    private void HideAllTexts()
    {
        // txtInfo 비활성화
        if (txtInfo != null)
            txtInfo.SetActive(false);

        // 텍스트만 비우기
        if (deviationValueText != null) deviationValueText.text = "";
        if (statusLabelText != null) statusLabelText.text = "";
        if (thresholdValueText != null) thresholdValueText.text = "";
        if (deviationText != null) deviationText.text = "";
    }
}