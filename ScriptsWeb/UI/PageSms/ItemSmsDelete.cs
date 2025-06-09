using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ItemSmsDelete : MonoBehaviour
{
    public bool isChecked { private set; get; }
    public SmsServiceModel data { private set; get;}

    Toggle tglIsChecked;

    TMP_Text lblName, lblPhone, lblthreshold;   //임계값
    private void Start()
    {
        lblName = transform.Find("GameObject (1)").GetComponentInChildren<TMP_Text>();
        lblPhone = transform.Find("GameObject (2)").GetComponentInChildren<TMP_Text>();
        lblthreshold = transform.Find("GameObject (3)").GetComponentInChildren<TMP_Text>(); //임계값
        tglIsChecked = transform.Find("Toggle").GetComponent<Toggle>();
    }

    public void SetData(SmsServiceModel data)
    {
        if (lblName == null) Start();

        this.data = data;

        lblName.text = data.name;
        lblPhone.text = data.phone;

        //0605 수정사항
        string thresholdValue = GetThresholdValue(data.alarm_level);
        lblthreshold.text = thresholdValue;

        tglIsChecked.onValueChanged.AddListener(OnToggleEnabled);
        tglIsChecked.isOn = false;
    }

    //0605 수정사항
    private string GetThresholdValue(string alarmLevel)
    {
        var sensor = UiManager.Instance.modelProvider.GetSensor(data.board_id, data.sensor_id);
        if (sensor == null) return "-";

        return alarmLevel switch
        {
            "Warning" => sensor.threshold_warning.ToString("F1"),
            "Serious" => sensor.threshold_serious.ToString("F1"),
            _ => "-"
        };
    }

    void OnToggleEnabled(bool isChecked) => this.isChecked = isChecked;

   
}