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
        public GameObject tooltipPrefab;
        public float pointRadius = 50f;

        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        public int boardId, sensorId;
        (DateTime from, DateTime to) datetime;
        SensorModel sensorData;
        List<MeasureModel> sensorLogs = new();

        private GameObject currentTooltip;
        private Camera uiCamera;
        private RectTransform chartRect;

        List<Vector2> ControlPoints =>
            dots.Where(dot => dot != null && dot.gameObject != null)
                .Select(dot => new Vector2(dot.localPosition.x, dot.localPosition.y) + new Vector2(dot.parent.localPosition.x, dot.parent.localPosition.y))
                .ToList();

        float GetFixedMaxValue()
        {
            if (sensorData == null) return 300f;

            return sensorData.sensor_id switch
            {
                1 => 300f,
                2 => 300f,
                3 => 4000f,
                _ => 300f
            };
        }

        float MaxValue => GetFixedMaxValue();

        TMP_Text lblIsFixing;
        List<RectTransform> dots = new();
        List<TMP_Text> lblHourList;
        List<TMP_Text> lblAmountList;
        List<Image> imgAmountList;

        const float thickness = 2f;
        static Dictionary<StatusType, Color> statusColorDic = new();

        private GameObject dailyDots, dailyGrid;
        private GameObject weeklyDots, weeklyGrid;

        #region [Initiating]
        static TrendLineMeasure()
        {
            Dictionary<StatusType, string> rawColorSets = new()
            {
                { StatusType.NORMAL,   "#00FBFF" },
                { StatusType.SERIOUS,  "#88FF22" },
                { StatusType.WARNING,  "#FFFF44" },
                { StatusType.CRITICAL, "#FF4444" },
                { StatusType.ERROR,    "#FF0000" },
            };

            foreach (var pair in rawColorSets)
                if (ColorUtility.TryParseHtmlString(pair.Value, out Color color))
                    statusColorDic[pair.Key] = color;
        }

        protected override void Awake()
        {
            raycastTarget = true;

            dailyDots = transform.Find("Chart_Dots").gameObject;
            dailyGrid = transform.Find("Chart_Grid").gameObject;
            weeklyDots = transform.Find("Chart_Dots_Week").gameObject;
            weeklyGrid = transform.Find("Chart_Grid_Week").gameObject;

            lblIsFixing?.gameObject.SetActive(false);

            chartRect = GetComponent<RectTransform>();
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            uiCamera = parentCanvas?.worldCamera ?? Camera.main;
        }

        protected override void Start()
        {
            if (!Application.isPlaying) return;

            base.Start();
            UiManager.Instance?.Register(UiEventType.Initiate, OnInitiate);
            UiManager.Instance?.Register(UiEventType.ChangeTrendLine, OnChangeTrendLine);
        }
        #endregion

        #region [Tooltip Events]
        public void OnPointerMove(PointerEventData eventData)
        {
            if (sensorLogs.Count == 0 || sensorData == null) return;

            Vector2 screenMousePos = eventData.position;
            var (closestIndex, isNearPoint) = FindClosestPointIndex(screenMousePos);

            if (closestIndex >= 0 && isNearPoint)
                ShowSingleTooltip(sensorLogs[closestIndex], eventData.position);
            else
                HideTooltip();
        }

        public void OnPointerExit(PointerEventData eventData) => HideTooltip();

        private (int index, bool isNearPoint) FindClosestPointIndex(Vector2 screenMousePos)
        {
            float minDistance = float.MaxValue;
            int closestIndex = -1;
            bool isNearPoint = false;

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

            return (closestIndex, isNearPoint);
        }

        private void ShowSingleTooltip(MeasureModel measureData, Vector2 screenPosition)
        {
            if (tooltipPrefab == null) return;
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null) return;

            if (currentTooltip == null)
                currentTooltip = Instantiate(tooltipPrefab, parentCanvas.transform);

            var tooltipDisplay = currentTooltip.GetComponent<SingleTooltipDisplay>();
            tooltipDisplay?.Show(measureData, sensorData, screenPosition, uiCamera);
        }

        private void HideTooltip()
        {
            currentTooltip?.GetComponent<SingleTooltipDisplay>()?.Hide();
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
            vh.AddVert(start + normal, colorStart, new Vector2(0, 0));
            vh.AddVert(start - normal, colorStart, new Vector2(0, 1));
            vh.AddVert(end - normal, colorEnd, new Vector2(1, 1));
            vh.AddVert(end + normal, colorEnd, new Vector2(1, 0));

            int baseIndex = vh.currentVertCount;
            vh.AddTriangle(baseIndex - 4, baseIndex - 3, baseIndex - 2);
            vh.AddTriangle(baseIndex - 2, baseIndex - 1, baseIndex - 4);
        }
        #endregion

        #region [EventListeners]
        private void OnChangeTrendLine(object obj)
        {
            sensorLogs = modelProvider.GetMeasureLogBySensor(boardId, sensorId);
            if (sensorLogs == null || sensorLogs.Count == 0) return;

            datetime.from = (sensorLogs.Last().MeasuredTime - sensorLogs.First().MeasuredTime > new TimeSpan(50, 0, 0)) ? DateTimeKst.Now.AddDays(-7) : DateTimeKst.Now.AddDays(-1);
            datetime.to = DateTimeKst.Now;

            bool isWeek = (datetime.to - datetime.from).TotalDays >= 4;

            dailyDots.SetActive(!isWeek);
            dailyGrid.SetActive(!isWeek);
            weeklyDots.SetActive(isWeek);
            weeklyGrid.SetActive(isWeek);

            var dotsRoot = transform.Find(isWeek ? "Chart_Dots_Week" : "Chart_Dots");
            dots = dotsRoot.GetComponentsInChildren<RectTransform>().ToList();
            dots.Remove(dotsRoot.GetComponent<RectTransform>());

            var grid = transform.Find(isWeek ? "Chart_Grid_Week" : "Chart_Grid");
            lblAmountList = grid.Find("Text_Vertical")?.GetComponentsInChildren<TMP_Text>().ToList();
            lblHourList = grid.Find("Text_Horizon")?.GetComponentsInChildren<TMP_Text>().ToList();
            imgAmountList = grid.Find("Lines_Vertical")?.GetComponentsInChildren<Image>().ToList();

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

            lblIsFixing?.gameObject.SetActive(sensorData.isUsing);

            UpdateAmountLabels();
            UpdateTimeLabels();

            if (sensorLogs.Count != dots.Count)
            {
                Debug.LogWarning($"센서 데이터 개수({sensorLogs.Count})와 점 개수({dots.Count})가 일치하지 않습니다.");
                return;
            }
            UpdateTrendLine();
        }

        void UpdateAmountLabels()
        {
            if (sensorLogs.Count < 1 || lblAmountList == null) return;

            float maxVal = GetFixedMaxValue();

            foreach (var lbl in lblAmountList)
            {
                float val = maxVal / (lblAmountList.Count - 1) * (lblAmountList.Count - 1 - lblAmountList.IndexOf(lbl));
                DOTween.To(() => val, value => lbl.text = value.ToString("F0"), val, 0.4f);
            }
        }

        void UpdateTimeLabels()
        {
            if (lblHourList == null) return;

            bool isWeek = weeklyGrid.activeSelf;

            if (isWeek && sensorLogs.Count > 0)
            {
                // 주간모드: 실제 데이터의 시간을 기준으로 라벨 생성
                lblHourList.ForEach(item =>
                {
                    int labelIndex = lblHourList.IndexOf(item);

                    // 실제 데이터 개수에 맞춰 인덱스 계산
                    int dataIndex = Mathf.RoundToInt((float)labelIndex / (lblHourList.Count - 1) * (sensorLogs.Count - 1));
                    dataIndex = Mathf.Clamp(dataIndex, 0, sensorLogs.Count - 1);

                    if (dataIndex < sensorLogs.Count)
                    {
                        DateTime actualTime = sensorLogs[dataIndex].MeasuredTime;
                        item.text = actualTime.ToString("MM.dd\nHH:mm");
                    }
                    else
                    {
                        // Fallback: 비율로 계산
                        float ratio = (float)labelIndex / (lblHourList.Count - 1);
                        DateTime dt = datetime.from + (datetime.to - datetime.from) * ratio;
                        item.text = dt.ToString("MM.dd\nHH:mm");
                    }
                });
            }
            {
                // 일간모드도 실제 데이터 시간 기준으로 변경
                if (sensorLogs.Count > 0)
                {
                    lblHourList.ForEach(item =>
                    {
                        int labelIndex = lblHourList.IndexOf(item);
                        int dataIndex = Mathf.RoundToInt((float)labelIndex / (lblHourList.Count - 1) * (sensorLogs.Count - 1));
                        dataIndex = Mathf.Clamp(dataIndex, 0, sensorLogs.Count - 1);

                        DateTime actualTime = sensorLogs[dataIndex].MeasuredTime; // 실제 데이터 시간 사용
                        TimeSpan timeSpan = datetime.to - datetime.from;

                        if (timeSpan.TotalDays < 4f)
                        {
                            if (item == lblHourList.Last() || item == lblHourList.First())
                            {
                                item.text = $"\n{actualTime:dd}일{actualTime:HH:mm}";
                            }
                            else
                            {
                                item.text = $"\n{actualTime:HH:mm}";
                            }
                        }
                        else
                        {
                            item.text = actualTime.ToString("MM.dd");
                        }
                    });
                }
            }
        }



        void UpdateTrendLine()
        {
            RectTransform parentRect = dots.First().parent.GetComponent<RectTransform>();
            Vector2 parentSize = parentRect.rect.size;
            Vector2 anchorPos = dots.First().anchorMin;

            float bottomY = anchorPos.y;
            float topY = anchorPos.y + parentSize.y;

            for (int i = dots.Count - 1; i >= 0; i--)
            {
                RectTransform childRect = dots[i];
                float measuredValue = sensorLogs[i].measured_value;
                float measuredRatio = measuredValue / GetFixedMaxValue();
                float targetY = Mathf.Lerp(bottomY, topY, measuredRatio);
                Vector2 newPos = new(anchorPos.x, targetY);
                childRect.DOAnchorPos(newPos, 0.4f);
            }
        }
    }
}