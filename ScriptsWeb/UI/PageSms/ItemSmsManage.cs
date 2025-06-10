using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ItemSmsManage : MonoBehaviour
{


    public bool isChecked { private set; get; }
    public SmsServiceModel data { private set; get; }


    Button btnItem;
    Toggle tglIsEnabled;
    TMP_Text lblName, lblPhone,lblSensor,lblLevel, lblThreshold;

    private void Start()
    {
        lblName = transform.Find("txtName").GetComponentInChildren<TMP_Text>();
        lblPhone = transform.Find("txtPhone").GetComponentInChildren<TMP_Text>();
        lblSensor = transform.Find("txtSensor").GetComponentInChildren<TMP_Text>();
        lblLevel = transform.Find("txtLevel").GetComponentInChildren<TMP_Text>();
        lblThreshold = transform.Find("txtThreshold").GetComponentInChildren<TMP_Text>();
        tglIsEnabled = transform.Find("ToggleOnOff").GetComponent<Toggle>();

        btnItem = GetComponent<Button>();
        btnItem.onClick.AddListener(OnClick);
    }


    public void SetData(SmsServiceModel data)
    {
        if (lblName == null) Start();

        this.data = data;

        lblName.text = data.name;
        lblPhone.text = data.phone;

        // 수정: 센서 표시 방식 변경
        string sensorInfo = GetSensorDisplayText(data.sensor_id);
        lblSensor.text = sensorInfo;

        // 수정: 레벨과 임계값 분리 표시
        lblLevel.text = GetLevelDisplayText(data.alarm_level);
        lblThreshold.text = GetThresholdDisplayValue(data.alarm_level, data.sensor_id);

        tglIsEnabled.SetIsOnWithoutNotify(data.is_enabled);
        tglIsEnabled.onValueChanged.AddListener(OnToggleEnabled);
    }

    //0609
    private string GetSensorDisplayText(int sensorId)
    {
        var sensor = UiManager.Instance.modelProvider.GetSensors()
            .FirstOrDefault(s => s.sensor_id == sensorId);

        if (sensor == null)
            return $"센서{sensorId}";

        return $"{sensor.sensor_name}";
    }
    //0609
    private string GetThresholdDisplayValue(string alarmLevel, int sensorId)
    {
        var sensor = UiManager.Instance.modelProvider.GetSensors()
            .FirstOrDefault(s => s.sensor_id == sensorId);

        if (sensor == null) return "-";

        return alarmLevel switch
        {
            "Warning" => sensor.threshold_warning.ToString("F1"),
            "Serious" => sensor.threshold_serious.ToString("F1"),
            _ => "-"
        };
    }
    //0609
    private string GetLevelDisplayText(string alarmLevel)
    {
        return alarmLevel switch
        {
            "Warning" => "경보",
            "Serious" => "경계",
            _ => "-"
        };
    }
    //0609 수정
    private void OnToggleEnabled(bool isEnabled)
    {
        data.is_enabled = isEnabled;
        UiManager.Instance.Invoke(UiEventType.RequestSmsUpdate, (data.service_id, data));

       /* data.is_enabled = isEnabled;

        //변경된 데이터만 새로 생성해서 전송
        var updatedData = new SmsServiceModel()
        {
            service_id = data.service_id,
            name = data.name,
            phone = data.phone,
            sensor_id = data.sensor_id,
            alarm_level = data.alarm_level,
            is_enabled = isEnabled,  // 변경된 값
            checked_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        UiManager.Instance.Invoke(UiEventType.RequestSmsUpdate, (data.service_id, updatedData));*/
    }

    public void OnClick()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsUpdate));
        UiManager.Instance.Invoke(UiEventType.NavigateSms, data.service_id);
    }

}