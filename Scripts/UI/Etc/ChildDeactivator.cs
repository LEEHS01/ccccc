using UnityEngine;
using System.Collections; // IEnumerator를 사용하기 위해 추가

internal class ChildDeactivator : MonoBehaviour
{
    private void OnEnable()
    {
        // 이 스크립트가 있는 오브젝트가 활성화될 때, 부모의 상태를 감시
        StartCoroutine(CheckParentState());
    }

    private void OnDisable()
    {
        // 이 스크립트가 있는 오브젝트가 비활성화될 때, 코루틴을 중지
        StopAllCoroutines();
    }

    private IEnumerator CheckParentState()
    {
        while (true)
        {
            // 부모 오브젝트가 비활성화되었는지 확인
            if (transform.parent != null && !transform.parent.gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }

            // 다음 프레임까지 대기
            yield return null;
        }
    }
}
