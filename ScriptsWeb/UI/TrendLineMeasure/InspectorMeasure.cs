using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InspectorMeasure : MonoBehaviour
{
    MeasureModel measureModel;
    public bool isViewingPanel { private set; get; }

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.InspectorApply, OnInspectorApply);
        UiManager.Instance.Register(UiEventType.InspectorRelease, OnInspectorRelease);
    }

    private void OnInspectorApply(object obj)
    {
        if (obj is not (Vector3 pos, MeasureModel measure) ) return;
        gameObject.SetActive(true);
        transform.position = pos;
    }
    private void OnInspectorRelease(object obj)
    {
        gameObject.SetActive(false);
    }
}
