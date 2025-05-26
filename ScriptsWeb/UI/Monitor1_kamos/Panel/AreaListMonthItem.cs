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
using static Unity.Burst.Intrinsics.X86.Avx;

internal class AreaListMonthItem : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;
    int areaId;

    TMP_Text lblAreaName, lblPercentage;
    Image imgColor;
    Button btnNavigateArea;
    List<GetNumData> numDatas;

    private void Start()
    {
        lblAreaName = transform.Find("Text (TMP)_Content").GetComponent<TMP_Text>();
        lblPercentage = transform.Find("Text (TMP)_Percent").GetComponent<TMP_Text>();

        numDatas = GetComponentsInChildren<GetNumData>().ToList();

        btnNavigateArea = GetComponent<Button>();
        btnNavigateArea.onClick.AddListener(OnClick);

        imgColor = transform.Find("Label_Colors").GetComponent<Image>();
    }

    internal void SetAreaData(Color color, int areaId, string areaName, int count, float percent)
    {
        imgColor.color = color;
        this.areaId = areaId;
        lblAreaName.text = areaName;

        numDatas.ForEach(numData => numData.ForcedUpdateView(count));

        lblPercentage.text = "" + Mathf.FloorToInt(percent * 100f) + " %";
    }

    void OnClick()
    {
        if (areaId < 1) return;
        UiManager.Instance.Invoke(UiEventType.NavigateArea, areaId);
    }

}