using UnityEngine;
using UnityEngine.EventSystems;

internal class UIDragHandler : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;
    private Vector2 lastPointerPosition;

    private void Awake()
    {
        // RectTransform과 Canvas 컴포넌트를 가져옵니다.
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Canvas component not found in parent hierarchy.");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 클릭한 지점의 위치를 로컬 포인터로 변환합니다.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out lastPointerPosition);

        // 클릭한 지점에서의 오프셋을 계산합니다.
        offset = rectTransform.anchoredPosition - lastPointerPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 시에 실행됩니다.
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 중인 동안 팝업 창의 위치를 업데이트합니다.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out var localPointerPosition))
        {
            // 새롭게 얻은 포인터 위치로 실제 이동할 위치를 계산합니다.
            Vector2 targetPosition = localPointerPosition + offset;
            rectTransform.anchoredPosition = targetPosition;

        }
    }
}
