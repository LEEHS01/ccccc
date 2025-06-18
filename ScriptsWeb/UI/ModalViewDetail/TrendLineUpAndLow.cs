
using Assets.ScriptsWeb.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Onthesys.WebBuild
{

    /// <summary>
    /// 기록조회 탭에서 사용하는 두 트렌드 중 하나로 상하류 계측기의 실제 값을 동시에 표시하는데 사용됩니다.
    /// </summary>
    internal class TrendLineUpAndLow : MaskableGraphic
    {
        //enum TabType { History, /*Denoise, Inference*/ }
        //TabType type;

        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        //Data
        SensorModel sensorData;
        (List<MeasureModel> upper, List<MeasureModel> lower) sensorLogs = (new(),new());
        (DateTime from, DateTime to) datetime;
        new (Color upper, Color lower) color = (new Color(0, 1, 1), new Color(1, 1, 0));

        //Func
        (List<Vector2> upper, List<Vector2> lower) ControlPoints => (
            dots.upper.Select(dot =>
                new Vector2(dot.localPosition.x, dot.localPosition.y) +
                new Vector2(dot.parent.localPosition.x, dot.parent.localPosition.y)
                ).ToList(),
            dots.lower.Select(dot =>
                new Vector2(dot.localPosition.x, dot.localPosition.y) +
                new Vector2(dot.parent.localPosition.x, dot.parent.localPosition.y)
                ).ToList()
            );
        //float MaxValue => Mathf.Max(sensorLogs.upper.Select(log => log.measured_value).Max(), sensorLogs.lower.Select(log => log.measured_value).Max(), 0.1f);

        float GetFixedMaxValue()
        {
            if (sensorData == null) return 300f;

            return sensorData.sensor_id switch
            {
                1 => 300f,   // 센서1: 0~300 범위
                2 => 300f,   // 센서2: 0~300 범위  
                3 => 4000f,  // 센서3: 0~4000 범위
                _ => 300f    // 기본값
            };
        }

        float MaxValue => GetFixedMaxValue();

        //Components
        //TMP_Text lblName;
        (List<RectTransform> upper, List<RectTransform> lower) dots = (new(), new());
        List<TMP_Text> lblHourList;
        List<TMP_Text> lblAmountList;
        //List<Image> imgAmountList;

        //Constants
        const float thickness = 2f;
        static Dictionary<StatusType, Color> statusColorDic = new();


        #region [Initiating]
        static TrendLineUpAndLow()
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
            //lblName = transform.Find("Title_Image").GetComponentInChildren<TMP_Text>();

            lblAmountList = transform.Find("Chart_Grid").Find("Text_Vertical").GetComponentsInChildren<TMP_Text>().ToList();
            lblHourList = transform.Find("Chart_Grid").Find("Text_Horizon").GetComponentsInChildren<TMP_Text>().ToList();
            //imgAmountList = transform.Find("Chart_Grid").Find("Lines_Vertical").GetComponentsInChildren<Image>().ToList();

            dots.upper = transform.Find("dotsUpper").GetComponentsInChildren<RectTransform>().ToList();
            dots.upper.Remove(transform.Find("dotsUpper").GetComponent<RectTransform>());
            dots.lower = transform.Find("dotsLower").GetComponentsInChildren<RectTransform>().ToList();
            dots.lower.Remove(transform.Find("dotsLower").GetComponent<RectTransform>());

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

            DrawLines(vh, dots.upper, ControlPoints.upper, color.upper);
            DrawLines(vh, dots.lower, ControlPoints.lower, color.lower);
        }

        private void DrawLines(VertexHelper vh, List<RectTransform> rects, List<Vector2> points, Color color)
        {
            try
            {
                for (int i = 0; i < points.Count - 1; i++)
                    AddVerticesForLineSegment(vh, points[i + 1], points[i], color, color, thickness);
            }
            catch (Exception e) 
            {
                Debug.LogError($"TrendLineUpAndLow - OnPopulateMesh - {e.GetType()} : {e.Message}");
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
            Debug.Log("TrendLineUpAndLow - OnChangeTrendLine");
            //sensorLogs = type switch
            //{
            //    TabType.History => modelProvider.GetMeasureHistoryList(),
            //    //TabType.Denoise => modelProvider.GetMeasureDenoisedList(),
            //    //TabType.Inference => modelProvider.GetMeasureInferenceList(),
            //    _ => throw new NotImplementedException(),
            //};
            var rawLogs = modelProvider.GetMeasureHistoryList();
            sensorLogs = (
                rawLogs.Where(log => log.board_id == 1).ToList(), 
                rawLogs.Where(log => log.board_id == 2).ToList());

            //Debug.Log($"rawLogs.Count : {rawLogs.Count}");

            while (sensorLogs.lower.Count > dots.lower.Count)
                sensorLogs.lower.Remove(sensorLogs.lower.First());
            while (sensorLogs.upper.Count > dots.upper.Count)
                sensorLogs.upper.Remove(sensorLogs.upper.First());

            //Debug.Log($"aft TrendLineUpAndLow - Awake - dots.upper.Count: {dots.upper.Count}, dots.lower.Count: {dots.lower.Count}");
            //Debug.Log($"aft TrendLineUpAndLow - OnChangeTrendLine - sensorLogs.upper.Count: {sensorLogs.upper.Count}, sensorLogs.lower.Count: {sensorLogs.lower.Count}");
            UpdateUi();
        }

        #endregion

        void UpdateUi()
        {
            //Debug.Log("UpdateUi");
            if (sensorData is null) return;

            //수직 축(값) 설정
            UpdateAmountLabels();

            //수평 축(시간) 설정
            UpdateTimeLabels();

            //실제 계측값들을 그래프에 적용
            if (sensorLogs.upper.Count != (dots.upper.Count)) return;
            if (sensorLogs.lower.Count != (dots.lower.Count)) return;

            UpdateTrendLine(dots.upper, sensorLogs.upper);
            UpdateTrendLine(dots.lower, sensorLogs.lower);
        }
    
        void UpdateAmountLabels()
        {
            if (sensorLogs.lower.Count < 1 || sensorLogs.upper.Count < 1) return;

            foreach (var lbl in lblAmountList)
            {
                int index = lblAmountList.IndexOf(lbl);

                // 깔끔한 값으로 계산
                float value = GetCleanLabelValue(MaxValue, index, lblAmountList.Count);

                //float value = MaxValue / (lblAmountList.Count-1) * (lblAmountList.Count - 1 - lblAmountList.IndexOf(lbl));
                DOTween.To(() => lbl.rectTransform.anchoredPosition.y,
                    value => lbl.text= value.ToString("F0"),
                    value, 0.4f);
            }

        }

        float GetCleanLabelValue(float maxValue, int index, int totalCount)
        {
            // 위에서부터 아래로 (index 0 = 최댓값, 마지막 index = 0)
            float ratio = (float)(totalCount - 1 - index) / (totalCount - 1);
            float rawValue = maxValue * ratio;

            // 센서별 깔끔한 간격으로 반올림
            return sensorData.sensor_id switch
            {
                1 or 2 => RoundToCleanValue(rawValue, 60f),
                3 => RoundToCleanValue(rawValue, 500f), 
                _ => RoundToCleanValue(rawValue, 50f)
            };
        }

        float RoundToCleanValue(float value, float step)
        {
            return Mathf.Round(value / step) * step;
        }

        void UpdateTimeLabels() 
        {
            lblHourList.ForEach(item =>
            {
                float ratio = (float)lblHourList.IndexOf(item) / (lblHourList.Count - 1);
                DateTime dt = datetime.from + (datetime.to - datetime.from) * ratio; 
                TimeSpan timeSpan = datetime.to - datetime.from;

                if(timeSpan.TotalDays < 1f)
                    item.text = dt.ToString("HH:mm");
                else if (timeSpan.TotalDays < 4f)
                    item.text = dt.ToString("yy-MM-dd\nHH:mm");
                else
                    item.text = dt.ToString("yy-MM-dd");
            });
        }

        void UpdateTrendLine(List<RectTransform> dots, List<MeasureModel> sensorLogs) 
        {
            if (dots.Count < 1) return;

            RectTransform parentRect = dots.First().parent.GetComponent<RectTransform>();

            //Debug.LogError("parentRect.name : " + parentRect.name);

            float parentHeight = parentRect.rect.height;

            for (int i = dots.Count - 1; i >= 0; i--)
            {
                RectTransform childRect = dots[i];
                float measuredValue = sensorLogs[i].measured_value;
                float measuredRatio = measuredValue / MaxValue;

                Vector2 anchorPos = childRect.anchorMin;

                float bottomY = anchorPos.y;
                float topY = anchorPos.y + parentHeight;
                float targetY = Mathf.Lerp(bottomY, topY, measuredRatio);
                //Debug.Log($"measuredValue = {measuredValue} / MaxValue = {MaxValue} / targetY = {targetY} / bottomY = {bottomY} / topY = {topY}");
                Vector2 newPos = new(anchorPos.x, targetY);

                childRect.DOAnchorPos(newPos, 0.4f);

                //StatusType status = modelProvider.GetStatusBySensorAndValue(sensorData.board_id, sensorData.sensor_id, measuredValue);
                //childRect.GetComponent<Image>().color = statusColorDic[status];

            }
        }
    }
}
