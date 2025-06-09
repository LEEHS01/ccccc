using Onthesys.WebBuild;
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

        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        UiManager.Instance.Register(UiEventType.NavigateThreshold, OnNavigateThreshold);    // 임계값 설정 페이지 활성화
        UiManager.Instance.Register(UiEventType.ResponseThresholdUpdate, OnResponseThresholdUpdate);    // 모든 ItemThreshold에게 센서 데이터 로드 지시
        UiManager.Instance.Register(UiEventType.ChangeSensorData, OnSensorDataChanged);

        gameObject.SetActive(false);
    }

    private void OnInitiate(object obj)
    {
        // 초기화 시에는 센서 데이터 변경 이벤트만 등록
    }

    private void OnSensorDataChanged(object obj)
    {
        // 센서 데이터가 로드된 후에 UI 업데이트
        if (gameObject.activeInHierarchy)
        {
            LoadThresholdItems();
        }
    }

    private void OnNavigateThreshold(object obj)
    {
        gameObject.SetActive(true);

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
        if (obj is not (bool isSucceed, string message)) return;

        if (isSucceed)
        {
            Debug.Log("임계값 저장 성공: " + message);
            // 성공 알림 TODO
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("임계값 저장 실패: " + message);
            // 실패 알림 TODO
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
            var updatedSensor = item.GetUpdatedSensorData();
            if (updatedSensor != null)
            {
                updatedSensors.Add(updatedSensor);
            }
        }

        if (updatedSensors.Count > 0)
        {
            UiManager.Instance.Invoke(UiEventType.RequestThresholdUpdate, updatedSensors);
        }
        else
        {
            Debug.LogWarning("저장할 유효한 센서 데이터가 없습니다.");
        }
    }

    void OnClickCancel()
    {
        gameObject.SetActive(false);
    }
}