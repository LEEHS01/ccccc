using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Onthesys.WebBuild
{
    internal class IndicatorMeasure : MonoBehaviour
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        //Data
        public int boardId, sensorId;
        SensorModel sensorData;
        float measuredValue;

        //Components
        TMP_Text lblName, lblProgressRecent, lblProgressMax, lblValueRecent, lblValueMax;
        Image imgProgressGauge;
        List<Image> imgMarkerList;

        //Constants
        static Dictionary<StatusType, Color> statusColorDic = new();

        #region [Initiating]
        static IndicatorMeasure()
        {
            Dictionary<StatusType, string> rawColorSets = new() {
                { StatusType.NORMAL,    "#00FBFF"},
                { StatusType.SERIOUS,   "#88FF22"},
                { StatusType.WARNING,   "#FFFF44"},
                { StatusType.CRITICAL,  "#FF4444"},
                { StatusType.ERROR,     "#FF0000"},
            };

            Color color;
            foreach (var pair in rawColorSets)
                if (ColorUtility.TryParseHtmlString(htmlString: pair.Value, out color))
                    statusColorDic[pair.Key] = color;
        }

        private void Awake()
        {
            lblName = transform.Find("Text (TMP) List").GetComponent<TMP_Text>();
            imgProgressGauge = transform.Find("Layout").Find("Dashboard").Find("Progress").GetComponent<Image>();
            lblProgressRecent = transform.Find("Layout").Find("Dashboard").Find("Progress_Value").GetComponent<TMP_Text>();
            lblProgressMax = transform.Find("Layout").Find("Dashboard").Find("Max").GetComponent<TMP_Text>();
            lblValueRecent = transform.Find("Text (TMP) List (1)").GetComponent<TMP_Text>();
            lblValueMax = transform.Find("Text (TMP) List (2)").GetComponent<TMP_Text>();
            imgMarkerList = transform.Find("Layout").Find("Markers").GetComponentsInChildren<Image>().ToList();
        }

        private void Start()
        {
            UiManager.Instance.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance.Register(UiEventType.ChangeRecentValue, OnChangeRecentValue);
        }

        #endregion [Initiating]


        void OnRectTransformDimensionsChange() => UpdateUi();

        private void OnChangeRecentValue(object obj)
        {
            MeasureModel recentModel = modelProvider.GetMeasureRecentBySensor(boardId, sensorId);
            measuredValue = recentModel is not null ? recentModel.measured_value : 0f;
            UpdateUi();
        }

        private void OnInitiate(object obj)
        {
            sensorData = modelProvider.GetSensor(boardId, sensorId);
            var recentModel = modelProvider.GetMeasureRecentBySensor(boardId, sensorId);
            measuredValue = recentModel is not null? recentModel.measured_value : 0f;

            UpdateUi();
        }

        void UpdateUi()
        {
            if (sensorData == null) return;

            lblName.text = sensorData.sensor_name;
            lblProgressMax.text = lblValueMax.text = "" + sensorData.threshold_critical;
            //lblProgressRecent.text = lblValueRecent.text = "" + measuredValue.ToString("0.0");

            DOVirtual.Float(float.Parse(lblValueRecent.text), measuredValue, 0.4f, value =>
            {
                lblProgressRecent.text = value.ToString("F2"); // 소수점 둘째자리까지
                lblValueRecent.text = value.ToString("F2"); // 소수점 둘째자리까지
            });

            float denominator = Mathf.Max(sensorData.threshold_critical, 1f);
            float clampedRatio = Mathf.Min(measuredValue / denominator, 1f);
            imgProgressGauge.DOFillAmount (clampedRatio, 0.4f);

            StatusType status = modelProvider.GetStatusBySensor(boardId, sensorId);
            imgProgressGauge.DOColor(statusColorDic[status], 0.4f);
            lblProgressRecent.DOColor(statusColorDic[status], 0.4f);



            RectTransform markerParent = imgMarkerList.First().rectTransform.parent.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(markerParent);

            List<(StatusType status, string objName, float threshold)> dataGrid = new()
            {
                (StatusType.SERIOUS, "Serious", sensorData.threshold_serious),
                (StatusType.WARNING, "Warning", sensorData.threshold_warning),
                (StatusType.CRITICAL, "Critical", sensorData.threshold_critical),
            };
            foreach (var data in dataGrid)
            {
                Image imgMarker = imgMarkerList.Find(img => img.name == data.objName);
                imgMarker.color = statusColorDic[data.status];
                float ratio = data.threshold / sensorData.threshold_critical;
                float angle = 270 * (1f - ratio) + 45;
                imgMarker.rectTransform.eulerAngles = new(0, 0, angle);

                float distance = markerParent.rect.width / 2f;
                imgMarker.rectTransform.localPosition =
                    new Vector3(
                    (float)Math.Sin(angle / 180f * Mathf.PI),
                    -(float)Math.Cos(angle / 180f * Mathf.PI), 0) * distance;
                imgMarker.rectTransform.anchoredPosition -= new Vector2(0, 17 * markerParent.rect.height / 200);

            }

            foreach(Transform childTransform in markerParent)
        {
                RectTransform child = childTransform as RectTransform;
                if (child == null) continue;
                if (markerParent.GetComponent<LayoutGroup>() != null) continue;


                Vector2 parentSize = markerParent.rect.size;

                // 자식 위치/크기를 기준으로 anchorMin/max 계산
                Vector2 newAnchorMin = new Vector2(
                    (child.localPosition.x - child.rect.width * child.pivot.x) / parentSize.x + markerParent.pivot.x,
                    (child.localPosition.y - child.rect.height * child.pivot.y) / parentSize.y + markerParent.pivot.y
                );

                Vector2 newAnchorMax = new Vector2(
                    (child.localPosition.x + child.rect.width * (1 - child.pivot.x)) / parentSize.x + markerParent.pivot.x,
                    (child.localPosition.y + child.rect.height * (1 - child.pivot.y)) / parentSize.y + markerParent.pivot.y
                );

                // 앵커 설정
                child.anchorMin = newAnchorMin;
                child.anchorMax = newAnchorMax;

                // 마진 초기화 (Top/Bottom/Left/Right = 0)
                child.offsetMin = Vector2.zero;
                child.offsetMax = Vector2.zero;
            }
        }
    }
}
