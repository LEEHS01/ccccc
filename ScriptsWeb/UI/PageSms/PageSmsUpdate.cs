using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        //InitializeDropdowns();  

        UiManager.Instance.Register(UiEventType.ResponseSmsUpdate, OnResponseSmsUpdate);
        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);


        gameObject.SetActive(false);
    }

    //0605 수정사항
    private void InitializeDropdowns()
    {

        ddlSensorselect.options.Clear();

        var sensors = modelProvider.GetSensors()
            .GroupBy(s => s.sensor_id)
            .Select(g => g.First())
            .OrderBy(s => s.sensor_id)
            .ToList();


        foreach (var sensor in sensors.Where(s => s.sensor_id <= 3))
        {
            ddlSensorselect.options.Add(new TMP_Dropdown.OptionData($"{sensor.sensor_name}"));
        }

        // 임계등급 드롭다운 초기화  
        ddlThresholdselect.options.Clear();
        ddlThresholdselect.options.Add(new TMP_Dropdown.OptionData("경계"));    // Serious
        ddlThresholdselect.options.Add(new TMP_Dropdown.OptionData("경보"));    // Warning
    }

   

    private void OnNavigateSms(object obj)
    {

        if (obj is int serviceId)
        {
            InitializeDropdowns();

            this.data = modelProvider.GetSmsServiceById(serviceId);

            // 기본 정보 설정
            txbName.SetTextWithoutNotify(data.name);
            txbPhoneNumber.SetTextWithoutNotify(data.phone);

            // 드롭다운 설정
            SetDropdownValues();
        }
    }

    private void SetDropdownValues()
    {
        // 센서 드롭다운 설정
        var sensors = modelProvider.GetSensors()
            .GroupBy(s => s.sensor_id)
            .Select(g => g.First())
            .OrderBy(s => s.sensor_id)
            .ToList();

        // 현재 데이터의 sensor_id와 일치하는 인덱스 찾기
        int sensorIndex = sensors.FindIndex(s => s.sensor_id == data.sensor_id);
        if (sensorIndex >= 0)
        {
            ddlSensorselect.SetValueWithoutNotify(sensorIndex);
            ddlSensorselect.RefreshShownValue();
        }

        // 임계등급 드롭다운 설정
        int thresholdIndex = data.alarm_level switch
        {
            "Serious" => 0,  // "경계"
            "Warning" => 1,  // "경보"
            _ => 0           // 기본값
        };

        ddlThresholdselect.SetValueWithoutNotify(thresholdIndex);
        ddlThresholdselect.RefreshShownValue();

        Debug.Log($"센서 설정: sensor_id={data.sensor_id}, 드롭다운 인덱스={sensorIndex}");
        Debug.Log($"임계등급 설정: {data.alarm_level}, 드롭다운 인덱스={thresholdIndex}");
    }


    private void OnResponseSmsUpdate(object obj)
    {
        btnConfirm.interactable = true;

        if (obj is not (bool isSucceed, string message)) return;

        if (isSucceed)
        {
            if (gameObject.activeSelf == true)
            {
                // 로컬 데이터 업데이트
                data.name = txbName.text;
                data.phone = txbPhoneNumber.text;

                // 센서와 임계등급도 업데이트
                var sensors = modelProvider.GetSensors()
                    .GroupBy(s => s.sensor_id)
                    .Select(g => g.First())
                    .OrderBy(s => s.sensor_id)
                    .ToList();

                var selectedSensor = sensors[ddlSensorselect.value];
                data.sensor_id = selectedSensor.sensor_id;
                data.alarm_level = GetTypeFromDropdown().ToDbString();

                Debug.Log("SMS 서비스 수정 성공!");
                UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
            }
        }
        else
        {
            //실패알림 TODO
            Debug.LogError("SMS 서비스 수정 실패: " + message);
        }
    }

    StatusType GetTypeFromDropdown()
    {
        return ddlThresholdselect.value switch  
        {
            0 => StatusType.SERIOUS,
            1 => StatusType.WARNING,
            _ => StatusType.SERIOUS
        };
    }
    void OnClickConfirm()
    {


        if (string.IsNullOrWhiteSpace(txbName.text))
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, ("서비스 등록 실패", "이름을 입력해주세요."));
            return;
        }

        if (string.IsNullOrWhiteSpace(txbPhoneNumber.text))
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, ("서비스 등록 실패", "전화번호를 입력해주세요."));
            return;
        }
        string regexPattern = @"^(01[016789]\d{7,8}|01[016789]-\d{3,4}-\d{4}|0\d{1,2}-\d{3,4}-\d{4})$";
        if (!Regex.IsMatch(txbPhoneNumber.text, regexPattern))
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, ("서비스 등록 실패", "유효한 전화번호 형식이 아닙니다."));
            return;
        }


        List<SmsServiceModel> ssms = modelProvider.GetSmsServices();
        if (ssms.Find(ssm => ssm.phone == txbPhoneNumber.text.Replace("-", string.Empty) && ssm.sensor_id == (ddlSensorselect.value + 1)) != null)
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, ("서비스 등록 실패", "해당 센서에 대한 서비스가 이미 등록되어 있어 서비스 등록에 실패했습니다."));
            return;
        }


        btnConfirm.interactable = false;

        var sensors = modelProvider.GetSensors()
         .GroupBy(s => s.sensor_id)
         .Select(g => g.First())
         .OrderBy(s => s.sensor_id)
         .ToList();

        var selectedSensor = sensors[ddlSensorselect.value];

        UiManager.Instance.Invoke(UiEventType.RequestSmsUpdate, (data.service_id, new SmsServiceModel() 
        {
            service_id = data.service_id,
            alarm_level = GetTypeFromDropdown().ToDbString(),
            name = txbName.text,
            phone = txbPhoneNumber.text.Replace("-", string.Empty),
            is_enabled = data.is_enabled,                    //0605 isEnabled → is_enabled
            sensor_id = selectedSensor.sensor_id,            //0605 수정
            checked_time = data.checked_time
        }));
    }
    void OnClickCancel()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }



}