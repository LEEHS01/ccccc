using UnityEngine;
using UnityEngine.UI;
using System.Collections;

internal class TriggerReset : MonoBehaviour
{
    //// 제어할 애니메이션 오브젝트
    //internal GameObject animationObject;

    //// 애니메이터 변수
    //private Animator animator;

    //// 트리거 리셋 딜레이 시간 (초)
    //internal float triggerResetDelay = 0.1f;

    //void Start()
    //{
    //    // 애니메이션 오브젝트에서 애니메이터 컴포넌트를 가져옵니다.
    //    if (animationObject != null)
    //    {
    //        animator = animationObject.GetComponent<Animator>();
    //    }
    //    else
    //    {
    //        Debug.LogError("Animation object is not assigned.");
    //    }

    //    // 버튼 컴포넌트를 가져옵니다.
    //    Button button = GetComponent<Button>();

    //    // 버튼 클릭 이벤트에 메서드를 추가합니다.
    //    if (button != null)
    //    {
    //        button.onClick.AddListener(OnButtonClick);
    //    }
    //    else
    //    {
    //        Debug.LogError("Button component is missing.");
    //    }
    //}

    //// 버튼 클릭 시 호출되는 메서드
    //void OnButtonClick()
    //{
    //    // 트리거 리셋 코루틴 시작
    //    if (animator != null)
    //    {
    //        if(this.gameObject.activeInHierarchy)
    //            StartCoroutine(ResetAllTriggersAfterDelay(animator, triggerResetDelay));
    //    }
    //    else
    //    {
    //        Debug.LogError("Animator component is missing in the animation object.");
    //    }
    //}

    //// 딜레이 후 모든 트리거 리셋
    //IEnumerator ResetAllTriggersAfterDelay(Animator animator, float delay)
    //{
    //    yield return new WaitForSeconds(delay);
    //    ResetAllTriggers(animator);
    //}

    //// 모든 트리거 리셋 메서드
    //void ResetAllTriggers(Animator animator)
    //{
    //    foreach (AnimatorControllerParameter parameter in animator.parameters)
    //    {
    //        if (parameter.type == AnimatorControllerParameterType.Trigger)
    //        {
    //            animator.ResetTrigger(parameter.name);
    //        }
    //    }
    //}
}
