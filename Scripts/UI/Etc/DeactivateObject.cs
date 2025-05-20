using UnityEngine;
using UnityEngine.UI;

internal class DeactivateObject : MonoBehaviour
{
    // B 오브젝트를 드래그하여 할당할 수 있는 변수
    internal GameObject targetObject;

    void Start() {
        if(this.GetComponent<Image>()) {
            //this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        }
    }

    // 버튼 클릭 시 호출되는 함수
    internal void OnButtonClick()
    {
        if (targetObject != null)
        {
            // 대상 오브젝트의 하위 모든 오브젝트를 비활성화
            SetChildrenActive(targetObject, false);
        }
        else
        {
            Debug.LogWarning("타겟 오브젝트가 설정되지 않았습니다.");
        }
    }

    // 자식 오브젝트들만 활성화/비활성화
    private void SetChildrenActive(GameObject obj, bool isActive)
    {
        foreach (Transform child in obj.transform)
        {
            child.gameObject.SetActive(isActive);
        }
    }
}
