using Onthesys;
using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class AreaListYearItem : MonoBehaviour
{
    int areaId;

    TMP_Text lblAreaName, lblSerious, lblWarning, lblMalfunction;
    Button btnNavigateArea; 

    private void Start()
    {
        lblAreaName = transform.Find("Text (TMP) Content").GetComponent<TMP_Text>();
        lblSerious = transform.Find("Text (TMP) Serious").GetComponent<TMP_Text>();
        lblWarning = transform.Find("Text (TMP) Warning").GetComponent<TMP_Text>();
        lblMalfunction = transform.Find("Text (TMP) Malfunction").GetComponent<TMP_Text>();

        btnNavigateArea = GetComponent<Button>();
        btnNavigateArea.onClick.AddListener(OnClick);
    }


    internal void SetAreaData(int areaId, string areaName, AlarmCount alarmCount) 
    {
        this.areaId = areaId;
        lblAreaName.text = areaName;
        lblSerious.text = "" + alarmCount.yellow;
        lblWarning.text = "" + alarmCount.red;
        lblMalfunction.text = "" + alarmCount.purple;
    }

    void OnClick()
    {
        if (areaId < 1) return;
        UiManager.Instance.Invoke(UiEventType.NavigateArea, areaId);
    }
}