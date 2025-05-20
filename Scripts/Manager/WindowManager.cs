using UnityEngine;

internal class WindowManager : MonoBehaviour
{
    private void Start()
    {
        ActivateDualDisplay();
    }
    internal bool ActivateDualDisplay() 
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Debug.Log("두 번째 디스플레이 활성화!");
            return true;
        }
        else
        {
            Debug.LogWarning("추가 디스플레이가 감지되지 않았습니다.\n해당 기능은 유니티 에디터 상에서 사용할 수 없습니다.");
            return false;
        }

        //if (!isWindowActivated)
        //    DOVirtual.DelayedCall(0.5f, ActivateDisplay);
    }
}