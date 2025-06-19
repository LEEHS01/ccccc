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

        // 위치 업데이트 (항상 실행)
        SetPosition(screenPosition, uiCamera);

        // 페이드 인 애니메이션 (처음 표시될 때만)
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.2f);
        }
        else if (canvasGroup != null)
        {
            // 이미 표시된 상태라면 알파값을 1로 유지
            canvasGroup.alpha = 1f;
        }
    }

    public void Hide(System.Action onComplete = null)
    {
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, 0.1f).OnComplete(() => {
                onComplete?.Invoke();
            });
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    private void SetPosition(Vector2 screenPosition, Camera uiCamera)
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

            // 화면 경계 체크 및 조정
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

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