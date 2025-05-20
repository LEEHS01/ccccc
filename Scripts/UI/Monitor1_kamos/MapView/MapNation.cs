using DG.Tweening;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Onthesys.ExeBuild;
internal class MapNation : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    RectTransform panelRectTransform => GetComponent<RectTransform>();
    List<MapNationMarker> nationMarkers;
    Image imgBackground;

    float moveSpeed = 10f;
    float scrollSpeed = 0.1f;

    float maxHorizontalMoveRange = 300f;  // 최대 확대 시 좌우 이동 거리
    float maxVerticalMoveRange = 500f;    // 최대 확대 시 위아래 이동 거리
    float minScale = 0.7f;
    float maxScale = 2f;

    bool controlable = true;

    private Vector3 originalPosition = new Vector3(-50, -100, 0);
    private Vector3 originalScale = new Vector3(1, 1, 1);

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateHome);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnInitiate);

        nationMarkers = transform.Find("MarkerList").GetComponentsInChildren<MapNationMarker>(true).ToList();
        imgBackground = transform.Find("MapNationBackground").GetComponent<Image>();

        ////DEBUG
        //for (int i = 0; i < nationMarkers.Count; i++)
        //{
        //    MapNationMarker nationMarker = nationMarkers[i];
        //    nationMarker.SetAreaData(i + 1, nationMarker.areaNm, nationMarker.areaType, nationMarker.status);
        //}
    }
    private void OnInitiate(object obj)
    {
        List<AreaData> areas = modelProvider.GetAreas();

        if (areas.Count != nationMarkers.Count)
            throw new Exception("MapNation - OnInitiate : 입력된 데이터 길이가 표현할 수 있는 데이터 길이와 일치하지 않습니다.");

        for (int i = 0; i < nationMarkers.Count; i++) 
        {
            var nationMarker = nationMarkers[i];
            AreaData area = areas[i];
            ToxinStatus areaStatus = modelProvider.GetAreaStatus(area.areaId);

            nationMarker.SetAreaData(area.areaId, area.areaName, area.areaType, areaStatus);
        }
    }
    private void OnNavigateArea(object obj)
    {
        controlable = false;
        SetAnimation(0f, new Vector3(960, 540) + new Vector3(700, 200), 0.60f, 1f);
    }
    private void OnNavigateHome(object obj)
    {
        controlable = true;
        SetAnimation(2 / 5f, new Vector3(960,540), 1f, 1f);

        panelRectTransform.localPosition = originalPosition;
        panelRectTransform.localScale = originalScale;
    }
    private void OnNavigateObs(object obj)
    {
        controlable = false;
        SetAnimation(0f, new Vector3(960, 540) + new Vector3(700, 200), 0.60f, 1f);
    }


    void SetAnimation(float alpha, Vector3 toPos, float toScale, float duration) 
    {
        Color fromColor = imgBackground.color;
        Vector3 fromPos = GetComponent<RectTransform>().position;
        Vector3 fromScale = GetComponent<RectTransform>().localScale;

        DOTween.ToAlpha(() => fromColor, x => fromColor = x, alpha, duration/2f).OnUpdate(() => {
            imgBackground.color = fromColor;
        });

        DOTween.To(() => fromPos, x => fromPos = x, toPos, duration).OnUpdate(() => {
            GetComponent<RectTransform>().position = fromPos;
        });

        DOTween.To(() => fromScale, x => fromScale = x, Vector3.one * toScale, duration).OnUpdate(() => {
            GetComponent<RectTransform>().localScale = fromScale;
        });
    }



    public void OnBeginDrag(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData)
    {
        if (!controlable) return;

        Vector3 newPos = panelRectTransform.localPosition + new Vector3(eventData.delta.x, eventData.delta.y, 0);
        newPos = ClampPosition(newPos);
        panelRectTransform.localPosition = newPos;
    }
    public void OnEndDrag(PointerEventData eventData) { }

    public void OnScroll(PointerEventData eventData)
    {
        if (!controlable) return;

        Vector3 newScale = panelRectTransform.localScale + Vector3.one * eventData.scrollDelta.y * scrollSpeed;
        newScale = ClampScale(newScale);

        // z 값 고정
        newScale.z = 1f;

        // 스케일에 따른 위치 조정
        Vector3 newPos = ClampPosition(panelRectTransform.localPosition);
        panelRectTransform.localPosition = newPos;

        panelRectTransform.localScale = newScale;
    }


    Vector3 ClampPosition(Vector3 position)
    {
        float scale = (panelRectTransform.localScale.x - minScale) / (maxScale - minScale);
        float horizontalMoveRange = Mathf.Lerp(0, maxHorizontalMoveRange, scale);
        float verticalMoveRange = Mathf.Lerp(0, maxVerticalMoveRange, scale);

        position.x = Mathf.Clamp(position.x, originalPosition.x - horizontalMoveRange, originalPosition.x + horizontalMoveRange);
        position.y = Mathf.Clamp(position.y, originalPosition.y - verticalMoveRange, originalPosition.y + verticalMoveRange);
        return position;
    }

    Vector3 ClampScale(Vector3 scale)
    {
        scale.x = Mathf.Clamp(scale.x, minScale, maxScale);
        scale.y = Mathf.Clamp(scale.y, minScale, maxScale);
        scale.z = 1f; // z 값 고정
        return scale;
    }

}

