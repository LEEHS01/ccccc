using Newtonsoft.Json.Linq;
using Onthesys;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UI;
using Onthesys.ExeBuild;


internal class DetailToxinBar : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    private ToxinData toxinData;

    private DateTime aladt;
    internal TMP_Text txtName;
    internal UILineRenderer2 line;
    internal List<TMP_Text> hours;
    internal List<TMP_Text> verticals;
    internal GameObject btnDetail;

    //private int periodDays = 1;
    internal TMP_Dropdown periodDropdown;



    void Start()
    {
        Initialize();
        InitializeDropdown();

        UiManager.Instance.Register(UiEventType.SelectCurrentSensor, OnSelectCurrentSensor);
        //UiManager.Instance.Register(UiEventType.SelectCurrentSensor, OnSelectLog);
        //UiManager.Instance.Register(UiEventType.SelectAlarmSensor, OnSelectToxin);
        UiManager.Instance.Register(UiEventType.ChangeTrendLine, OnChangeTrendLine); // 이벤트 등록
        btnDetail.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClick);

        //UiManager.Instance.Invoke(UiEventType.SelectCurrentSensor, (1, 0)); // 디폴트 센서 강제 트리거
    }

    private void OnClick()
    {
        if (toxinData == null) return;

        UiManager.Instance.Invoke(UiEventType.Popup_AiTrend, (toxinData.boardid, toxinData.hnsid));
    }

    private void OnChangeTrendLine(object obj)
    {
        if (toxinData == null) return;

        if (toxinData.values.Count == 0)
            toxinData = modelProvider.GetToxin(toxinData.boardid, toxinData.hnsid);


        List<float> normalizedValues = new();
        float max = Math.Max(toxinData.values.Max(), toxinData.warning);
        toxinData.values.ForEach(val => normalizedValues.Add(val / max));

        line.UpdateControlPoints(normalizedValues);

    }
    /*
    private void OnSelectCurrentSensor(object obj)
    {
        if (obj is not (int boardId, int hnsId)) return;

        toxinData = modelProvider.GetToxin(toxinData.boardid, toxinData.hnsid);

        txtName.text = toxinData.hnsName;

        List<float> normalizedValues = new();
        float max = Math.Max(toxinData.values.Max(), toxinData.warning);
        toxinData.values.ForEach(val => normalizedValues.Add(val / max));
        SetVertical(max);
        SetDynamicHours(1);
        line.UpdateControlPoints(normalizedValues);
    }
    */
    private void Initialize()
    {
        SetDynamicHours(1);
        this.txtName.text = "";
        this.btnDetail.SetActive(true);
    }

    

    private void OnSelectCurrentSensor(object obj)
    {
        // 전달된 값이 튜플 형식이 아닌 경우
        if (obj is not (int boardId, int hnsId))
        {
            Debug.LogError("[OnSelectCurrentSensor] 잘못된 파라미터 전달됨 (expected: (int, int))");
            return;
        }

        // ToxinData 조회
        toxinData = modelProvider.GetToxin(boardId, hnsId);

        if (toxinData == null)
        {
            Debug.LogError($"[OnSelectCurrentSensor] 해당 센서 데이터 없음 (boardId={boardId}, hnsId={hnsId})");
            return;
        }

        if (toxinData.values == null || toxinData.values.Count == 0)
        {
            Debug.LogWarning($"[OnSelectCurrentSensor] 센서 값이 비어 있음 (boardId={boardId}, hnsId={hnsId})");
            return;
        }

        // 이름 표시
        txtName.text = toxinData.hnsName;

        // 정규화 후 라인 그래프 업데이트
        List<float> normalizedValues = new();
        float max = Math.Max(toxinData.values.Max(), toxinData.warning);
        toxinData.values.ForEach(val => normalizedValues.Add(val / max));

        SetVertical(max);
        SetDynamicHours(1);
        line.UpdateControlPoints(normalizedValues);

        Debug.Log($"[OnSelectCurrentSensor] 그래프 정상 업데이트 완료 (boardId={boardId}, hnsId={hnsId})");
    }

    private void InitializeDropdown()
    {
        if (periodDropdown == null)
        {
            Debug.LogError("Dropdown 객체가 할당x");
            return;
        }

        var options = new List<string> { "1일", "7일", "30일" };
        periodDropdown.ClearOptions();
        periodDropdown.AddOptions(options);

        periodDropdown.onValueChanged.AddListener(value =>
        {
            int periodDays = GetSelectedPeriodDays();
            Debug.Log($"기간 선택: {periodDays}일");
            //TODO Set Period
        });

        Debug.Log("Dropdown 초기화 완료");
    }


    internal void OnSelectLog(object data)
    {
        if (data is LogData logData)
        {
            this.aladt = logData.time;
            UpdateChartData(1);
        }
    }

    internal void OnSelectToxin(object data)
    {
        if (data is ToxinData toxinData)
        {
            this.toxinData = toxinData;
            this.txtName.text = toxinData.hnsName;
            this.btnDetail.SetActive(true);

            int periodDays = GetSelectedPeriodDays();
            UpdateChartData(periodDays);
        }
    }

    private int GetSelectedPeriodDays()
    {
        int periodDays = 1;
        switch (periodDropdown.value)
        {
            case 0:
                periodDays = 1;  // 1일
                break;
            case 1:
                periodDays = 7;  // 7일
                break;
            case 2:
                periodDays = 30; // 30일
                break;
        }
        return periodDays;
    }

    private void UpdateChartData(int periodDays)
    {
        if (toxinData == null)
        {
            Debug.LogWarning("ToxinData is null. 랜덤 데이터로 그래프를 초기화합니다.");
            return;
        }

        var convertedData = ConvertToChartData(toxinData);
        line.UpdateControlPoints(convertedData);
        SetVertical(Mathf.Max( convertedData.Max(), toxinData.warning));
        SetDynamicHours(periodDays);
    }


    private List<float> ConvertToChartData(ToxinData toxin)
    {
        if (toxin == null) throw new Exception("DetailToxinBar.toxinData is null. Cannot draw the graph!");

        var max = toxin.values.Max();
        var lchart = new List<float>();
        if (max > toxin.warning)
        {
            lchart = toxin.values.Select(t => t / max).ToList();
        }
        else
        {
            max = toxin.warning;
            lchart = toxin.values.Select(t => t / toxin.warning).ToList();
        }
        SetVertical(max);
        return lchart;
    }

    private void SetDynamicHours(int periodDays)
    {
        DateTime endDt = DateTime.Now;
        DateTime startDt = endDt.AddDays(-periodDays);
        var interval = (endDt - startDt).TotalMinutes / (this.hours.Count-1);

        for (int i = 0; i < this.hours.Count; i++)
        {
            var t = startDt.AddMinutes(interval * i);
            this.hours[i].text = t.ToString("MM-dd HH:mm");
        }
    }

    private void SetVertical(float max)
    {
        var verticalMax = max + 1;

        for (int i = 0; i < this.verticals.Count; i++)
        {
            float ratio = ((float)this.verticals.Count - i-1) / (verticals.Count-1);
            //Debug.Log("ratio : " + ratio);
            //Debug.Log("Math.Round((verticalMax * ratio),2) : " + Math.Round((verticalMax * ratio),2));
            this.verticals[i].text = Math.Round((verticalMax * ratio), 2).ToString();
        }
    }

    internal void OnDetailSelect()
    {
        if (toxinData == null)
        {
            Debug.LogWarning("No toxin data available.");
            return;
        }

        UiManager.Instance.Invoke(UiEventType.SelectAlarmSensor, (this.aladt, this.toxinData.boardid, this.toxinData.hnsid));
    }

    private void STORED_OnChangeTrendLine(object data)
    {
        if (data is int periodDays)
        {
            Debug.Log($"ChangeTrendLine 이벤트 수신: {periodDays}일");
            SetDynamicHours(periodDays);
            UpdateChartData(periodDays);
        }
    }




}

