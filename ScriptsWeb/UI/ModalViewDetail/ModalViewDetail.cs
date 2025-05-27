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
    List<Button> btnTabList;
    TMP_Dropdown ddlSelectSensor;
    TMP_Text lblSensorName;

    //Data
    //SensorModel sensorData;

    private void Start()
    {
        btnTabList = transform.Find("conBtns").GetComponentsInChildren<Button>().ToList();

        ddlSelectSensor = transform.Find("Title_Image").Find("Dropdown").GetComponent<TMP_Dropdown>();
        lblSensorName = transform.Find("Title_Image").Find("Title_Text").GetComponent<TMP_Text>();

        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        UiManager.Instance.Register(UiEventType.SelectSensorWithinTab, OnSelectSensorWithinTab);
    }

    private void OnSelectSensorWithinTab(object obj)
    {
        if (obj is not (int boardId, int sensorId)) return;

        int idx = modelProvider.GetSensors().FindIndex(sensor => sensor.board_id == boardId && sensor.sensor_id == sensorId);
        ddlSelectSensor.SetValueWithoutNotify(idx);
        lblSensorName.text = "" + modelProvider.GetSensors()[idx].sensor_name;
    }

    private void OnInitiate(object obj)
    {
        ddlSelectSensor.onValueChanged.AddListener(OnSelectSensor);
        ddlSelectSensor.options = modelProvider.GetSensors().Select(sensor => new TMP_Dropdown.OptionData(sensor.sensor_name))
            .ToList();
        ddlSelectSensor.value = 0;

        OnSelectSensor(0);


        btnTabList[0].onClick.Invoke();
        gameObject.SetActive(false);
    }

    private void OnSelectSensor(int arg0)
    {
        SensorModel sensor = modelProvider.GetSensors()[arg0];

        UiManager.Instance.Invoke(UiEventType.SelectSensorWithinTab, (sensor.board_id, sensor.sensor_id));

        lblSensorName.text = "" + sensor.sensor_name;
    }

}