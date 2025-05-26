using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

using Onthesys.ExeBuild;
internal class AreaListYear : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;
    List<AreaListYearItem> items = new();

    private void Start()
    {
        var items = transform.Find("List_Panel").GetComponentsInChildren<AreaListYearItem>();
        this.items.AddRange(items);

        UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
        UiManager.Instance.Register(UiEventType.ChangeAlarmYearly, OnInitiate);
    }

    private void OnInitiate(object obj)
    {
        var alarmYearlyList = modelProvider.GetAlarmYearly();

        //DB에서 받은 데이터가 없는 경우, 시연용 데이터로 대체
        if (alarmYearlyList.Count == 0)
            alarmYearlyList = new() {
                (4,new(18,10,9,11)),(5,new(15,9,6,9)),(10,new(14,6,5,7)),(9,new(13,5,3,4)),(3,new(12,2,1,4))
            };

        for (int i = 0; i < items.Count; i++)
        {
            AreaListYearItem item = items[i];

            if (i < alarmYearlyList.Count)
            {
                (int, AlarmCount) alarmYearly = alarmYearlyList[i];
                AreaData area = modelProvider.GetArea(alarmYearly.Item1);

                item.SetAreaData(area.areaId, area.areaName, alarmYearly.Item2);
            }
            else
            {
                item.SetAreaData(-1, "-", new(0,0,0,0));
            }
        }
    }
}