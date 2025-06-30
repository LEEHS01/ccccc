using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Onthesys.WebBuild;
using System.Linq;

public class SingleTooltipDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text txtDate;    // MainLayout/txtDate
    public TMP_Text txtTime;    // MainLayout/txtTime  
    public TMP_Text txtSensor;  

    public CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 초기에는 보이지 않게
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void Show(MeasureModel measureData, SensorModel sensorData, Vector2 screenPosition, Camera uiCamera)
    {
        // 원시 데이터에서 가장 가까운 시간 찾기
        var rawData = FindClosestRawData(measureData, sensorData);

        // 날짜 표시
        if (txtDate != null)
            txtDate.text = (rawData?.MeasuredTime ?? measureData.MeasuredTime).ToString("yyyy-MM-dd");

        // 시간 표시 (원시 데이터 우선)
        if (txtTime != null)
            txtTime.text = (rawData?.MeasuredTime ?? measureData.MeasuredTime).ToString("HH:mm:ss");

        // 센서값 표시 (원시 데이터 우선)
        if (txtSensor != null)
        {
            string unit = sensorData.unit ?? "";
            string locationText = (rawData?.board_id ?? measureData.board_id) == 1 ? "상류" : "하류";
            float value = rawData?.measured_value ?? measureData.measured_value;
            txtSensor.text = $"{sensorData.sensor_name}: {value:F1}{unit}";

            Color textColor = GetLocationColor(rawData?.board_id ?? measureData.board_id);
            txtSensor.color = textColor;
        }

        SetPosition(screenPosition, uiCamera);

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    private MeasureModel FindClosestRawData(MeasureModel measureData, SensorModel sensorData)
    {
        try
        {
            var rawLogs = UiManager.Instance.modelProvider.GetMeasureLogRaw();

            // 🔍 원시 데이터 개수 확인
            Debug.Log($"[툴팁] 원시 데이터 개수: {rawLogs.Count}");

            var result = rawLogs
                .Where(log => log.board_id == sensorData.board_id && log.sensor_id == sensorData.sensor_id)
                .OrderBy(log => Math.Abs((log.MeasuredTime - measureData.MeasuredTime).TotalMinutes))
                .FirstOrDefault();

            if (result != null)
            {
                // 🔍 원시 데이터 vs 기존 데이터 비교
                Debug.Log($"[툴팁] 기존 데이터: {measureData.MeasuredTime:yyyy-MM-dd HH:mm:ss} = {measureData.measured_value:F1}");
                Debug.Log($"[툴팁] 원시 데이터: {result.MeasuredTime:yyyy-MM-dd HH:mm:ss} = {result.measured_value:F1}");
            }
            else
            {
                Debug.Log("[툴팁] 원시 데이터를 찾을 수 없음");
            }

            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[툴팁] 원시 데이터 조회 실패: {ex.Message}");
            return null;
        }
    }

    // 상류/하류에 따른 색상 반환
    private Color GetLocationColor(int boardId)
    {
        if (boardId == 1) return new Color(0f, 1f, 1f, 1f);  // 상류: #00FFFF
        if (boardId == 2) return new Color(1f, 1f, 0f, 1f);  // 하류: #FFFF00
        return Color.white; // 기본값
    }

    public void Hide(System.Action onComplete = null)
    {
        // 즉시 숨김
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        onComplete?.Invoke();
    }

    public void SetPosition(Vector2 screenPosition, Camera uiCamera)
    {
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas != null)
        {
            Vector2 localPoint;
            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPosition,
                uiCamera,
                out localPoint
            );

            if (success)
            {
                // 먼저 레이아웃 업데이트로 실제 크기 계산
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                Vector2 tooltipSize = rectTransform.rect.size;

                // 화면 경계 근처인지 확인
                float screenWidth = Screen.width;
                float leftBoundary = screenWidth * 0.15f;   // 왼쪽 15% 구간
                float rightBoundary = screenWidth * 0.85f;  // 오른쪽 15% 구간

                // 툴팁의 하단 중앙이 노드 위치에 오도록 Y축 오프셋 계산
                Vector2 offset = new Vector2(0, tooltipSize.y * 0.8f + 5f);

                // 왼쪽 끝 노드들 - 툴팁을 오른쪽으로 이동
                if (screenPosition.x < leftBoundary)
                {
                    offset.x = tooltipSize.x * 0.3f; // 툴팁을 오른쪽으로
                    Debug.Log("[SetPosition] 왼쪽 끝 노드 - 오른쪽으로 이동");
                }
                // 오른쪽 끝 노드들 - 툴팁을 왼쪽으로 이동  
                else if (screenPosition.x > rightBoundary)
                {
                    offset.x = -tooltipSize.x * 0.4f; // 툴팁을 왼쪽으로
                    Debug.Log("[SetPosition] 오른쪽 끝 노드 - 왼쪽으로 이동");
                }

                Vector2 finalPosition = localPoint + offset;
                rectTransform.localPosition = finalPosition;

                Debug.Log($"[SetPosition] 스크린 위치: {screenPosition.x}, 최종 오프셋: {offset}");

                /*// 레이아웃 업데이트로 실제 크기 계산
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                Vector2 tooltipSize = rectTransform.rect.size;

                // 툴팁이 노드 위쪽에 나타나도록 Y축 오프셋 계산
                float yOffset = tooltipSize.y * 1f ;

                Vector2 finalPosition = localPoint + new Vector2(0, yOffset);
                rectTransform.localPosition = finalPosition;*/
            }
        }
    }
}