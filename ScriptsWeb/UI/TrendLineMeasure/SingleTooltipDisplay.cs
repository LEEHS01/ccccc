using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Onthesys.WebBuild;

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
        // 날짜 표시
        if (txtDate != null)
            txtDate.text = measureData.MeasuredTime.ToString("yyyy-MM-dd");

        // 시간 표시  
        if (txtTime != null)
            txtTime.text = measureData.MeasuredTime.ToString("HH:mm:ss");

        // 센서명과 값을 한 줄로 표시
        if (txtSensor != null)
        {
            string unit = sensorData.unit ?? "";
            string locationText = measureData.board_id == 1 ? "상류" : "하류";
            txtSensor.text = $"{sensorData.sensor_name}: {measureData.measured_value:F1}{unit}";

            // 상류/하류에 따른 색상 설정
            Color textColor = GetLocationColor(measureData.board_id);
            txtSensor.color = textColor;

            Debug.Log($"[Tooltip] {locationText} 센서 색상 적용: {textColor}");
        }

        // 위치 업데이트
        SetPosition(screenPosition, uiCamera);

        // 즉시 표시
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
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