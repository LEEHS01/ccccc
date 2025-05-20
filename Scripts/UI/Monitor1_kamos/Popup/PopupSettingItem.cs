using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class PopupSettingItem : MonoBehaviour
{
    internal Toggle tglVisibility;
    internal TMP_Text lblSensorName;

    int obsId, boardId, hnsId;

    internal bool isValid = false;

    private void Awake()
    {
        tglVisibility = GetComponentInChildren<Toggle>();
        lblSensorName = GetComponentInChildren<TMP_Text>();
    }
    private void Start()
    {

        tglVisibility.onValueChanged.AddListener(OnValueChanged);
    }

    internal void SetItem(int obsId, int boardId, int hnsId, string sensorName, bool isVisible) 
    {
        lblSensorName.text = sensorName;
        tglVisibility.SetIsOnWithoutNotify(isVisible);
        this.obsId = obsId;
        this.boardId = boardId;
        this.hnsId = hnsId;

        isValid = hnsId > 0;
    }
    private void OnValueChanged(bool isVisible)
    {
        //Temporary Function
        UiManager.Instance.Invoke(UiEventType.CommitSensorUsing, (obsId, boardId, hnsId, isVisible));
    }
}