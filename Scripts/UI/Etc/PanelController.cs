using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Onthesys.ExeBuild;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Events;

internal class PanelController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
{
    internal Animator allUIAnimator;
    internal RectTransform panelRectTransform;
    internal Button homeButton;  // 홈 버튼

    internal float moveSpeed = 10f;
    internal float scrollSpeed = 0.1f;

    internal float maxHorizontalMoveRange = 300f;  // 최대 확대 시 좌우 이동 거리
    internal float maxVerticalMoveRange = 500f;    // 최대 확대 시 위아래 이동 거리
    internal float minScale = 0.7f;
    internal float maxScale = 2f;

    private Vector3 originalPosition = new Vector3(-50, -100, 0);
    private Vector3 originalScale = new Vector3(1, 1, 1);

    internal UnityEvent onHome;
    internal UnityEvent<int> onSelectArea;
    internal UnityEvent<Dictionary<int, AlarmCount>> onAlarmUpdateMap;

    void Start()
    {
        if (panelRectTransform == null)
        {
            panelRectTransform = GetComponent<RectTransform>();
        }

        panelRectTransform.anchoredPosition = originalPosition;
        panelRectTransform.localScale = originalScale;

        //if (homeButton != null)
        //{
        //    homeButton.onClick.AddListener(ResetPanel);
        //}
        //DataManager.Instance.OnHome.AddListener(this.ResetPanel);
        //DataManager.Instance.OnHome.AddListener(this.OnHome);
        //DataManager.Instance.OnSelectArea.AddListener(this.OnSelectArea);
        //DataManager.Instance.OnAlarmUpdateMap.AddListener(this.OnAlarmUpdateMap);
    }

    void Update()
    {
        HandleKeyboardInput();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public void OnHome() =>onHome.Invoke();
    public void OnSelectArea(int areaId) => onSelectArea.Invoke(areaId);
    //internal void OnAlarmUpdateMap(List<KeyValuePair<int, AlarmCount>> mapData) => onAlarmUpdateMap.Invoke(mapData.ToDictionary());


    public void OnDrag(PointerEventData eventData)
    {
        if(this.allUIAnimator.playbackTime != -1) return;
        this.allUIAnimator.enabled = false;
        Vector3 newPos = panelRectTransform.localPosition + new Vector3(eventData.delta.x, eventData.delta.y, 0);
        newPos = ClampPosition(newPos);
        panelRectTransform.localPosition = newPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //allUIAnimator.enabled = true;   
    }

    public void OnScroll(PointerEventData eventData)
    {
        if(this.allUIAnimator.playbackTime != -1) return;
        this.allUIAnimator.enabled = false;
        Vector3 newScale = panelRectTransform.localScale + Vector3.one * eventData.scrollDelta.y * scrollSpeed;
        newScale = ClampScale(newScale);

        // z 값 고정
        newScale.z = 1f;

        // 스케일에 따른 위치 조정
        Vector3 newPos = ClampPosition(panelRectTransform.localPosition);
        panelRectTransform.localPosition = newPos;

        panelRectTransform.localScale = newScale;
    }

    private void HandleKeyboardInput()
    {
        if(this.allUIAnimator.enabled) return;
        Vector3 newPos = panelRectTransform.localPosition;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            newPos.y += moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            newPos.y -= moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            newPos.x -= moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            newPos.x += moveSpeed * Time.deltaTime;
        }

        newPos = ClampPosition(newPos);
        panelRectTransform.localPosition = newPos;
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        float scale = (panelRectTransform.localScale.x - minScale) / (maxScale - minScale);
        float horizontalMoveRange = Mathf.Lerp(0, maxHorizontalMoveRange, scale);
        float verticalMoveRange = Mathf.Lerp(0, maxVerticalMoveRange, scale);

        position.x = Mathf.Clamp(position.x, originalPosition.x - horizontalMoveRange, originalPosition.x + horizontalMoveRange);
        position.y = Mathf.Clamp(position.y, originalPosition.y - verticalMoveRange, originalPosition.y + verticalMoveRange);
        return position;
    }

    private Vector3 ClampScale(Vector3 scale)
    {
        scale.x = Mathf.Clamp(scale.x, minScale, maxScale);
        scale.y = Mathf.Clamp(scale.y, minScale, maxScale);
        scale.z = 1f; // z 값 고정
        return scale;
    }

    internal void ResetPanel()
    {
        panelRectTransform.localPosition = originalPosition;
        panelRectTransform.localScale = originalScale;
    }
}
