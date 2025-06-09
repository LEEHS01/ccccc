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
    TMP_Dropdown ddlSensorselect, ddlThresholdselect;
    Button btnConfirm, btnCancel;


    private void Start()
    {
        txbName = transform.Find("InputFieldName").GetComponent<TMP_InputField>();
        txbPhoneNumber = transform.Find("InputFieldNumber").GetComponent<TMP_InputField>();

        btnCancel = transform.Find("btnClose").GetComponent<Button>();
        btnCancel.onClick.AddListener(OnClickCancel);

        btnConfirm = transform.Find("btnInput").GetComponent<Button>();
        btnConfirm.onClick.AddListener(OnClickConfirm);

        //0605 수정
        ddlSensorselect = transform.Find("ddlSensor").GetComponent<TMP_Dropdown>();
        ddlThresholdselect = transform.Find("ddlThreshold").GetComponent<TMP_Dropdown>();

        //InitializeDropdowns();        //임시더미데이터

        UiManager.Instance.Register(UiEventType.ResponseSmsRegister, OnResponseSmsRegister);
        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);

        gameObject.SetActive(false);
    }

    private void InitializeDropdowns()
    {
        ddlSensorselect.options.Clear();

        /*//임시 더미센서데이터
        ddlSensorselect.options.Add(new TMP_Dropdown.OptionData("센서1 (1-1)"));
        ddlSensorselect.options.Add(new TMP_Dropdown.OptionData("센서2 (1-2)"));
        ddlSensorselect.options.Add(new TMP_Dropdown.OptionData("센서3 (2-1)"));*/

        var sensors = UiManager.Instance.modelProvider.GetSensors();
        foreach (var sensor in sensors)
        {
            ddlSensorselect.options.Add(new TMP_Dropdown.OptionData($"{sensor.sensor_name} ({sensor.board_id}-{sensor.sensor_id})"));
        }

        ddlThresholdselect.options.Clear();
        ddlThresholdselect.options.Add(new TMP_Dropdown.OptionData("경보"));    // Warning
        ddlThresholdselect.options.Add(new TMP_Dropdown.OptionData("경계"));    // Serious
    }

    private void OnNavigateSms(object obj)
    {
        if (obj is not Type type) return;

        if (type == typeof(PageSmsRegister)) 
        {
            InitializeDropdowns();

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
        return ddlThresholdselect.value switch
        {
            0 => StatusType.WARNING,   
            1 => StatusType.SERIOUS,  
            _ => StatusType.WARNING    
        };
    }
    void OnClickConfirm() 
    {
        var sensors = UiManager.Instance.modelProvider.GetSensors();
        var selectedSensor = sensors[ddlSensorselect.value];


        SmsServiceModel model = new SmsServiceModel
        {
            name = txbName.text,
            phone = txbPhoneNumber.text,
            alarm_level = GetTypeFromDropdown().ToString(),  // 0605 수정
            is_enabled = true,                               // 0605 수정
            board_id = selectedSensor.board_id,              // 0605 수정
            sensor_id = selectedSensor.sensor_id,            // 0605 수정
            checked_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")  //0605 수정
        };

        UiManager.Instance.Invoke(UiEventType.RequestSmsRegister, model);
    }
    void OnClickCancel()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }

}