using Onthesys.WebBuild;
using UnityEngine;
using TMPro;


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
        // 대신 텍스트만 비우기
        if (deviationText != null) deviationText.text = "";
        if (deviationValueText != null) deviationValueText.text = "";
        if (statusLabelText != null) statusLabelText.text = "";
        if (thresholdValueText != null) thresholdValueText.text = "";
        
    }

    public void UpdateDeviation(float deviation)
    {
        Debug.Log($"UpdateDeviation - 편차: {deviation}, 센서ID: {sensorId}");

        var sensor = UiManager.Instance.modelProvider.GetSensors()
            .Find(s => s.sensor_id == sensorId);

        if (sensor == null)
        {
            HideAllTexts();
            return;
        }

        float seriousThreshold = sensor.threshold_serious;  // 경계
        float warningThreshold = sensor.threshold_warning;  // 경보

        Debug.Log($"편차: {deviation}, 경계(serious): {seriousThreshold}, 경보(warning): {warningThreshold}");

        if (deviation >= warningThreshold)  // 경보 (더 높은 값)
        {
            Debug.Log("경보 상태");
            ShowAlertStatus(deviation, warningThreshold);
        }
        else if (deviation >= seriousThreshold)  // 경계 (더 낮은 값)
        {
            Debug.Log("경계 상태");
            ShowWarningStatus(deviation, seriousThreshold);
        }
        else
        {
            Debug.Log("정상 상태");
            HideAllTexts();
        }
    }

    private void ShowWarningStatus(float value, float threshold)
    {
        // txtInfo 활성화 추가 - 이게 빠져있었음!
        if (txtInfo != null)
            txtInfo.SetActive(true);

        Debug.Log($"ShowWarningStatus - 센서ID: {sensorId}");
        Debug.Log($"텍스트 오브젝트 상태 - deviationText: {deviationText != null}, deviationValueText: {deviationValueText != null}");

        if (deviationText != null)
        {
            deviationText.text = "차이:";
            Debug.Log($"편차 텍스트 설정됨: {deviationText.text}");
        }
        if (deviationValueText != null)
        {
            deviationValueText.text = value.ToString("F1");
            Debug.Log($"편차 값 설정됨: {deviationValueText.text}");
        }
        if (statusLabelText != null)
        {
            statusLabelText.text = "경계 기준:";
            Debug.Log($"경계 기준 텍스트 설정됨: {statusLabelText.text}");
        }
        if (thresholdValueText != null)
        {
            thresholdValueText.text = threshold.ToString("F1");
            Debug.Log($"임계값 설정됨: {thresholdValueText.text}");
        }
    }


    private void ShowAlertStatus(float value, float threshold)
    {
        // txtInfo 활성화
        if (txtInfo != null)
            txtInfo.SetActive(true);

        deviationText.text = "차이:";
        deviationValueText.text = value.ToString("F1");
        statusLabelText.text = "경보 기준:";
        thresholdValueText.text = threshold.ToString("F1");
    }

    private void HideAllTexts()
    {
        // 텍스트만 비우기
        if (deviationValueText != null) deviationValueText.text = "";
        if (statusLabelText != null) statusLabelText.text = "";
        if (thresholdValueText != null) thresholdValueText.text = "";
        // "편차:" 텍스트도 숨기려면
        if (deviationText != null) deviationText.text = "";
    }
}