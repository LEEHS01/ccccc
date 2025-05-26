using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Onthesys.ExeBuild;

internal class PopupStep : MonoBehaviour
{

    int stepBefore = -1;

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.ChangeSensorStep, OnChangeSensorStep);
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.NavigateHome, OnNavigateObs);


        SetPopupPanels(stepBefore);
    }

    private void OnNavigateObs(object obj)
    {
        stepBefore = -1;
        SetPopupPanels(stepBefore);
    }

    private void OnChangeSensorStep(object obj)
    {
        if (obj is not int step) return;

        if (stepBefore > 0 && stepBefore != step) SetPopupPanels(step);

        stepBefore = step;
    }
    void SetPopupPanels(int step) 
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(child.name == ("Popup_Info_Step_0" + step));
    }

}
