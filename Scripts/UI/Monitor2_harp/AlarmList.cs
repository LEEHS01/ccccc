using Onthesys;
using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


internal class AlarmList : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    // 기존 코드와 동일하게 유지
    private List<LogData> list; // 알람 데이터 목록

    internal AlarmListItem itemPrefab;
    internal GameObject itemContainer;

    internal List<AlarmListItem> items;
    internal TMP_Dropdown dropdownMap;
    internal TMP_Dropdown dropdownStatus;


    private int areaIndex = -1; // 선택된 지역 인덱스
    private int statusIndex = -1; // 선택된 상태 인덱스

    #region 버튼안씀
    private Button btnTimeAsc;
    private Button btnTimeDesc;
    private Button btnContentAsc;
    private Button btnContentDesc;
    private Button btnAreaAsc;
    private Button btnAreaDesc;
    private Button btnObsAsc;
    private Button btnObsDesc;
    private Button btnStatusAsc;
    private Button btnStatusDesc;
    #endregion

    void Start()
    {

        this.itemPrefab.gameObject.SetActive(false);

        // 알람 리스트 변경 이벤트 구독
        UiManager.Instance.Register(UiEventType.Initiate, OnUpdateAlarmList);
        UiManager.Instance.Register(UiEventType.ChangeAlarmList, OnUpdateAlarmList);

        // 드롭다운 이벤트 연결
        dropdownStatus.onValueChanged.AddListener(OnStatusFilterChanged);
        dropdownMap.onValueChanged.AddListener(OnAreaFilterChanged);


    }



    // 알람 리스트 업데이트 이벤트 핸들러
    private void OnUpdateAlarmList(object data)
    {
        List<LogData> logs = modelProvider.GetAlarms();
        list = logs;
        // 드롭다운에 지역명 옵션 자동 추가
        var areaNames = logs.Select(log => log.areaName).Distinct().ToList();
        areaNames.Insert(0, "전체");
        dropdownMap.ClearOptions();
        dropdownMap.AddOptions(areaNames);

        // 드롭다운에 상태 옵션 추가 (정수 → 텍스트 매핑)
        var statusOptions = new List<string> { "전체", "설비이상", "경고", "경계" };
        dropdownStatus.ClearOptions();
        dropdownStatus.AddOptions(statusOptions);


        UpdateText();

    }

    private void UpdateText()
    {
        for (int i = 0; i < this.items.Count; i++)
        {
            AlarmListItem item = this.items[i];

            if (item != itemPrefab) DestroyImmediate(this.items[i].gameObject);
        }
        this.items.Clear();

        float height = this.list.Count * this.itemPrefab.GetComponent<RectTransform>().sizeDelta.y;
        for (int i = 0; i < this.list.Count; i++)
        {
            AlarmListItem item = Instantiate(this.itemPrefab);
            item.gameObject.SetActive(true);
            item.transform.SetParent(this.itemContainer.transform);
            item.SetText(this.list[i]);
            //item.GetComponent<Button>().onClick.AddListener(() => OnAlarmSelected(item.data.idx));
            this.items.Add(item);
        }
        this.itemContainer.GetComponent<RectTransform>().sizeDelta =
            new Vector2(this.itemContainer.GetComponent<RectTransform>().sizeDelta.x, height);



    }


    private List<LogData> GetFilteredAlarms()
    {
        List<LogData> alarms = modelProvider.GetAlarms();

        // 지역 필터링
        if (areaIndex > 0)
        {
            string selectedAreaName = dropdownMap.options[areaIndex].text;
            alarms = alarms.Where(a => a.areaName == selectedAreaName).ToList();
        }
        // 상태 필터링
        if (statusIndex >= 0)
        {
            alarms = alarms.Where(a => a.status == statusIndex).ToList();
        }

        return alarms;
    }


    // 알람 필터링 (드롭다운 메뉴에 연결)
    internal void OnAreaFilterChanged(int areaIndex)
    {
        this.areaIndex = areaIndex;
        list = GetFilteredAlarms(); // 필터링 반영
        UpdateText();
    }

    internal void OnStatusFilterChanged(int statusIndex)
    {
        this.statusIndex = statusIndex - 1; // 0: 전체, 1~: 상태코드
        list = GetFilteredAlarms(); // 필터링 반영
        UpdateText();
    }

    private void UpdateFilter()
    {
        for (int i = 0; i < this.items.Count; i++)
        {
            AlarmListItem item = this.items[i]; // ← 정확한 타입으로 수정
            item.gameObject.SetActive(true);

            // Area Filtering
            if (this.areaIndex > 0)
            {
                // Fetch the area by name from ModelProvider
                var area = modelProvider.GetAreaByName(item.data.areaName);
                if (area != null && area.areaId != this.areaIndex)
                {
                    item.gameObject.SetActive(false);
                }
            }

            if (this.statusIndex > -1)
            {
                if (item.data.status != this.statusIndex)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }
    }


    internal void OnClickOrder(string order)
    {
        this.list.Sort((a, b) =>
        {
            if (order == AlramOrder.TIME_UP.ToString())
            {
                return b.time.CompareTo(a.time);
            }
            else if (order == AlramOrder.TIME_DOWN.ToString())
            {
                return a.time.CompareTo(b.time);
            }
            else if (order == AlramOrder.STATUS_UP.ToString())
            {
                return b.hnsName.CompareTo(a.hnsName);
            }
            else if (order == AlramOrder.STATUS_DOWN.ToString())
            {
                return a.hnsName.CompareTo(b.hnsName);
            }
            else if (order == AlramOrder.MAP_UP.ToString())
            {
                return b.areaName.CompareTo(a.areaName);
            }
            else if (order == AlramOrder.MAP_DOWN.ToString())
            {
                return a.areaName.CompareTo(b.areaName);
            }
            else if (order == AlramOrder.AREA_UP.ToString())
            {
                return b.obsName.CompareTo(a.obsName);
            }
            else if (order == AlramOrder.AREA_DOWN.ToString())
            {
                return a.obsName.CompareTo(b.obsName);
            }
            return 0;
        });
        this.UpdateText();
        this.UpdateFilter();
    }
    internal enum AlramOrder
    {
        TIME_UP,
        TIME_DOWN,
        STATUS_UP,
        STATUS_DOWN,
        MAP_UP,
        MAP_DOWN,
        AREA_UP,
        AREA_DOWN,
        VALUE_UP,
        VALUE_DOWN
    };

}

