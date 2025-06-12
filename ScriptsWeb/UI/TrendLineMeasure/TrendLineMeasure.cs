
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Onthesys.WebBuild
{
    internal class TrendLineMeasure : MaskableGraphic
    {
        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        //Data
        public int boardId, sensorId;
        (DateTime from, DateTime to) datetime;
        SensorModel sensorData;
        List<MeasureModel> sensorLogs = new();
        
        //Func
        List<Vector2> ControlPoints => dots.Select(dot =>
            new Vector2(dot.localPosition.x, dot.localPosition.y)
          + new Vector2(dot.parent.localPosition.x, dot.parent.localPosition.y)
                ).ToList();
        float MaxValue => Mathf.Max(sensorLogs.Select(log => log.measured_value).Max(), 0.1f);
        
        //Components
        TMP_Text /*lblName,*/ lblIsFixing;
        List<RectTransform> dots = new();
        List<TMP_Text> lblHourList;
        List<TMP_Text> lblAmountList;
        List<Image> imgAmountList;

        //Constants
        const float thickness = 2f;
        static Dictionary<StatusType, Color> statusColorDic = new();


        #region [Initiating]
        static TrendLineMeasure()
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
            imgAmountList = transform.Find("Chart_Grid").Find("Lines_Vertical").GetComponentsInChildren<Image>().ToList();

            dots = transform.Find("Chart_Dots").GetComponentsInChildren<RectTransform>().ToList();
            dots.Remove(transform.Find("Chart_Dots").GetComponent<RectTransform>());
            sensorLogs = dots.Select(item => new MeasureModel()).ToList();


            lblIsFixing?.gameObject.SetActive(false);
        }
        protected override void Start()
        {
            if (!Application.isPlaying) return;

            base.Start();
            UiManager.Instance?.Register(UiEventType.Initiate, OnInitiate);
            //UiManager.Instance?.Register(UiEventType.ChangeRecentValue, OnChangeRecentValue);
            UiManager.Instance?.Register(UiEventType.ChangeTrendLine, OnChangeTrendLine);
        }
        #endregion

        #region [Draw]

        protected override void OnRectTransformDimensionsChange() => UpdateUi();

        void Update()
        {
            SetVerticesDirty();
            color = new Color(0, 1, 1);
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
        private void OnChangeRecentValue(object obj)
        {
            var recentModel = modelProvider.GetMeasureRecentBySensor(boardId, sensorId);
            sensorLogs.Add(recentModel);

            while (sensorLogs.Count > dots.Count)
                sensorLogs.Remove(sensorLogs.First());

            UpdateUi();
        }
        private void OnChangeTrendLine(object obj)
        {
            sensorLogs = modelProvider.GetMeasureLogBySensor(boardId, sensorId);

            while (sensorLogs.Count > dots.Count)
                sensorLogs.Remove(sensorLogs.First());

            datetime.from = sensorLogs.First().MeasuredTime;
            datetime.to = sensorLogs.Last().MeasuredTime;

            UpdateUi();
        }
        private void OnInitiate(object obj)
        {
            sensorData = modelProvider.GetSensor(boardId, sensorId);
            UpdateUi();
        }
        #endregion

        void UpdateUi()
        {
            if (sensorData is null) return;

            //제목 설정
            //lblName.text = sensorData.sensor_name;
            lblIsFixing?.gameObject.SetActive(sensorData.isUsing);

            //수직 축(값) 설정
            UpdateAmountLabels();

            //수평 축(시간) 설정
            UpdateTimeLabels();

            //실제 계측값들을 그래프에 적용
            //Debug.Log($"{sensorLogs.Count} != {dots.Count - dotsMargin}?");
            if (sensorLogs.Count != dots.Count) return;
            //Debug.Log($"{sensorLogs.Count} == {dots.Count - dotsMargin}!");
            UpdateTrendLine();
        }
    
        void UpdateAmountLabels()
        {

            if (sensorLogs.Count < 1) return;

            float maxVal = sensorLogs.Select(val => Mathf.Abs(val.measured_value)).Max();


            foreach (var lbl in lblAmountList)
            {
                float val = maxVal / (lblAmountList.Count - 1) * (lblAmountList.Count - 1 - lblAmountList.IndexOf(lbl));
                DOTween.To(() => val,
                    value => lbl.text = value.ToString("F0"),
                    val, 0.4f);
            }


            //if (sensorLogs.Count < 1) return;
            //if (imgAmountList.Count != lblAmountList.Count && lblAmountList.Count != 5) throw new Exception("TrendLineMeasure - UpdateAmountLabels - chartGrid 노트의 구성 요소가 부적절하여 초기화를 진행할 수 없습니다.");

            ////float maxValue = Mathf.Max(sensorLogs.Select(log => log.measured_value).Max(), sensorData.threshold_critical);


            ////실제 최대값
            //if (MaxValue != sensorData.threshold_critical)
            //{
            //    lblAmountList.First().text = MaxValue.ToString("F0"); ;
            //    lblAmountList.First().color = statusColorDic[StatusType.ERROR];
            //    imgAmountList.First().color = statusColorDic[StatusType.ERROR];

            //}
            //else
            //{
            //    lblAmountList[0].text = "";
            //    lblAmountList.First().color = new Color(0,0,0,0);
            //    imgAmountList.First().color = new Color(0, 0, 0, 0);
            //}

            ////영점
            //lblAmountList[lblAmountList.Count - 1].text = "0";


            ////범위
            //RectTransform
            //    lblFrom = lblAmountList.Last().rectTransform,
            //    lblTo = lblAmountList.First().rectTransform,
            //    imgFrom = imgAmountList.Last().rectTransform,
            //    imgTo = imgAmountList.First().rectTransform;

            //((Vector2 min, Vector2 max, Vector2 pos) from, (Vector2 min, Vector2 max, Vector2 pos) to)
            //    labelAnchorPair = (
            //        (lblFrom.anchorMin, lblFrom.anchorMax, lblFrom.anchoredPosition),
            //        (lblTo.anchorMin, lblTo.anchorMax, lblTo.anchoredPosition)),
            //    imageAnchorPair = (
            //        (imgFrom.anchorMin, imgFrom.anchorMax, imgFrom.anchoredPosition),
            //        (imgTo.anchorMin, imgTo.anchorMax, imgTo.anchoredPosition));

            ////임계값들 설정
            //List <(StatusType status, float threshold, int tIdx)> dataGrid = new()
            //{
            //    //(StatusType.NORMAL, 0f, 4),
            //    (StatusType.SERIOUS, sensorData.threshold_serious, 3),
            //    (StatusType.WARNING, sensorData.threshold_warning, 2),
            //    (StatusType.CRITICAL, sensorData.threshold_critical, 1),
            //};
            //foreach (var data in dataGrid) 
            //{
            //    TMP_Text lblAmount = lblAmountList[data.tIdx];
            //    Image imgAmount = imgAmountList[data.tIdx];
            //    float ratio = data.threshold / MaxValue;

            //    lblAmount.rectTransform.anchorMin = Vector2.Lerp(labelAnchorPair.from.min, labelAnchorPair.to.min, ratio);
            //    lblAmount.rectTransform.anchorMax = Vector2.Lerp(labelAnchorPair.from.max, labelAnchorPair.to.max, ratio);
            //    lblAmount.rectTransform.anchoredPosition = Vector2.Lerp(labelAnchorPair.from.pos, labelAnchorPair.to.pos, ratio);
            //    lblAmount.color = statusColorDic[data.status];
            //    lblAmount.text = data.threshold.ToString("F0");

            //    imgAmount.rectTransform.anchorMin = Vector2.Lerp(imageAnchorPair.from.min, imageAnchorPair.to.min, ratio);
            //    imgAmount.rectTransform.anchorMax = Vector2.Lerp(imageAnchorPair.from.max, imageAnchorPair.to.max, ratio);
            //    imgAmount.rectTransform.anchoredPosition = Vector2.Lerp(imageAnchorPair.from.pos, imageAnchorPair.to.pos, ratio);
            //    imgAmount.color = statusColorDic[data.status];
            //}

        }

        void UpdateTimeLabels()
        {
            lblHourList.ForEach(item =>
            {
                float ratio = (float)lblHourList.IndexOf(item) / (lblHourList.Count - 1);
                DateTime dt = datetime.from + (datetime.to - datetime.from) * ratio;
                TimeSpan timeSpan = datetime.to - datetime.from;

                if (timeSpan.TotalDays < 0.9f)
                    item.text = dt.ToString("HH:mm");
                else if (timeSpan.TotalDays < 4f)
                    item.text = dt.ToString("MM-dd HH:mm");
                else
                    item.text = dt.ToString("yy-MM-dd");
            });
        }

        void UpdateTrendLine() 
        {
            RectTransform parentRect = transform.Find("Chart_Dots").GetComponent<RectTransform>();
            Vector2 parentSize = parentRect.rect.size;
            Vector2 anchorPos = dots.First().anchorMin;

            float bottomY = anchorPos.y;
            float topY = anchorPos.y + parentSize.y;

            for (int i = dots.Count - 1; i >= 0; i--)
            {
                RectTransform childRect = dots[i];
                //RectTransform childRectBefore = dots[i - 1];
                float measuredValue = sensorLogs[i].measured_value;
                float measuredRatio = measuredValue / MaxValue;

                float targetY = Mathf.Lerp(bottomY, topY, measuredRatio);

                Vector2 newPos = new(anchorPos.x, targetY);

                //Vector3 difVec = childRect.position - childRectBefore.position;
                //if (i != dots.Count - 1) childRect.anchoredPosition += new Vector2(difVec.x, 0f);

                childRect.DOAnchorPos(newPos, 0.4f);
                //childRect.anchoredPosition = newPos;

                //childRect.anchorMax = childRectBefore.anchorMax;
                //childRect.anchorMin = childRectBefore.anchorMin;
                //StatusType status = modelProvider.GetStatusBySensorAndValue(boardId, sensorId, measuredValue);
            }

            //Debug.Log("DEBUG_VALUE : " + DEBUG_VALUE.ToString() + "\nLOG_COUNT : " + sensorLogs.Count);

            //가장 앞의 노드를 뒤로 이동
            //dots[0].SetAsLastSibling();
            //dots[0].anchorMin = dots[dots.Count - 1].anchorMin;
            //dots[0].anchorMax = dots[dots.Count - 1].anchorMax;
            //dots[0].anchoredPosition = dots[0].anchorMin;

            //dots 자료구조 초기화
            //dots = transform.Find("Chart_Dots").GetComponentsInChildren<RectTransform>().ToList();
            //dots.Remove(transform.Find("Chart_Dots").GetComponent<RectTransform>());

            //Fading 효과
            //DOVirtual.Float(1, 0, 0.4f, value =>
            //{
            //    Image img = dots[0].GetComponent<Image>();
            //    dots[0].GetComponent<Image>().color = new Color
            //    {
            //        r = img.color.r,
            //        g = img.color.g,
            //        b = img.color.b,
            //        a = value,
            //    };

            //    img = dots[dots.Count - 2].GetComponent<Image>();
            //    dots[dots.Count - 2].GetComponent<Image>().color = new Color
            //    {
            //        r = img.color.r,
            //        g = img.color.g,
            //        b = img.color.b,
            //        a = 1f - value,
            //    };
            //});
        }
    }
}
