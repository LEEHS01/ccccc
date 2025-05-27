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
        lblName = transform.Find("GameObject (1)").GetComponentInChildren<TMP_Text>();
        lblPhone = transform.Find("GameObject (2)").GetComponentInChildren<TMP_Text>();
        lblAlarmLevel = transform.Find("GameObject (3)").GetComponentInChildren<TMP_Text>();
        tglIsChecked = transform.Find("Toggle").GetComponent<Toggle>();
    }


    public void SetData(SmsServiceModel data)
    {
        if (lblName == null) Start();

        this.data = data;

        lblName.text = data.name;
        lblPhone.text = data.phone;
        lblAlarmLevel.text = data.alarm_level;

        tglIsChecked.onValueChanged.AddListener(OnToggleEnabled);
        tglIsChecked.isOn = false;
    }

    void OnToggleEnabled(bool isChecked) => this.isChecked = isChecked;

   
}