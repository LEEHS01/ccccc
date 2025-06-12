using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Onthesys.WebBuild;
using System.Linq;

internal class GetTime : MonoBehaviour
{
    TextMeshProUGUI txtUpriverTime;
    TextMeshProUGUI txtDownriverTime;

    [Header("보드 ID 설정")]
    public int upriverBoardId = 1;
    public int downriverBoardId = 2;

    ModelProvider modelProvider => UiManager.Instance?.modelProvider;

    void Start()
    {
        txtUpriverTime = transform.Find("upriverMeasure/txtClock").GetComponent<TextMeshProUGUI>();
        txtDownriverTime = transform.Find("downriverMeasure/txtClock").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        UpdateBoardTimes();
    }

    void UpdateBoardTimes()
    {
        if (modelProvider == null) return;

        // 상류 최신 시간 업데이트
        string upriverTime = GetLatestTimeByBoard(upriverBoardId);
        if (txtUpriverTime != null)
            txtUpriverTime.text = $"상류 측정 시간\n{upriverTime}";

        // 하류 최신 시간 업데이트
        string downriverTime = GetLatestTimeByBoard(downriverBoardId);
        if (txtDownriverTime != null)
            txtDownriverTime.text = $"하류 측정 시간\n{downriverTime}";
    }

    string GetLatestTimeByBoard(int boardId)
    {
        var recentData = modelProvider.GetMeasureRecentList();
        if (recentData == null || recentData.Count == 0)
            return "00/00 00:00:00";

        //해당 보드(측정기구)의 모든 센서 데이터 가져오기
        var boardData = recentData.Where(m => m.board_id == boardId).ToList();
        // 예: boardId=1이면 (1,1), (1,2), (1,3) 센서들의 데이터

        if (boardData.Count == 0)
            return "00/00 00:00:00";

        //그 중에서 가장 최신 시간 찾기
        var latestTime = boardData.Max(m => m.MeasuredTime).AddHours(9);
        // 상류 3개 센서 중 가장 최근에 측정된 시간

        return latestTime.ToString("MM/dd HH:mm:ss");
    }

}