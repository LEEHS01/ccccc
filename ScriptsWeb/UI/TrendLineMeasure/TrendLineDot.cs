using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrendLineDot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //Raycast Target이 활성화된 UI 컴포넌트가 있어야 함
    //Canvas가 GraphicRaycaster를 포함해야 함
    public MeasureModel measureModel;


    public void OnPointerExit(PointerEventData eventData)
    {
        UiManager.Instance.Invoke(UiEventType.InspectorApply, (transform.position, measureModel));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UiManager.Instance.Invoke(UiEventType.InspectorRelease);
    }
}
