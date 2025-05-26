using Onthesys;
using UnityEngine;
using UnityEngine.UI;

internal class ButtonAnimatorController : MonoBehaviour
{
    // 제어할 애니메이션 오브젝트
    internal GameObject animationObject;

    // 트리거 파라미터 이름
    private readonly string unselectTriggerName = "UI_UnSellect";
    private readonly string selectTriggerName = "UI_Sellect";

    // 버튼 변수
    internal Button button;

    // 애니메이터 변수
    private Animator animator;

    void Start()
    {
        // 애니메이션 오브젝트에서 애니메이터 컴포넌트를 가져옵니다.
        if (animationObject != null)
        {
            animator = animationObject.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("Animation object is not assigned.");
        }

        // 버튼 컴포넌트를 가져옵니다.
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        // 버튼 클릭 이벤트에 메서드를 추가합니다.
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("Button component is missing.");
        }
    }

    // 버튼 클릭 시 호출되는 메서드
    void OnButtonClick()
    {
        // 기존의 트리거를 리셋
        if (animator != null)
        {
            animator.enabled = true;
            animator.ResetTrigger(selectTriggerName);
            animator.SetTrigger(unselectTriggerName);
        }
        else
        {
            Debug.LogError("Animator component is missing in the animation object.");
        }
    }
}
