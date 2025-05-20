using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Onthesys;

internal class PanelController2 : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
{
    internal RectTransform panelRectTransform;
    internal float moveSpeed = 10f;
    internal float scrollSpeed = 0.1f;

    internal float maxHorizontalMoveRange = 300f;  // 최대 확대 시 좌우 이동 거리
    internal float maxVerticalMoveRange = 500f;    // 최대 확대 시 위아래 이동 거리
    internal float minScale = 0.7f;
    internal float maxScale = 2f;

    private Vector3 originalPosition;
    private Vector3 originalScale;

    void Awake()
    {
        this.originalPosition = this.GetComponent<RectTransform>().anchoredPosition;    
        this.originalScale = this.GetComponent<RectTransform>().localScale;    
    }

    void Start()
    {
        if (panelRectTransform == null)
        {
            panelRectTransform = GetComponent<RectTransform>();
        }
    
        panelRectTransform.anchoredPosition = originalPosition;
        panelRectTransform.localScale = originalScale;

    }

    void Update()
    {
        HandleKeyboardInput();
    }

    
    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }


    public void OnDrag(PointerEventData eventData)
    {
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

    private void ResetPanel()
    {
        panelRectTransform.localPosition = originalPosition;
        panelRectTransform.localScale = originalScale;
    }
}
