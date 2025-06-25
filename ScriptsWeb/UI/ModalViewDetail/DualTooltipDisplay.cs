using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
using Onthesys.WebBuild;

public class DualTooltipDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text upperValueText;
    public TMP_Text lowerValueText;
    public TMP_Text timeText;
    public TMP_Text dateText;
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

    public void Show(MeasureModel upperData, MeasureModel lowerData, Vector2 screenPosition, Camera uiCamera)
    {
        // 데이터 표시
        if (upperValueText != null)
            upperValueText.text = $"상류: {upperData.measured_value:F1}";

        if (lowerValueText != null)
            lowerValueText.text = $"하류: {lowerData.measured_value:F1}";

        if (timeText != null)
            timeText.text = upperData.MeasuredTime.ToString("HH:mm:ss");

        if (dateText != null)
            dateText.text = upperData.MeasuredTime.ToString("yyyy-MM-dd");

        // 위치 업데이트
        SetPosition(screenPosition, uiCamera);

        // 즉시 표시 (애니메이션 없음)
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    public void Hide(System.Action onComplete = null)
    {
        // 즉시 숨김 (애니메이션 없음)
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void SetPosition(Vector2 screenPosition, Camera uiCamera)
    {
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                screenPosition,
                uiCamera,
                out localPoint
            );

            /*// 핵심: 툴팁의 하단 중앙이 마우스 위치에 오도록 조정

            // 먼저 레이아웃 업데이트로 실제 크기 계산
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            Vector2 tooltipSize = rectTransform.rect.size;

            // 툴팁의 pivot이 center(0.5, 0.5)라고 가정하고
            // 하단 중앙이 마우스 위치에 오려면 Y축으로 툴팁 높이의 절반만큼 위로 이동
            float yOffset = tooltipSize.y * 0.7f;

            Vector2 finalPosition = localPoint + new Vector2(0, yOffset);
            rectTransform.localPosition = finalPosition;*/

            // 기본 위치 설정
            rectTransform.localPosition = localPoint + new Vector2(10, 10);

            // 화면 밖으로 나가지 않도록 조정
            KeepInBounds();
        }
    }

    private void KeepInBounds()
    {
        if (parentCanvas == null) return;

        Vector3[] canvasCorners = new Vector3[4];
        Vector3[] tooltipCorners = new Vector3[4];

        (parentCanvas.transform as RectTransform).GetWorldCorners(canvasCorners);
        rectTransform.GetWorldCorners(tooltipCorners);

        Vector3 offset = Vector3.zero;

        // 오른쪽 경계 체크
        if (tooltipCorners[2].x > canvasCorners[2].x)
            offset.x = canvasCorners[2].x - tooltipCorners[2].x - 10;

        // 위쪽 경계 체크  
        if (tooltipCorners[2].y > canvasCorners[2].y)
            offset.y = canvasCorners[2].y - tooltipCorners[2].y - 10;

        // 왼쪽 경계 체크
        if (tooltipCorners[0].x < canvasCorners[0].x)
            offset.x = canvasCorners[0].x - tooltipCorners[0].x + 10;

        // 아래쪽 경계 체크
        if (tooltipCorners[0].y < canvasCorners[0].y)
            offset.y = canvasCorners[0].y - tooltipCorners[0].y + 10;

        rectTransform.position += offset;
    }
}