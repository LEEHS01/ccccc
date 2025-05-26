using Onthesys;
using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;


internal class PopupDetailToxin2 : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    private ToxinData data;
    internal TMP_Text txtName;
    internal TMP_Text txtCurrent;
    internal TMP_Text txtTotal;
    internal UILineRenderer2 line;
    internal List<TMP_Text> hours;
    internal List<TMP_Text> verticals;

    internal GameObject statusGreen;
    internal GameObject statusRed;
    internal GameObject statusYellow;
    internal GameObject statusPurple;


    private void Start()
    {
        UiManager.Instance.Register(UiEventType.SelectAlarmSensor, OnSelectCurrentToxin);
        gameObject.SetActive(false);
    }

    // 이벤트 핸들러 (데이터 처리 로직 추가 예정)
    internal void OnSelectCurrentToxin(object data)
    {
        gameObject.SetActive(true);

        if (data is not (int boardId, int hnsId)) return;

        ToxinData toxin = modelProvider.GetToxin(boardId, hnsId);

        if (toxin == null) return;

        this.data = toxin;
        txtName.text = toxin.hnsName;
        txtCurrent.text = Math.Round(toxin.GetLastValue(), 2).ToString();
        txtTotal.text = Math.Round(this.data.warning, 2).ToString();

        var max = Mathf.Max(this.data.warning, this.data.values.Max());
        var lcahrt = this.data.values.Select(t => t / max).ToList();
        line.UpdateControlPoints(lcahrt);

        // 상태 표시 설정
        SetStatusIcon(this.data.status);

        SetMins(DateTime.Now);
        SetVertical(max);
    }

    // 상태 아이콘 설정 메서드
    private void SetStatusIcon(ToxinStatus status)
    {
        statusGreen.SetActive(status == ToxinStatus.Green);
        statusRed.SetActive(status == ToxinStatus.Red);
        statusYellow.SetActive(status == ToxinStatus.Yellow);
        statusPurple.SetActive(status == ToxinStatus.Purple);
    }

    // 시간 축 설정
    private void SetMins(DateTime dt)
    {
        DateTime startDt = dt.AddHours(-4);
        var interval = (dt - startDt).TotalMinutes / hours.Count;

        for (int i = 0; i < hours.Count; i++)
        {
            var t = dt.AddMinutes(-(interval * i));
            hours[i].text = t.ToString("HH:mm");
        }
    }

    // 세로 축 설정
    private void SetVertical(float max)
    {
        var verticalMax = ((max + 1) / (verticals.Count - 1));

        for (int i = 0; i < verticals.Count; i++)
        {
            verticals[i].text = Math.Round((verticalMax * i), 2).ToString();
        }
    }
}

