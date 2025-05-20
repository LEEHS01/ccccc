using UnityEngine;
using TMPro;
using System.Linq;
using Onthesys;
using DG.Tweening;
using System.Collections.Generic;

internal class GetNumData : MonoBehaviour
{
    internal GameObject threeCirclePercentObject; // TMThreeCirclePercent를 가지고 있는 게임 오브젝트
    internal TextMeshProUGUI label; // 값을 표시할 TextMeshProUGUI 컴포넌트
    internal int orderNumber; // 1에서 5 사이의 숫자
    internal int displayType; // 1, 2 또는 3 입력 (1: 1의 자릿수, 2: 10의 자릿수, 3: 100의 자릿수)


    void Start()
    {
        // TMThreeCirclePercent 컴포넌트를 가져옵니다.

        // orderNumber가 1에서 5 사이인지 확인합니다.
        if (orderNumber < 1 || orderNumber > 5)
        {
            Debug.LogError("Order number must be between 1 and 5.");
        }

        // displayType이 1, 2 또는 3인지 확인합니다.
        if (displayType < 1 || displayType > 3)
        {
            Debug.LogError("Display type must be either 1, 2, or 3.");
        }

        //DataManager.Instance.OnUpdate.AddListener(this.UpdateLabelData);

        //DataManager.Instance.OnAlarmUpdate.AddListener(this.UpdateLabelData);
    }

    internal void ForcedUpdateView(int selectedValue)
    {
        label.text = "" + Mathf.Floor(selectedValue / Mathf.Pow(10, displayType - 1)) % 10;
    }
}
