﻿using UnityEngine;
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
        Debug.Log($"[SetPosition] 입력 스크린 위치: {screenPosition}");

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

            Debug.Log($"[SetPosition] 변환 성공: {success}, 로컬 위치: {localPoint}");

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
                    offset.x = -tooltipSize.x * 0.3f; // 툴팁을 왼쪽으로
                    Debug.Log("[SetPosition] 오른쪽 끝 노드 - 왼쪽으로 이동");
                }

                Vector2 finalPosition = localPoint + offset;
                rectTransform.localPosition = finalPosition;

                Debug.Log($"[SetPosition] 스크린 위치: {screenPosition.x}, 최종 오프셋: {offset}");

/*                Vector2 finalPosition = localPoint + new Vector2(0, yOffset);
                rectTransform.localPosition = finalPosition;

                Debug.Log($"[SetPosition] 툴팁 크기: {tooltipSize}, Y 오프셋: {yOffset}");*/
            }

            /* if (success)
             {
                 // 기본 위치 설정 (오프셋 추가)
                 Vector2 finalPosition = localPoint + new Vector2(20, 20);
                 rectTransform.localPosition = finalPosition;

                 Debug.Log($"[SetPosition] 최종 설정 위치: {finalPosition}");
             }
             else
             {
                 Debug.LogError("[SetPosition] 좌표 변환 실패!");
             }*/
        }
    }

    
}