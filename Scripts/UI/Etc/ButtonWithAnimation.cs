using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections; // IEnumerator를 사용하기 위해 추가

internal class ButtonWithAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    internal GameObject targetObject; // 애니메이션이 적용된 게임 오브젝트
    private Animator animator;

    private void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (Transform child in targetObject.transform)
        {
            if (child.gameObject.activeSelf)
            {
                animator = child.GetComponent<Animator>();
                break;
            }
        }

        if (animator != null)
        {
            animator.SetTrigger("Play"); // Play 트리거를 설정
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator != null)
        {
            StartCoroutine(StopAnimationAfterDelay(1.0f)); // 1초 후에 Stop 트리거를 설정
        }
    }

    private IEnumerator StopAnimationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null)
        {
            animator.SetTrigger("Stop"); // Stop 트리거를 설정
        }
    }
}
