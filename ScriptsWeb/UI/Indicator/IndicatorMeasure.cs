using DG.Tweening;
using Onthesys.ExeBuild;
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
        GameObject pnlCoverDelayedDate;//0610 수정

        //Components
        TMP_Text lblName, /*lblProgressRecent, lblProgressMax,*/ lblValueRecent,/* lblValueMax*/ /*lblIsFixing*/ lblUnit;
        TMP_Text lblCoverDate, lblCoverTime, lblCoverName, lblCoverTitle;

        //Image imgProgressGauge;
        //List<Image> imgMarkerList;

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
            lblName = transform.Find("SensorName").GetComponent<TMP_Text>();
            //imgProgressGauge = transform.Find("Layout").Find("Dashboard").Find("Progress").GetComponent<Image>();
            //lblProgressRecent = transform.Find("Layout").Find("Dashboard").Find("Progress_Value").GetComponent<TMP_Text>();
            //lblProgressMax = transform.Find("Layout").Find("Dashboard").Find("Max").GetComponent<TMP_Text>();
            lblValueRecent = transform.Find("TextSensorValue").GetComponent<TMP_Text>();
            //lblValueMax = transform.Find("Text (TMP) List (2)").GetComponent<TMP_Text>();
            //imgMarkerList = transform.Find("Layout").Find("Markers").GetComponentsInChildren<Image>().ToList();
            //lblIsFixing = transform.Find("Inspection").GetComponent<TMP_Text>();
            lblUnit = transform.Find("TextUnit").GetComponent<TMP_Text>();

            //0610 수정
            pnlCoverDelayedDate = transform.Find("pnlCover").gameObject;
            lblCoverDate = pnlCoverDelayedDate.transform.Find("txtDate").GetComponent<TMP_Text>();
            lblCoverTime = pnlCoverDelayedDate.transform.Find("txtTime").GetComponent<TMP_Text>();
            lblCoverTitle = pnlCoverDelayedDate.transform.Find("txtTitle").GetComponent<TMP_Text>();
            lblCoverName = pnlCoverDelayedDate.transform.Find("txtSensorName").GetComponent<TMP_Text>();
            //maintenancePanel.SetActive(false);

            GetComponentInParent<Button>().onClick.AddListener(() =>
            {
                UiManager.Instance.Invoke(UiEventType.SelectSensorWithinTab, sensorId);
            });
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

        DateTime GetMaintenanceStartTime()
        {
            // 통신이 끊긴 시간 = 마지막으로 데이터를 받은 시간
            var recentModel = modelProvider.GetMeasureRecentBySensor(boardId, sensorId);
            if (recentModel != null)
            {
                return recentModel.MeasuredTime.AddHours(9);
            }

            return DateTime.Now;
        }

        void UpdateUi()
        {
            if (sensorData == null) return;

            lblName.text = sensorData.sensor_name;
            //lblIsFixing.gameObject.SetActive(sensorData.isFixing);
            if (lblUnit != null)
            {
                lblUnit.text = sensorData.unit;
            }
            
            /*// 테스트용: 특정 센서만 강제로 점검중 표시
            bool testFixing = sensorData.isFixing;
            if (boardId == 1 && sensorId == 1) // 센서1-1 테스트
            {
                testFixing = true; // 이 센서만 강제로 점검중
            }
            maintenancePanel.gameObject.SetActive(testFixing);
*/
            //0610 수정       

            TimeSpan delayment = DateTime.UtcNow.AddHours(9) - GetMaintenanceStartTime();
            //Debug.Log($"[IndicatorMeasure] Delayment for sensor {sensorData.sensor_name} (Board {boardId}, Sensor {sensorId}): {delayment.TotalMinutes} minutes");
            if (sensorData.isFixing || delayment > new TimeSpan(0,5,0)) //테스트로 1분 지연을 임계치로
            {
                pnlCoverDelayedDate.gameObject.SetActive(true);
                DateTime maintenanceTime = GetMaintenanceStartTime();

                lblCoverDate.text = (DateTime.UtcNow.AddHours(9).Date != maintenanceTime.Date) ? maintenanceTime.ToString("yy-MM-dd") : "";
                lblCoverTime.text = maintenanceTime.ToString("HH:mm:ss");
                lblCoverName.text = $"{sensorData.sensor_name}({(sensorData.board_id == 1 ? "상류" : "하류")})";//0611 센서명 추가

                lblCoverTitle.text = sensorData.isFixing? "센서 점검 중" : "데이터 불러오기 지연";
            }
            else 
            {
                pnlCoverDelayedDate.gameObject.SetActive(false);
            }

            DOVirtual.Float(float.Parse(lblValueRecent.text), measuredValue, 0.4f, value =>
            {
                lblValueRecent.text = value.ToString("F1"); // 소수점 첫째자리
            });

        }
    }
}
