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
    Button btnItem;
    SmsServiceModel data;

    Toggle tglIsEnabled;
    TMP_Text lblName, lblPhone, lblAlarmLevel;
    public void SetSmsService(SmsServiceModel data)
    {
        //TODO
        this.data = data;
        btnItem.onClick.AddListener(OnClick);

        lblName.text = data.name;
        lblPhone.text = data.phone;
        lblAlarmLevel.text = data.alarm_level;
        tglIsEnabled.isOn = data.isEnabled;
        tglIsEnabled.onValueChanged.AddListener(OnToggleEnabled);
    }

    private void OnToggleEnabled(bool isEnabled)
    {
        data.is_enabled = isEnabled? 1 : 0;
        UiManager.Instance.Invoke(UiEventType.RequestSmsUpdate, (data.service_id, data));
    }

    public void OnClick()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsUpdate));
        UiManager.Instance.Invoke(UiEventType.NavigateSms, data.service_id);
    }

}