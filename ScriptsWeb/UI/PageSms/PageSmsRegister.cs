using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageSmsRegister : MonoBehaviour
{
    TMP_InputField txbName, txbPhoneNumber;
    TMP_Dropdown ddlSensorselect, ddlLevelselect;
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
        ddlLevelselect = transform.Find("ddlThreshold").GetComponent<TMP_Dropdown>();

        InitializeDropdowns();

        UiManager.Instance.Register(UiEventType.ResponseSmsRegister, OnResponseSmsRegister);
        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);

        gameObject.SetActive(false);
    }

    private void InitializeDropdowns()
    {

        ddlSensorselect.options.Clear();
        var sensors = UiManager.Instance.modelProvider.GetSensors()
            .GroupBy(s => s.sensor_id)
            .Select(g => g.First())
            .OrderBy(s => s.sensor_id)
            .ToList();


        if (sensors != null && sensors.Count > 0)
        {
            foreach (var sensor in sensors)
            {
                ddlSensorselect.options.Add(new TMP_Dropdown.OptionData($"{sensor.sensor_name}"));
            }
            ddlSensorselect.value = 0; 
            ddlSensorselect.RefreshShownValue();
        }

        ddlLevelselect.options.Clear();
        ddlLevelselect.options.Add(new TMP_Dropdown.OptionData("경계"));    // Serious
        ddlLevelselect.options.Add(new TMP_Dropdown.OptionData("경보"));    // Warning
        ddlLevelselect.value = 0;
        ddlLevelselect.RefreshShownValue();


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

            UiManager.Instance.Invoke(UiEventType.PopupError, ("서비스 등록 성공", "신규 서비스를 등록하는데에 성공했습니다"));
            UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
        }
        else 
        {
            //실패알림 TODO
            UiManager.Instance.Invoke(UiEventType.PopupError, ("서비스 등록 실패", "신규 서비스를 등록하는데에 실패했습니다"));
        }
    }

    StatusType GetTypeFromDropdown() 
    {
        //Debug.Log($"Selected threshold index: {ddlLevelselect.value}");
        return ddlLevelselect.value switch
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

        var sensors = UiManager.Instance.modelProvider.GetSensors()
            .GroupBy(s => s.sensor_id)
            .Select(g => g.First())
            .OrderBy(s => s.sensor_id)
            .ToList();

        var selectedSensor = sensors[ddlSensorselect.value ];

        //Debug.Log($"최종 선택된 센서: {selectedSensor.sensor_name} (sensor_id: {selectedSensor.sensor_id})");

        SmsServiceModel model = new SmsServiceModel
        {
            name = txbName.text,
            phone = txbPhoneNumber.text,
            alarm_level = GetTypeFromDropdown().ToDbString(),  
            is_enabled = true,                               
            //board_id = selectedSensor.board_id,              
            sensor_id = selectedSensor.sensor_id,            
            checked_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")  
        };
        /*// 생성된 모델 확인 로그 추가
        Debug.Log($"=== 생성된 SMS 모델 ===");
        Debug.Log($"이름: '{model.name}'");
        Debug.Log($"전화번호: '{model.phone}'");
        Debug.Log($"센서ID: {model.sensor_id}");
        Debug.Log($"알람레벨: '{model.alarm_level}'");*/

        UiManager.Instance.Invoke(UiEventType.RequestSmsRegister, model);
    }

    void OnClickCancel()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }

}