
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Onthesys.WebBuild
{
    internal class TrendLineMeasure : MaskableGraphic, IPointerMoveHandler, IPointerExitHandler
    {
        [Header("Tooltip Settings")]
        public GameObject tooltipPrefab; // TrendLineTooltipPrefab 할당
        public float pointRadius = 50f;   // 포인트 감지 반경

        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        //Data
        public int boardId, sensorId;
        (DateTime from, DateTime to) datetime;
        SensorModel sensorData;
        List<MeasureModel> sensorLogs = new();

        //Tooltip
        private GameObject currentTooltip;
        private Camera uiCamera;
        private RectTransform chartRect;

        //Func
        List<Vector2> ControlPoints => dots.Select(dot =>
            new Vector2(dot.localPosition.x, dot.localPosition.y)
          + new Vector2(dot.parent.localPosition.x, dot.parent.localPosition.y)
                ).ToList();
        //float MaxValue => Mathf.Max(sensorLogs.Select(log => log.measured_value).Max(), 0.1f);

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
            raycastTarget = true;

            //lblName = transform.Find("Title_Image").GetComponentInChildren<TMP_Text>();

            lblAmountList = transform.Find("Chart_Grid").Find("Text_Vertical").GetComponentsInChildren<TMP_Text>().ToList();
            lblHourList = transform.Find("Chart_Grid").Find("Text_Horizon").GetComponentsInChildren<TMP_Text>().ToList();
            imgAmountList = transform.Find("Chart_Grid").Find("Lines_Vertical").GetComponentsInChildren<Image>().ToList();

            dots = transform.Find("Chart_Dots").GetComponentsInChildren<RectTransform>().ToList();
            dots.Remove(transform.Find("Chart_Dots").GetComponent<RectTransform>());
            sensorLogs = dots.Select(item => new MeasureModel()).ToList();

            lblIsFixing?.gameObject.SetActive(false);

            // 툴팁을 위한 초기화 - TrendLineUpAndLow2와 동일
            chartRect = GetComponent<RectTransform>();
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                Debug.Log($"[Canvas] sortingOrder: {parentCanvas.sortingOrder}");
            }
            uiCamera = parentCanvas?.worldCamera ?? Camera.main;
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

        #region [툴팁 이벤트 처리]

        public void OnPointerMove(PointerEventData eventData)
        {
            Debug.Log($"Main-[OnPointerMove] 호출됨! 마우스: {eventData.position}");

            if (sensorLogs.Count == 0 || sensorData == null)
                return;

            Vector2 screenMousePos = eventData.position;

            // 실제 점(노드) 근처에서만 툴팁 표시
            var (closestIndex, isNearPoint) = FindClosestPointIndex(screenMousePos);

            if (closestIndex >= 0 && isNearPoint)
            {
                Debug.Log($"Main-[Step 1] 조건문 통과 - 인덱스: {closestIndex}");
                Debug.Log($"Main-[Step 2] 배열 크기 확인 - 센서로그: {sensorLogs?.Count}");

                var measureData = sensorLogs[closestIndex];
                Debug.Log($"Main-[Step 3] measureData 추출 완료: {measureData?.measured_value}");

                ShowSingleTooltip(measureData, eventData.position);
                Debug.Log($"Main-[Step 4] ShowSingleTooltip 호출 완료");
            }
            else
            {
                Debug.Log($"Main-[OnPointerMove] 툴팁 숨김");
                HideTooltip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private (int index, bool isNearPoint) FindClosestPointIndex(Vector2 screenMousePos)
        {
            float minDistance = float.MaxValue;
            int closestIndex = -1;
            bool isNearPoint = false;
            float pointRadius = 50f; // TrendLineUpAndLow2와 동일한 감지 반경

            Debug.Log($"[Debug]Main- 마우스 Screen 위치: ({screenMousePos.x:F1}, {screenMousePos.y:F1})");

            // 점들 검사 - TrendLineUpAndLow2와 동일한 방식
            for (int i = 0; i < dots.Count; i++)
            {
                if (i >= sensorLogs.Count) continue;

                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, dots[i].position);
                float distance = Vector2.Distance(screenMousePos, screenPos);

                if (distance <= pointRadius && distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                    isNearPoint = true;
                }
            }

            if (isNearPoint)
            {
                Debug.Log($"[Debug]Main- 최종 선택: 인덱스[{closestIndex}], 거리: {minDistance:F1}");
            }

            return (closestIndex, isNearPoint);
        }

        private void ShowSingleTooltip(MeasureModel measureData, Vector2 screenPosition)
        {
            Debug.Log($"[Main-ShowSingleTooltip] 메서드 진입 확인!");
            Debug.Log($"[Main-ShowSingleTooltip] 요청 위치: {screenPosition}");

            if (tooltipPrefab == null)
                return;

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
                return;

            // 툴팁이 없으면 새로 생성
            if (currentTooltip == null)
            {
                currentTooltip = Instantiate(tooltipPrefab, parentCanvas.transform);
                Debug.Log($"[Main-ShowSingleTooltip] 툴팁 생성 완료: {currentTooltip.name}");
            }

            // 툴팁 내용 업데이트 - DualTooltipDisplay 대신 SingleTooltipDisplay 사용
            var tooltipDisplay = currentTooltip.GetComponent<SingleTooltipDisplay>();
            if (tooltipDisplay != null)
            {
                tooltipDisplay.Show(measureData, sensorData, screenPosition, uiCamera);
            }

            // 최종 위치 확인
            Debug.Log($"[Main-ShowSingleTooltip] 최종 툴팁 위치: {currentTooltip.transform.position}");
        }

        private void HideTooltip()
        {
            if (currentTooltip != null)
            {
                var tooltipDisplay = currentTooltip.GetComponent<SingleTooltipDisplay>();
                tooltipDisplay?.Hide();
            }
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

            datetime.from = (sensorLogs.Last().MeasuredTime- sensorLogs.First().MeasuredTime > new TimeSpan(50,0,0))? DateTimeKst.Now.AddDays(-7) : DateTimeKst.Now.AddDays(-1);
            datetime.to= DateTimeKst.Now;

            //Debug.Log($"[TrendLineMeasure] OnChangeTrendLine - boardId: {boardId}, sensorId: {sensorId}, datetime.from: {sensorLogs.First().measured_time}, datetime.to: {sensorLogs.Last().measured_time}");

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

            // 고정된 최대값 사용 (자동 계산 제거)
            float maxVal = GetFixedMaxValue();

            foreach (var lbl in lblAmountList)
            {
                float val = maxVal / (lblAmountList.Count - 1) * (lblAmountList.Count - 1 - lblAmountList.IndexOf(lbl));
                DOTween.To(() => val,
                    value => lbl.text = value.ToString("F0"),
                    val, 0.4f);
            }
        }

        void UpdateTimeLabels()
        {
            lblHourList.ForEach(item =>
            {
                float ratio = (float)lblHourList.IndexOf(item) / (lblHourList.Count - 1);
                DateTime dt = datetime.from + (datetime.to - datetime.from) * ratio;
                TimeSpan timeSpan = datetime.to - datetime.from;

                //item.alignment = TMPro.TextAlignmentOptions.Center;

                if (timeSpan.TotalDays < 4f)
                {
                    if (item == lblHourList.Last() || item == lblHourList.First())
                    {
                        //날짜는 위쪽에, 시간은 아래쪽에 배치
                        item.text = $"\n{dt:dd}일{dt:HH:mm}";
                    }
                    else
                    {
                        // 중간은 시간만 표시
                        item.text = $"\n{dt:HH:mm}";
                    }
                }
                else
                {
                    item.text = dt.ToString("MM.dd");
                }
            });
        }

        /*void UpdateTimeLabels()
        {
            lblHourList.ForEach(item =>
            {
                float ratio = (float)lblHourList.IndexOf(item) / (lblHourList.Count - 1);
                DateTime dt = datetime.from + (datetime.to - datetime.from) * ratio;
                TimeSpan timeSpan = datetime.to - datetime.from;

                //Debug.Log($"[TrendLineMeasure] UpdateTimeLabels - ratio: {ratio}, dt: {dt}, timeSpan: {timeSpan}");
                if (timeSpan.TotalDays < 4f)
                {
                    if (item == lblHourList.Last() || item == lblHourList.First())
                        item.text = dt.ToString("dd\nHH:mm");
                    else
                        item.text = dt.ToString("\nHH:mm");
                }
                else
                    item.text = dt.ToString("MM.dd");
            });
        }*/

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
                //float measuredRatio = measuredValue / MaxValue;

                // 고정된 최대값으로 비율 계산
                float measuredRatio = measuredValue / GetFixedMaxValue();

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
