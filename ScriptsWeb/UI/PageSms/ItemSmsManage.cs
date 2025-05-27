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
    TMP_Text lblName, lblPhone, lblAlarmLevel;

    private void Start()
    {
        lblName = transform.Find("GameObject (1)").GetComponentInChildren<TMP_Text>();
        lblPhone = transform.Find("GameObject (2)").GetComponentInChildren<TMP_Text>();
        lblAlarmLevel = transform.Find("GameObject (3)").GetComponentInChildren<TMP_Text>();
        tglIsEnabled = transform.Find("Toggle").GetComponent<Toggle>();

        btnItem = GetComponent<Button>();
        btnItem.onClick.AddListener(OnClick);
    }


    public void SetData(SmsServiceModel data)
    {
        if (lblName == null) Start();

        this.data = data;

        lblName.text = data.name;
        lblPhone.text = data.phone;
        lblAlarmLevel.text = data.alarm_level;
        tglIsEnabled.SetIsOnWithoutNotify(data.isEnabled);
        tglIsEnabled.onValueChanged.AddListener(OnToggleEnabled);
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