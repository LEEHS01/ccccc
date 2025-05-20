using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageSmsUpdate : MonoBehaviour
{
    TMP_InputField txbName, txbPhoneNumber;
    TMP_Dropdown ddlThreshold;
    Button btnConfirm, btnCancel;

    SmsServiceModel data;



    private void Start()
    {
        UiManager.Instance.Register(UiEventType.ResponseSmsUpdate, OnResponseSmsUpdate);
        UiManager.Instance.Register(UiEventType.RequestSmsUpdate, OnRequestSmsUpdate);

        btnCancel.onClick.AddListener(OnClickCancel);
        btnConfirm.onClick.AddListener(OnClickConfirm);
    }

    private void OnRequestSmsUpdate(object obj)
    {
        if (obj is not (int sensorId, SmsServiceModel model)) return;

        this.data = new();
        //TODO
    }

    private void OnResponseSmsUpdate(object obj)
    {
        if (obj is not (bool isSucceed, string message)) return;

        if (isSucceed)
        {
            //성공알림 TODO
            UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
        }
        else
        {
            //실패알림 TODO
        }
    }

    StatusType GetTypeFromDropdown()
    {
        //TODO
        return StatusType.WARNING;
    }
    void OnClickConfirm()
    {
        UiManager.Instance.Invoke(UiEventType.RequestSmsUpdate, (data.service_id, new SmsServiceModel() 
        {
            alarm_level = GetTypeFromDropdown().ToString(),
            name = txbName.text,
            phone = txbPhoneNumber.text,
            is_enabled = data.isEnabled ? 1 : 0,
            service_id = data.service_id
        }));
    }
    void OnClickCancel()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }



}