
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Onthesys.WebBuild
{

    /// <summary>
    /// 기록조회 탭에서 사용하는 두 트렌드 중 하나로 상하류 계측기의 값 사이의 차를 구하는데 사용합니다.
    /// </summary>
    internal class TrendLineDifference : MaskableGraphic
    {
        //enum TabType { History, /*Denoise, Inference*/ }
        //TabType type;

        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        //Data
        SensorModel sensorData;

        (List<MeasureModel> upper, List<MeasureModel> lower) sensorLogs = (new(), new());
        (DateTime from, DateTime to) datetime;
        
        //Func
        List<Vector2> ControlPoints => dots.Select(dot =>
            new Vector2(dot.localPosition.x, dot.localPosition.y)
          + new Vector2(dot.parent.localPosition.x, dot.parent.localPosition.y)
                ).ToList();
        //두 리스트 간의 차를 구해야하므로 다른 방식으로 구해야함.
        //float MaxValue => Mathf.Max(sensorLogs.upper.Select(log => log.measured_value).Max(), sensorLogs.lower.Select(log => log.measured_value).Max(), 0.1f);

        //Components
        List<RectTransform> dots = new();
        List<TMP_Text> lblHourList;
        List<TMP_Text> lblAmountList;

        //Constants
        const float thickness = 2f;
        static Dictionary<StatusType, Color> statusColorDic = new();


        #region [Initiating]
        static TrendLineDifference()
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

        protected override void Awake()
        {
            lblAmountList = transform.Find("Chart_Grid").Find("Text_Vertical").GetComponentsInChildren<TMP_Text>().ToList();
            lblHourList = transform.Find("Chart_Grid").Find("Text_Horizon").GetComponentsInChildren<TMP_Text>().ToList();

            dots = transform.Find("Chart_Dots").GetComponentsInChildren<RectTransform>().ToList();
            dots.Remove(transform.Find("Chart_Dots").GetComponent<RectTransform>());
        }
        protected override void Start()
        {
            if (!Application.isPlaying) return;

            ////부모가 가진 탭 View 타입을 통해 표현할 자료에 따라 탭 유형을 결정
            //if (GetComponentInParent<PanelHistory>() is not null)
            //    type = TabType.History;
            ////else if (GetComponentInParent<PanelDenoise>() is not null)
            ////    type = TabType.Denoise;
            ////else if (GetComponentInParent<PanelInference>() is not null)
            ////    type = TabType.Inference;
            //else Destroy(this.gameObject);

            ////탭 유형을 R&R 이벤트 쌍으로 변환
            //(UiEventType response, UiEventType request) eventPair = type switch
            //{
            //    TabType.History => (UiEventType.ChangeTrendLineHistory, UiEventType.RequestSearchHistory),
            //    //TabType.Denoise => (UiEventType.ChangeTrendLineDenoised, UiEventType.RequestSearchDenoised),
            //    //TabType.Inference => (UiEventType.ChangeTrendLineInference, UiEventType.RequestSearchInference),
            //    _ => throw new NotImplementedException(),
            //};

            //얻은 이벤트를 통해 이벤트 리스너 등록
            UiManager.Instance.Register(UiEventType.ChangeTrendLineHistory, OnChangeTrendLine);
            UiManager.Instance.Register(UiEventType.RequestSearchHistory, OnRequestSearch);

            base.Start();
        }
        #endregion

        #region [Draw]

        protected override void OnRectTransformDimensionsChange() => UpdateUi();

        void Update()
        {
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
                        
            if (ControlPoints.Count < 2) return;

            DrawLines(vh, ControlPoints);
        }

        private void DrawLines(VertexHelper vh, List<Vector2> points)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Color colorStart = dots[i + 1].GetComponent<Image>().color;
                Color colorEnd = dots[i].GetComponent<Image>().color;
                AddVerticesForLineSegment(vh, points[i + 1], points[i], colorStart, colorEnd, thickness);
            }
        }

        private void AddVerticesForLineSegment(VertexHelper vh, Vector2 start, Vector2 end, Color colorStart, Color colorEnd, float thickness)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * thickness / 2;
            vh.AddVert(start + normal,  colorStart, new Vector2(0, 0));
            vh.AddVert(start - normal,  colorStart, new Vector2(0, 1));
            vh.AddVert(end - normal,    colorEnd, new Vector2(1, 1));
            vh.AddVert(end + normal,    colorEnd, new Vector2(1, 0));

            int baseIndex = vh.currentVertCount;
            vh.AddTriangle(baseIndex - 4, baseIndex - 3, baseIndex - 2);
            vh.AddTriangle(baseIndex - 2, baseIndex - 1, baseIndex - 4);
        }
        #endregion

        #region [EventListeners]
        private void OnRequestSearch(object obj)
        {
            if (obj is not (int sensorId, DateTime fromDt, DateTime toDt)) return;

            sensorData = modelProvider.GetSensor(1, sensorId);
            datetime = (fromDt, toDt);

            UpdateUi();
        }

        private void OnChangeTrendLine(object obj)
        {
            //sensorLogs = type switch
            //{
            //    TabType.History => modelProvider.GetMeasureHistoryList(),
            //    //TabType.Denoise => modelProvider.GetMeasureDenoisedList(),
            //    //TabType.Inference => modelProvider.GetMeasureInferenceList(),
            //    _ => throw new NotImplementedException(),
            //};
            var rawLogs = modelProvider.GetMeasureHistoryList().ToList();


            sensorLogs = (
                upper: rawLogs.Where(log => log.board_id == 1).ToList(),
                lower: rawLogs.Where(log => log.board_id == 2).ToList()
            );


            if(sensorLogs.upper.Count != sensorLogs.lower.Count && sensorLogs.lower.Count > dots.Count)
                throw new Exception("TrendLineDifference - OnChangeTrendLine - 계측값의 개수가 그래프에 표시할 수 있는 점의 개수보다 많습니다. 그래프를 표시할 수 없습니다.");
            //while (sensorLogs.upper.Count != sensorLogs.lower.Count && sensorLogs.lower.Count > dots.Count)
            //    sensorLogs.Remove(sensorLogs.First());

            UpdateUi();
        }

        #endregion

        void UpdateUi() 
        {
            if (sensorData is null) return;

            List<MeasureModel> diffs = sensorLogs.lower
                .Zip(sensorLogs.upper, (lower, upper) => new MeasureModel
                {
                    board_id = lower.board_id,
                    sensor_id = lower.sensor_id,
                    measured_time = lower.measured_time,
                    measured_value = lower.measured_value - upper.measured_value
                }).ToList();


            //수직 축(값) 설정
            UpdateAmountLabels(diffs);

            //수평 축(시간) 설정
            UpdateTimeLabels();

            //실제 계측값들을 그래프에 적용
            if (sensorLogs.upper.Count != sensorLogs.lower.Count || sensorLogs.lower.Count != dots.Count) return;

            UpdateTrendLine(diffs);
        }
    
        void UpdateAmountLabels(List<MeasureModel> diffs)
        {
            if (sensorLogs.lower.Count < 1 || sensorLogs.upper.Count < 1) return;

            float maxVal = diffs.Select(val => Mathf.Abs(val.measured_value)).Max();
            maxVal = Math.Max(maxVal, 0.1f); 

            foreach (var lbl in lblAmountList)
            {
                float value = maxVal / (lblAmountList.Count - 1) * (lblAmountList.Count - 1 - lblAmountList.IndexOf(lbl));
                value = value*2f - maxVal; 
                DOTween.To(() => lbl.rectTransform.anchoredPosition.y,
                    value => lbl.text = value.ToString("F1"),
                    value, 0.4f);
            }

        }

        void UpdateTimeLabels()
        {
            lblHourList.ForEach(item =>
            {
                float ratio = (float)lblHourList.IndexOf(item) / (lblHourList.Count - 1);
                DateTime dt = datetime.from + (datetime.to - datetime.from) * ratio;
                TimeSpan timeSpan = datetime.to - datetime.from;

                if (timeSpan.TotalDays < 1f)
                    item.text = dt.ToString("HH:mm");
                else if (timeSpan.TotalDays < 4f)
                    item.text = dt.ToString("yy-MM-dd\nHH:mm");
                else
                    item.text = dt.ToString("yy-MM-dd");
            });
        }

        void UpdateTrendLine(List<MeasureModel> diffs) 
        {
            if (dots.Count == 0) return;

            RectTransform parentRect = transform.Find("Chart_Dots").GetComponent<RectTransform>();
            Vector2 parentSize = parentRect.rect.size;
            Vector2 anchorPos = dots.First().anchorMin;

            float bottomY = anchorPos.y;
            float topY = anchorPos.y + parentSize.y;

            float maxVal = diffs.Select(val => Mathf.Abs(val.measured_value)).Max();
            maxVal = Math.Max(maxVal, 0.1f); 

            for (int i = dots.Count-1; i >= 0; i--)
            {
                RectTransform childRect = dots[i];
                float measuredValue =/* i==0? 0f : */diffs[i].measured_value;
                float measuredRatio = measuredValue / maxVal * 0.5f;    //음과 양 모두 표현하기에 0.5에서 시작

                float targetY = Mathf.LerpUnclamped(bottomY, topY, measuredRatio);
                Vector2 newPos = new(anchorPos.x, targetY);

                childRect.DOAnchorPos(newPos, 0.4f);

                StatusType status = modelProvider.GetStatusBySensorAndValue(sensorData.board_id, sensorData.sensor_id, measuredValue);
                childRect.GetComponent<Image>().color = statusColorDic[status];
            }

            
        }
    }
}
