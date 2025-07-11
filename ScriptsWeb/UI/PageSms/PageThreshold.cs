﻿using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PageThreshold : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    Button btnSave, btnCancel;

    private void Start()
    {
        btnCancel = transform.Find("btnClose").GetComponent<Button>();
        btnCancel.onClick.AddListener(OnClickCancel);

        btnSave = transform.Find("btnInput").GetComponent<Button>();
        btnSave.onClick.AddListener(OnClickSave);

        UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);
        UiManager.Instance.Register(UiEventType.ResponseThresholdUpdate, OnResponseThresholdUpdate);    

        gameObject.SetActive(false);
    }

    //0609 수정
    private void OnNavigateSms(object obj)
    {
        if (obj is not Type type) return;

        if (type != typeof(PageThreshold)) return;

        // 센서 데이터가 있으면 바로 로드
        if (modelProvider.GetSensors().Count > 0)
        {
            LoadThresholdItems();
        }
        else
        {
            Debug.Log("센서 데이터 로딩 중... ChangeSensorData 이벤트 대기");
        }

    }

    private void OnResponseThresholdUpdate(object obj)
    {
        btnSave.interactable = true;

        if (obj is not (bool isSucceed, string message)) return;

        if (isSucceed)
        {
            Debug.Log("임계값 저장 성공: " + message);
            // 성공 알림 TODO
            UiManager.Instance.Invoke(UiEventType.PopupError, ("임계값 수정 성공", "임계값이 성공적으로 저장되었습니다."));
            UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
        }
        else
        {
            Debug.LogError("임계값 저장 실패: " + message);
            // 실패 알림 TODO
            UiManager.Instance.Invoke(UiEventType.PopupError, ("임계값 수정 실패", message));
        }
    }

    void LoadThresholdItems()
    {
        List<SensorModel> sensors = modelProvider.GetSensors();

        if (sensors == null || sensors.Count == 0)
        {
            Debug.LogWarning("센서 데이터가 아직 로드되지 않았습니다.");
            return;
        }

        // 모든 ItemThreshold에게 각자 지정된 센서 데이터를 로드하도록 지시
        ItemThreshold[] allItems = GetComponentsInChildren<ItemThreshold>();
        foreach (var item in allItems)
        {
            item.LoadData(); // 각자 sensorId로 센서 찾아서 로드
        }

        Debug.Log($"총 {allItems.Length}개 센서 UI 로드 완료");
    }

    void OnClickSave()
    {
        List<SensorModel> updatedSensors = new List<SensorModel>();

        // 모든 ItemThreshold에서 업데이트된 데이터 가져오기
        ItemThreshold[] allItems = GetComponentsInChildren<ItemThreshold>();
        foreach (var item in allItems)
        {
            if (item.GetUpdatedSensorData() is not SensorModel updatedSensor) return;
            updatedSensors.Add(updatedSensor);
        }

        if (updatedSensors.Count != 3) {
            Debug.LogWarning("모든 항목이 올바른 경우에만 저장할 수 있습니다!");
            return;
        }

        btnSave.interactable = false;

        // 임계값 업데이트 요청
        UiManager.Instance.Invoke(UiEventType.RequestThresholdUpdate, updatedSensors);
    }

    void OnClickCancel()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms, typeof(PageSmsManage));
    }
}