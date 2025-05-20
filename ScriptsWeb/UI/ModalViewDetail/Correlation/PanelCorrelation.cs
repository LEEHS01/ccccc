using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PanelCorrelation : MonoBehaviour
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;

    (int boardId, int sensorId) sensorAddress;
    List<CorrelationModel> correlations;

    private void Start()
    {
        UiManager.Instance.Register(UiEventType.SelectSensorWithinTab, OnSelectSensorWithinTab);
    }

    void OnSelectSensorWithinTab(object obj)
    {
        if (obj is not (int boardId, int sensorId)) return;
        sensorAddress = (boardId, sensorId);
        correlations = modelProvider.GetCorrelationBySensor(boardId, sensorId);

        UpdateUi();
    }

    private void UpdateUi()
    {
        throw new NotImplementedException();
    }
}