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
    TMP_Text lblName, lblPhone, lblthreshold;

    private void Start()
    {
        lblName = transform.Find("GameObject (1)").GetComponentInChildren<TMP_Text>();
        lblPhone = transform.Find("GameObject (2)").GetComponentInChildren<TMP_Text>();
        lblthreshold = transform.Find("GameObject (3)").GetComponentInChildren<TMP_Text>();     //임계값
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

        //0605 수정사항
        string thresholdValue = GetThresholdValue(data.alarm_level);
        lblthreshold.text = thresholdValue;

        tglIsEnabled.SetIsOnWithoutNotify(data.is_enabled);
        tglIsEnabled.onValueChanged.AddListener(OnToggleEnabled);
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

    private void OnToggleEnabled(bool isEnabled)
    {
        data.is_enabled = isEnabled;
        UiManager.Instance.Invoke(UiEventType.RequestSmsUpdate, (data.service_id, data));
    }

    public void OnClick()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsUpdate));
        UiManager.Instance.Invoke(UiEventType.NavigateSms, data.service_id);
    }

}