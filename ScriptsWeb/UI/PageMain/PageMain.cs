using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPage : MonoBehaviour
{
    [Header("UI Controls")]
    Button btnTimespan;
    Button btnNavigateSms;
    TMP_Text lblchange;

    // 센서별로 구분하여 관리
    [Header("Sensor1 Charts (상류)")]
    public GameObject[] sensor1ChartsDaily;   // Sensor1-1, 1-2, 1-3 일간
    public GameObject[] sensor1ChartsWeekly;  // Sensor1-1, 1-2, 1-3 주간

    [Header("Sensor2 Charts (하류)")]
    public GameObject[] sensor2ChartsDaily;   // Sensor2-1, 2-2, 2-3 일간
    public GameObject[] sensor2ChartsWeekly;  // Sensor2-1, 2-2, 2-3 주간

    private bool isWeek = false;

    private void Start()
    {
        // UI 컴포넌트 초기화
        btnTimespan = transform.Find("CycleChangeBtn").GetComponent<Button>();
        btnTimespan.onClick.AddListener(OnClickTimespan);

        btnNavigateSms = transform.Find("SmsManagerBtn").GetComponent<Button>();
        btnNavigateSms.onClick.AddListener(OnClickSms);

        lblchange = transform.Find("txtchange").GetComponent<TMP_Text>();

        // 초기 버튼 표시 상태 설정
        UpdateButtonDisplay();

        // 초기 차트 표시 상태 설정 (일간 차트만 활성화)
        UpdateChartDisplay();

        // 이벤트 등록 (필요시)
        // UiManager.Instance.Register(UiEventType.NavigateMain, OnNavigateMain);
        // UiManager.Instance.Register(UiEventType.NavigateSms, OnNavigateSms);
    }

    private void OnNavigateSms(object obj)
    {
        gameObject.SetActive(false);
    }

    private void OnNavigateMain(object obj)
    {
        gameObject.SetActive(true);
    }

    void UpdateButtonDisplay()
    {
        if (isWeek)
        {
            // 주간 모드
            lblchange.text = "주간";
        }
        else
        {
            // 일간 모드  
            lblchange.text = "일간";
        }
    }

    void UpdateChartDisplay()
    {
        // 센서별로 구분하여 관리
        UpdateSensorCharts(sensor1ChartsDaily, sensor1ChartsWeekly);
        UpdateSensorCharts(sensor2ChartsDaily, sensor2ChartsWeekly);
    }

    void UpdateSensorCharts(GameObject[] dailyCharts, GameObject[] weeklyCharts)
    {
        if (dailyCharts != null)
        {
            for (int i = 0; i < dailyCharts.Length; i++)
            {
                if (dailyCharts[i] != null)
                    dailyCharts[i].SetActive(!isWeek);
            }
        }

        if (weeklyCharts != null)
        {
            for (int i = 0; i < weeklyCharts.Length; i++)
            {
                if (weeklyCharts[i] != null)
                    weeklyCharts[i].SetActive(isWeek);
            }
        }
    }

    void OnClickTimespan()
    {
        isWeek = !isWeek;
        UpdateButtonDisplay();   // 버튼 텍스트 업데이트
        UpdateChartDisplay();    // 차트 표시 상태 업데이트

        // UiManager에 시간 범위 변경 이벤트 발송
        UiManager.Instance.Invoke(UiEventType.ChangeTimespan, isWeek);
    }

    void OnClickSms()
    {
        UiManager.Instance.Invoke(UiEventType.NavigateSms);
    }
}