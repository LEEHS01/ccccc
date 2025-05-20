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
        UiManager.Instance.Register(UiEventType.ResponseSmsRegister, OnResponseSmsRegister);
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
        UiManager.Instance.Invoke(UiEventType.RequestSmsRegister, (txbName.name, txbPhoneNumber.name, GetTypeFromDropdown()));
    }
    void OnClickCancel()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }

}