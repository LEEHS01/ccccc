using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class PanelInference : MonoBehaviour
{

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.SelectSensorWithinTab, OnSelectSensorWithinTab);

    }

    private void OnSelectSensorWithinTab(object obj)
    {
        //@TODO
    }
}