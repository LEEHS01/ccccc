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
    TMP_Dropdown ddlSensorselect, ddlThresholdselect;   //0605 수정
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

        //0605 수정
        ddlSensorselect = transform.Find("ddlSensor").GetComponent<TMP_Dropdown>();
        ddlThresholdselect = transform.Find("ddlThreshold").GetComponent<TMP_Dropdown>();

        //InitializeDropdowns();    //임시더미데스트0605

        UiManager.Instance.Register(UiEventType.ResponseSmsUpdate, OnResponseSmsUpdate);
        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);


        gameObject.SetActive(false);
    }

    //0605 수정사항
    private void InitializeDropdowns()
    {
        ddlSensorselect.options.Clear();
        /*//임시 더미센서데이터
        ddlSensorselect.options.Add(new TMP_Dropdown.OptionData("센서1 (1-1)"));
        ddlSensorselect.options.Add(new TMP_Dropdown.OptionData("센서2 (1-2)"));
        ddlSensorselect.options.Add(new TMP_Dropdown.OptionData("센서3 (2-1)"));*/


        var sensors = modelProvider.GetSensors();
        foreach (var sensor in sensors)
        {
            ddlSensorselect.options.Add(new TMP_Dropdown.OptionData($"{sensor.sensor_name} ({sensor.board_id}-{sensor.sensor_id})"));
        }

        ddlThresholdselect.options.Clear();
        ddlThresholdselect.options.Add(new TMP_Dropdown.OptionData("경보"));    // Warning
        ddlThresholdselect.options.Add(new TMP_Dropdown.OptionData("경계"));    // Serious  
    }

    private void SetDropdownValues()
    {
        var sensors = modelProvider.GetSensors();
        int sensorIndex = sensors.FindIndex(s => s.board_id == data.board_id && s.sensor_id == data.sensor_id);
        if (sensorIndex >= 0) ddlSensorselect.SetValueWithoutNotify(sensorIndex);

        int thresholdIndex = data.alarm_level switch
        {
            "WARNING" => 0,
            "SERIOUS" => 1,
            _ => 0
        };
        ddlThresholdselect.SetValueWithoutNotify(thresholdIndex);
    }

    private void OnNavigateSms(object obj)
    {
        if (obj is not int serviceId) return;

        this.data = modelProvider.GetSmsServiceById(serviceId);

        InitializeDropdowns();      //0605 수정

        txbName.SetTextWithoutNotify(data.name);
        txbPhoneNumber.SetTextWithoutNotify(data.phone);

        SetDropdownValues();
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
        return ddlThresholdselect.value switch  
        {
            0 => StatusType.WARNING,
            1 => StatusType.SERIOUS,
            _ => StatusType.WARNING
        };
    }
    void OnClickConfirm()
    {
        var sensors = modelProvider.GetSensors();
        var selectedSensor = sensors[ddlSensorselect.value];

        UiManager.Instance.Invoke(UiEventType.RequestSmsUpdate, (data.service_id, new SmsServiceModel() 
        {
            service_id = data.service_id,
            alarm_level = GetTypeFromDropdown().ToString(),
            name = txbName.text,
            phone = txbPhoneNumber.text,
            is_enabled = data.is_enabled,                    //0605 isEnabled → is_enabled
            board_id = selectedSensor.board_id,              //0605 수정
            sensor_id = selectedSensor.sensor_id,            //0605 수정
            checked_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")  // 0605 수정
        }));
    }
    void OnClickCancel()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }



}