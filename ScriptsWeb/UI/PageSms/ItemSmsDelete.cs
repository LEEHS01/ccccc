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

    TMP_Text lblName, lblPhone, lblAlarmLevel;
    private void Start()
    {
    }
    public void SetSmsService(SmsServiceModel data)
    {
        this.data = data;
        lblName.text = data.name;
        lblPhone.text = data.phone;
        lblAlarmLevel.text = data.alarm_level;
        tglIsChecked.onValueChanged.AddListener(OnToggleEnabled);
    }

    void OnToggleEnabled(bool isChecked) => this.isChecked = isChecked;
}