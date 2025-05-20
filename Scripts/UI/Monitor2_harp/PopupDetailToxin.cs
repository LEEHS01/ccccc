using Assets.Scripts.Data;
using Onthesys;
using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;


internal class PopupDetailToxin : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    LogData log;
    private ToxinData data;

    internal TMP_Text txtName;
    internal TMP_Text txtCurrent;
    internal TMP_Text txtTotal;
    internal List<ChartBar> bars;

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.SelectAlarm, OnSelectAlarm);
        UiManager.Instance.Register(UiEventType.Popup_AiTrend, OnSelectChart);
        gameObject.SetActive(false);
    }

    private void OnSelectAlarm(object obj)
    {
        if (obj is not int logIdx) return;

        LogData log = modelProvider.GetAlarm(logIdx);

        this.log = log;

    }


    internal void OnSelectChart(object data)
    {
        gameObject.SetActive(true);

        if (data is not ValueTuple<int, int> tuple) return;

        int boardId = tuple.Item1;
        int hnsId = tuple.Item2;

        ToxinData toxin = modelProvider.GetToxinsInLog()
        .Find(toxin => toxin.boardid == boardId && toxin.hnsid == hnsId);

        if (toxin == null)
        {
            return;
        }

        this.data = toxin;
        this.txtName.text = toxin.hnsName.Replace("\n", string.Empty);
        this.txtCurrent.text = Math.Round(toxin.GetLastValue(), 2).ToString();
        this.txtTotal.text = Math.Round(toxin.warning, 2).ToString();

        // 그래프 바가 제대로 활성화되지 않으면 활성화
        bars.ForEach(bar => bar.gameObject.SetActive(true));

        // 그래프 데이터 추출기 정의 (aiValues, values, diffValues)
        List<Func<ToxinData, List<float>>> valuesExtractors = new()
        {
            toxinData => toxinData.aiValues,
            toxinData => toxinData.values,
            toxinData => toxinData.diffValues,
        };

        if (bars.Count != valuesExtractors.Count)
        {
            Debug.LogWarning($"bars.Count({bars.Count})와 valuesExtractors.Count({valuesExtractors.Count})가 일치하지 않음");
            return;
        }

        // 그래프 데이터 업데이트
        for (int i = 0; i < Mathf.Min(bars.Count, valuesExtractors.Count); i++)
        {
            //Func<ToxinData, List<float>> extractFunc = valuesExtractors[i];
            List<float> extracted = valuesExtractors[i](this.data);


            float max = Mathf.Max(this.data.warning, extracted.Max());

            var chartValues = extracted.Select(value => value / max).ToList();
            bars[i].line.UpdateControlPoints(chartValues);
            bars[i].CreatAxis(log.time, max);
        }
    }


}

