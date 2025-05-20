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


internal class AreaListTypeItem : MonoBehaviour
{
    internal int areaId;
    internal string areaName;

    TMP_Text lblAreaName;
    Dictionary<ToxinStatus, TMP_Text> lblAlarmCounts;
    Button btnNavigateArea;

    private void OnValidate()
    {
        lblAreaName = transform.Find("TitleName_Button").GetComponentInChildren<TMP_Text>();
        lblAreaName.text = areaName;
    }


    private void Start()
    {
        lblAreaName = transform.Find("TitleName_Button").GetComponentInChildren<TMP_Text>();

        var signalLamps = transform.Find("SignalLamps");
        lblAlarmCounts = new() {
            {ToxinStatus.Green  , signalLamps.Find("SignalLamp_Green").GetComponentInChildren<TMP_Text>() },
            {ToxinStatus.Yellow , signalLamps.Find("SignalLamp_Yellow").GetComponentInChildren<TMP_Text>() },
            {ToxinStatus.Red    , signalLamps.Find("SignalLamp_Red").GetComponentInChildren<TMP_Text>() },
            {ToxinStatus.Purple , signalLamps.Find("SignalLamp_Purple").GetComponentInChildren<TMP_Text>() },
        };

        btnNavigateArea = GetComponent<Button>();
        btnNavigateArea.onClick.AddListener(OnClick);   
    }

    private void OnClick()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateArea, areaId);
    }

    internal void SetAreaData(int areaId, string areaName, AlarmCount alarmCount) => SetAreaData(areaId, areaName, alarmCount.green, alarmCount.yellow, alarmCount.red, alarmCount.purple);
    internal void SetAreaData(int areaId, string areaName, Dictionary<ToxinStatus, int> obsStatus) 
    {
        this.areaId = areaId;
        lblAreaName.text = areaName;

        foreach (var pair in obsStatus)
            lblAlarmCounts[pair.Key].text = pair.Value.ToString();
    }
    internal void SetAreaData(int areaId, string areaName, int greenObs, int yellowObs, int redObs, int purpleObs)
        => SetAreaData(areaId, areaName, new Dictionary<ToxinStatus, int>() {
            { ToxinStatus.Green, greenObs },
            { ToxinStatus.Yellow, yellowObs },
            { ToxinStatus.Red, redObs },
            { ToxinStatus.Purple, purpleObs },
        });



    
}