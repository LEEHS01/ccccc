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

    TMP_Text lblName, lblPhone, lblSensor, lblLevel;   //임계값
    private void Start()
    {
        lblName = transform.Find("GameObject (1)").GetComponentInChildren<TMP_Text>();
        lblPhone = transform.Find("GameObject (2)").GetComponentInChildren<TMP_Text>();
        lblSensor = transform.Find("GameObject (3)").GetComponentInChildren<TMP_Text>();    //센서명
        lblLevel = transform.Find("GameObject (4)").GetComponentInChildren<TMP_Text>();    //임계수준
        tglIsChecked = transform.Find("Toggle").GetComponent<Toggle>();
    }

    public void SetData(SmsServiceModel data)
    {
        if (lblName == null) Start();

        this.data = data;

        lblName.text = data.name;
        lblPhone.text = data.phone;

        string sensorName = GetSensorDisplayText(data.sensor_id);
        lblSensor.text = sensorName;

        string levelText = GetLevelDisplayText(data.alarm_level);
        lblLevel.text = levelText;

        tglIsChecked.onValueChanged.AddListener(OnToggleEnabled);
        tglIsChecked.isOn = false;
    }

    private string GetSensorDisplayText(int sensorId)
    {
        var sensor = UiManager.Instance.modelProvider.GetSensors()
            .FirstOrDefault(s => s.sensor_id == sensorId);

        if (sensor == null)
            return $"센서{sensorId}";

        return $"{sensor.sensor_name}"; 
    }

    private string GetLevelDisplayText(string alarmLevel)
    {
        return alarmLevel switch
        {
            "Warning" => "경보",
            "Serious" => "경계",
            _ => "-"
        };
    }

    void OnToggleEnabled(bool isChecked) => this.isChecked = isChecked;

   
}