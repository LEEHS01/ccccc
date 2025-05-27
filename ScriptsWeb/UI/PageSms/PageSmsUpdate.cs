using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PageSmsUpdate : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    TMP_InputField txbName, txbPhoneNumber;
    TMP_Dropdown ddlThreshold;
    Button btnConfirm, btnCancel;

    SmsServiceModel data;



    private void Start()
    {
        txbName = transform.Find("InputFieldName").GetComponent<TMP_InputField>();
        txbPhoneNumber = transform.Find("InputFieldNumber").GetComponent<TMP_InputField>();

        btnCancel = transform.Find("btnClose").GetComponent<Button>();
        btnCancel.onClick.AddListener(OnClickCancel);

        btnConfirm = transform.Find("btnInput").GetComponent<Button>();
        btnConfirm.onClick.AddListener(OnClickConfirm);

        UiManager.Instance.Register(UiEventType.ResponseSmsUpdate, OnResponseSmsUpdate);
        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);


        gameObject.SetActive(false);
    }

    private void OnNavigateSms(object obj)
    {
        if (obj is not int serviceId) return;

        this.data = modelProvider.GetSmsServiceById(serviceId);

        txbName.SetTextWithoutNotify(data.name);
        txbPhoneNumber.SetTextWithoutNotify(data.phone);
    }

    private void OnResponseSmsUpdate(object obj)
    {
        if (obj is not (bool isSucceed, string message)) return;

        if (isSucceed)
        {
            if (gameObject.activeSelf == true)
            {
                data.name = txbName.text;
                data.phone = txbPhoneNumber.text;

                //성공알림 TODO
                UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
            }
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
            is_enabled = data.isEnabled,
            service_id = data.service_id
        }));
    }
    void OnClickCancel()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }



}