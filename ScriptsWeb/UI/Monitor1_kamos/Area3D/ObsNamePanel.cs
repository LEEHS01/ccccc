using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

using Onthesys.ExeBuild;

internal class ObsNamePanel : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    internal int obsId = -1;
    TMP_Text lblObsName;

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.NavigateObs, OnNavigateObs);
        UiManager.Instance.Register(UiEventType.NavigateArea, OnNavigateArea);

        lblObsName = GetComponentInChildren<TMP_Text>();
    }

    private void OnNavigateObs(object obj)
    {
        if (obj is not int obsId) return;

        gameObject.SetActive(true);
        lblObsName.text = modelProvider.GetObs(obsId).obsName;
    }
    private void OnNavigateArea(object obj)
    {
        gameObject.SetActive(false);
    }
}
