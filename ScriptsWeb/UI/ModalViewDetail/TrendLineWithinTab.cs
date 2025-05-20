
using Assets.ScriptsWeb.UI;
using DG.Tweening;
using Onthesys.ExeBuild;
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
    /// 상세 탭에서 사용할 트렌드 라인 그래프 View 객체입니다. History, Denoise, inference 등의 탭에서 공용으로 사용합니다.
    /// </summary>
    internal class TrendLineWithinTab : MaskableGraphic
    {

        enum TabType { History, Denoise, Inference}
        TabType type;

        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        //Data
        SensorModel sensorData;
        List<MeasureModel> sensorLogs = new();
        (DateTime from, DateTime to) datetime;
        
        //Func
        List<Vector2> ControlPoints => dots.Select(dot =>
            new Vector2(dot.localPosition.x, dot.localPosition.y)
          + new Vector2(dot.parent.localPosition.x, dot.parent.localPosition.y)
                ).ToList();
        float MaxValue => Mathf.Max(sensorLogs.Select(log => log.measured_value).Max(), sensorData.threshold_critical);
        
        //Components
        TMP_Text lblName;
        List<RectTransform> dots = new();
        List<TMP_Text> lblHourList;
        List<TMP_Text> lblAmountList;
        List<Image> imgAmountList;

        //Constants
        const float thickness = 2f;
        static Dictionary<StatusType, Color> statusColorDic = new();


        #region [Initiating]
        static TrendLineWithinTab()
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
            lblName = transform.Find("Title_Image").GetComponentInChildren<TMP_Text>();

            lblAmountList = transform.Find("Chart_Grid").Find("Text_Vertical").GetComponentsInChildren<TMP_Text>().ToList();
            lblHourList = transform.Find("Chart_Grid").Find("Text_Horizon").GetComponentsInChildren<TMP_Text>().ToList();
            imgAmountList = transform.Find("Chart_Grid").Find("Lines_Vertical").GetComponentsInChildren<Image>().ToList();

            dots = transform.Find("Chart_Dots").GetComponentsInChildren<RectTransform>().ToList();
            dots.Remove(transform.Find("Chart_Dots").GetComponent<RectTransform>());
        }
        protected override void Start()
        {
            if (!Application.isPlaying) return;

            //부모가 가진 탭 View 타입을 통해 표현할 자료에 따라 탭 유형을 결정
            if (GetComponentInParent<PanelHistory>() is not null)
                type = TabType.History;
            else if (GetComponentInParent<PanelDenoise>() is not null)
                type = TabType.Denoise;
            else if (GetComponentInParent<PanelInference>() is not null)
                type = TabType.Inference;
            else Destroy(this.gameObject);

            //탭 유형을 R&R 이벤트 쌍으로 변환
            (UiEventType response, UiEventType request) eventPair = type switch
            {
                TabType.History => (UiEventType.ChangeTrendLineHistory, UiEventType.RequestSearchHistory),
                TabType.Denoise => (UiEventType.ChangeTrendLineDenoised, UiEventType.RequestSearchDenoised),
                TabType.Inference => (UiEventType.ChangeTrendLineInference, UiEventType.RequestSearchInference),
                _ => throw new NotImplementedException(),
            };

            //얻은 이벤트를 통해 이벤트 리스너 등록
            UiManager.Instance.Register(eventPair.response, OnChangeTrendLine);
            UiManager.Instance.Register(eventPair.request, OnRequestSearch);

            base.Start();
            transform.parent.gameObject.SetActive(false);
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
            if (obj is not (int boardId, int sensorId, DateTime fromDt, DateTime toDt)) return;

            sensorData = modelProvider.GetSensor(boardId, sensorId);
            datetime = (fromDt, toDt);

            UpdateUi();
        }

        private void OnChangeTrendLine(object obj)
        {
            sensorLogs = type switch
            {
                TabType.History => modelProvider.GetMeasureHistoryList(),
                TabType.Denoise => modelProvider.GetMeasureDenoisedList(),
                TabType.Inference => modelProvider.GetMeasureInferenceList(),
                _ => throw new NotImplementedException(),
            };

            while (sensorLogs.Count > dots.Count)
                sensorLogs.Remove(sensorLogs.First());

            UpdateUi();
        }

        #endregion

        void UpdateUi() 
        {
            if (sensorData is null) return;

            //제목 설정
            if(lblName != null) lblName.text = sensorData.sensor_name;

            //수직 축(값) 설정
            UpdateAmountLabels();

            //수평 축(시간) 설정
            UpdateTimeLabels();

            //실제 계측값들을 그래프에 적용
            if (sensorLogs.Count != (dots.Count)) return;

            UpdateTrendLine();
        }
    
        void UpdateAmountLabels()
        {
            if (sensorLogs.Count < 1) return;
            if (imgAmountList.Count != lblAmountList.Count && lblAmountList.Count != 5) throw new Exception("TrendLineMeasure - UpdateAmountLabels - chartGrid 노트의 구성 요소가 부적절하여 초기화를 진행할 수 없습니다.");

            float maxValue = Mathf.Max(sensorLogs.Select(log => log.measured_value).Max(), sensorData.threshold_critical);

            //실제 최대값
            if (MaxValue != sensorData.threshold_critical)
            {
                lblAmountList.First().text = MaxValue.ToString("F0"); ;
                lblAmountList.First().color = statusColorDic[StatusType.ERROR];
                imgAmountList.First().color = statusColorDic[StatusType.ERROR];
            }
            else
            {
                lblAmountList[0].text = "";
                lblAmountList.First().color = new Color(0,0,0,0);
                imgAmountList.First().color = new Color(0, 0, 0, 0);
            }
            
            //영점
            lblAmountList[lblAmountList.Count - 1].text = "0";


            //범위
            RectTransform
                lblFrom = lblAmountList.Last().rectTransform,
                lblTo = lblAmountList.First().rectTransform,
                imgFrom = imgAmountList.Last().rectTransform,
                imgTo = imgAmountList.First().rectTransform;

            ((Vector2 min, Vector2 max, Vector2 pos) from, (Vector2 min, Vector2 max, Vector2 pos) to)
                labelAnchorPair = (
                    (lblFrom.anchorMin, lblFrom.anchorMax, lblFrom.anchoredPosition),
                    (lblTo.anchorMin, lblTo.anchorMax, lblTo.anchoredPosition)),
                imageAnchorPair = (
                    (imgFrom.anchorMin, imgFrom.anchorMax, imgFrom.anchoredPosition),
                    (imgTo.anchorMin, imgTo.anchorMax, imgTo.anchoredPosition));

            //임계값들 설정
            List <(StatusType status, float threshold, int tIdx)> dataGrid = new()
            {
                //(StatusType.NORMAL, 0f, 4),
                (StatusType.SERIOUS, sensorData.threshold_serious, 3),
                (StatusType.WARNING, sensorData.threshold_warning, 2),
                (StatusType.CRITICAL, sensorData.threshold_critical, 1),
            };
            foreach (var data in dataGrid) 
            {
                TMP_Text lblAmount = lblAmountList[data.tIdx];
                Image imgAmount = imgAmountList[data.tIdx];
                float ratio = data.threshold / maxValue;

                lblAmount.rectTransform.anchorMin = Vector2.Lerp(labelAnchorPair.from.min, labelAnchorPair.to.min, ratio);
                lblAmount.rectTransform.anchorMax = Vector2.Lerp(labelAnchorPair.from.max, labelAnchorPair.to.max, ratio);
                lblAmount.rectTransform.anchoredPosition = Vector2.Lerp(labelAnchorPair.from.pos, labelAnchorPair.to.pos, ratio);
                lblAmount.color = statusColorDic[data.status];
                lblAmount.text = data.threshold.ToString("F0");

                imgAmount.rectTransform.anchorMin = Vector2.Lerp(imageAnchorPair.from.min, imageAnchorPair.to.min, ratio);
                imgAmount.rectTransform.anchorMax = Vector2.Lerp(imageAnchorPair.from.max, imageAnchorPair.to.max, ratio);
                imgAmount.rectTransform.anchoredPosition = Vector2.Lerp(imageAnchorPair.from.pos, imageAnchorPair.to.pos, ratio);
                imgAmount.color = statusColorDic[data.status];
            }

        }

        void UpdateTimeLabels() 
        {
            lblHourList.ForEach(item =>
            {
                float ratio = (float)lblHourList.IndexOf(item) / (lblHourList.Count - 1);
                DateTime dt = datetime.from + (datetime.to - datetime.from) * ratio; 
                item.text = dt.ToString("yy-MM-dd\nHH:mm");
            });
        }

        void UpdateTrendLine() 
        {
            RectTransform parentRect = transform.Find("Chart_Dots").GetComponent<RectTransform>();
            Vector2 parentSize = parentRect.rect.size;

            for (int i = dots.Count - 1; i > 0; i--)
            {
                RectTransform childRect = dots[i];
                float measuredValue = sensorLogs[i-1].measured_value;
                float measuredRatio = measuredValue / MaxValue;

                Vector2 anchorPos = childRect.anchorMin;

                float bottomY = anchorPos.y;
                float topY = anchorPos.y + parentSize.y;
                float targetY = Mathf.Lerp(bottomY, topY, measuredRatio);

                Vector2 newPos = new(anchorPos.x, targetY);

                childRect.DOAnchorPos(newPos, 0.4f);

                StatusType status = modelProvider.GetStatusBySensorAndValue(sensorData.board_id, sensorData.sensor_id, measuredValue);
                childRect.GetComponent<Image>().color = statusColorDic[status];

            }
        }
    }
}
