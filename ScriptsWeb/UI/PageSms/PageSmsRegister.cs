using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageSmsRegister : MonoBehaviour
{
    TMP_InputField txbName, txbPhoneNumber;
    TMP_Dropdown ddlThreshold;
    Button btnConfirm, btnCancel;


    private void Start()
    {
        txbName = transform.Find("InputFieldName").GetComponent<TMP_InputField>();
        txbPhoneNumber = transform.Find("InputFieldNumber").GetComponent<TMP_InputField>();

        btnCancel = transform.Find("btnClose").GetComponent<Button>();
        btnCancel.onClick.AddListener(OnClickCancel);

        btnConfirm = transform.Find("btnInput").GetComponent<Button>();
        btnConfirm.onClick.AddListener(OnClickConfirm);

        UiManager.Instance.Register(UiEventType.ResponseSmsRegister, OnResponseSmsRegister);
        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);

        gameObject.SetActive(false);
    }

    private void OnNavigateSms(object obj)
    {
        if(obj is not Type type) return;

        if (type == typeof(PageSmsRegister)) 
        {
            txbName.text = string.Empty;
            txbPhoneNumber.text = string.Empty;
        }
    }

    private void OnResponseSmsRegister(object obj)
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
        SmsServiceModel model = new SmsServiceModel
        {
            name = txbName.text,
            phone = txbPhoneNumber.text,
            alarm_level = GetTypeFromDropdown().ToString(),
            is_enabled = true 
        };


        UiManager.Instance.Invoke(UiEventType.RequestSmsRegister, model);
    }
    void OnClickCancel()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }

}