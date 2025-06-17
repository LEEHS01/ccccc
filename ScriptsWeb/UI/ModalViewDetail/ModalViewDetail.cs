using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModalViewDetail : MonoBehaviour
 {
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    //Components
    //List<Button> btnTabList;
    TMP_Dropdown ddlSelectSensor;
    TMP_Text lblSensorName;

    //Data
    //SensorModel sensorData;

    private void Start()
    {
        //btnTabList = transform.Find("conBtns").GetComponentsInChildren<Button>().ToList();

        ddlSelectSensor = transform.Find("Title_Image").Find("Dropdown").GetComponent<TMP_Dropdown>();
        lblSensorName = transform.Find("Title_Image").Find("Title_Text").GetComponent<TMP_Text>();

        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        UiManager.Instance.Register(UiEventType.SelectSensorWithinTab, OnSelectSensorWithinTab);
    }

    private void OnSelectSensorWithinTab(object obj)
    {
        if (obj is not int sensorId) return;

        //기존 코드를 그대로 쓰면 0,1,2 컬랙션에 0,2,4로 접근하게 됨.
        int idx = sensorId - 1;/*modelProvider.GetSensors().FindIndex(sensor => sensor.board_id == 1 && sensor.sensor_id == sensorId);*/
        ddlSelectSensor.SetValueWithoutNotify(idx);
        lblSensorName.text = "" + modelProvider.GetSensors()[idx].sensor_name;
        this.gameObject.SetActive(true);


        //// 기존 DatetimeFrom/DateTimeTo 프로퍼티와 동일한 정규화 적용
        //DateTime now = DateTime.UtcNow.AddHours(9);
        //DateTime yesterday = now.AddDays(-1);

        //// From: 어제 00:00:00
        //DateTime normalizedFrom = yesterday.Date;

        //// To: 오늘이면 현재 시각, 아니면 23:59:59
        //DateTime normalizedTo;
        //if (now.Date == DateTime.UtcNow.AddHours(9).Date)
        //{
        //    normalizedTo = DateTime.UtcNow.AddHours(9); // 현재 시각
        //}
        //else
        //{
        //    normalizedTo = now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        //}

        //UiManager.Instance.Invoke(UiEventType.RequestSearchHistory, (sensorId, normalizedFrom, normalizedTo));
    }

    private void OnInitiate(object obj)
    {
        ddlSelectSensor.onValueChanged.AddListener(OnSelectSensor);
        //3개만 추려서 옵션으로 변환
        ddlSelectSensor.options = modelProvider.GetSensors()
            .Where(sensor => sensor.board_id == 1)
            .Select(sensor => new TMP_Dropdown.OptionData(sensor.sensor_name))
            .ToList();
        ddlSelectSensor.value = 0;

        OnSelectSensor(0);


        //btnTabList[0].onClick.Invoke();
        gameObject.SetActive(false);
    }

    private void OnSelectSensor(int arg0)
    {
        SensorModel sensor = modelProvider.GetSensors()[arg0];

        UiManager.Instance.Invoke(UiEventType.SelectSensorWithinTab, sensor.sensor_id);

        lblSensorName.text = "" + sensor.sensor_name;
    }

}