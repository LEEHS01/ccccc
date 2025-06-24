using Assets.ScriptsWeb.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Onthesys.WebBuild
{
    /// <summary>
    /// 기록조회 탭에서 사용하는 두 트렌드 중 하나로 상하류 계측기의 실제 값을 동시에 표시하는데 사용됩니다.
    /// Y축(시간) 기준으로 상하류 데이터를 동시에 보여주는 툴팁이 추가되었습니다.
    /// </summary>
    /// <remarks>
    /// <para>- MaskableGraphic: Unity UI에서 그래픽을 그릴 수 있는 기본 클래스</para>
    /// <para>- IPointerMoveHandler: 마우스 움직임 감지</para>
    /// <para>- IPointerExitHandler: 마우스가 벗어날 때 감지</para>
    /// </remarks>
    internal class TrendLineUpAndLow2 : MaskableGraphic, IPointerMoveHandler, IPointerExitHandler
    {
        [Header("Tooltip Settings")]
        public GameObject tooltipPrefab; // Inspector에서 DualTooltipPrefab 할당
        public float xTolerance = 20f;   // X축 허용 범위 (시간축 기준)

        ModelProvider modelProvider => UiManager.Instance.modelProvider;

        //Data
        SensorModel sensorData;
        (List<MeasureModel> upper, List<MeasureModel> lower) sensorLogs = (new(), new());
        (DateTime from, DateTime to) datetime;
        new (Color upper, Color lower) color = (new Color(0, 1, 1), new Color(1, 1, 0));

        //Tooltip
        private GameObject currentTooltip;
        private Camera uiCamera;
        private RectTransform chartRect;

        //Components
        (List<RectTransform> upper, List<RectTransform> lower) dots = (new(), new());
        List<TMP_Text> lblHourList;
        List<TMP_Text> lblAmountList;

        //Constants
        const float thickness = 2f;
        static Dictionary<StatusType, Color> statusColorDic = new();

        #region [초기화]
        
        protected override void Awake()
        {
            base.Awake();

            //Raycast Target 활성화 - 매우 중요!
            raycastTarget = true;

            lblAmountList = transform.Find("Chart_Grid").Find("Text_Vertical").GetComponentsInChildren<TMP_Text>().ToList();
            lblHourList = transform.Find("Chart_Grid").Find("Text_Horizon").GetComponentsInChildren<TMP_Text>().ToList();

            dots.upper = transform.Find("dotsUpper").GetComponentsInChildren<RectTransform>().ToList();
            dots.upper.Remove(transform.Find("dotsUpper").GetComponent<RectTransform>());
            dots.lower = transform.Find("dotsLower").GetComponentsInChildren<RectTransform>().ToList();
            dots.lower.Remove(transform.Find("dotsLower").GetComponent<RectTransform>());

            // 툴팁을 위한 초기화
            chartRect = GetComponent<RectTransform>();
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            uiCamera = parentCanvas?.worldCamera ?? Camera.main;

            Debug.Log($"[TrendLineTooltip] Awake - raycastTarget: {raycastTarget}, Canvas: {parentCanvas?.name}");
        }

        protected override void Start()
        {
            if (!Application.isPlaying) return;

            // 기존 툴팁 정리
            CleanupTooltip();

            UiManager.Instance.Register(UiEventType.ChangeTrendLineHistory, OnChangeTrendLine);
            UiManager.Instance.Register(UiEventType.RequestSearchHistory, OnRequestSearch);

            base.Start();

            Debug.Log($"[TrendLineTooltip] Started - tooltipPrefab: {tooltipPrefab?.name}");
        }

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
        #endregion

        #region [툴팁 이벤트 처리]
        private int currentTooltipIndex = -1; // 현재 표시 중인 툴팁의 인덱스
        public void OnPointerMove(PointerEventData eventData)
        {
            Debug.Log($"[TrendLineTooltip] OnPointerMove called at {eventData.position}");

            if (sensorLogs.upper.Count == 0 || sensorLogs.lower.Count == 0)
            {
                Debug.Log("[TrendLineTooltip] No sensor data available");
                return;
            }

            // 마우스 위치를 로컬 좌표로 변환
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                chartRect, eventData.position, uiCamera, out Vector2 localMousePos))
            {
                Debug.Log("[TrendLineTooltip] Failed to convert screen point to local point");
                return;
            }

           Debug.Log($"[TrendLineTooltip] Local mouse position: {localMousePos}");

            // 실제 점(노드) 근처에서만 툴팁 표시
            var (closestIndex, isNearPoint) = FindClosestPointIndex(localMousePos);
            Debug.Log($"[TrendLineTooltip] Closest index: {closestIndex}, Near point: {isNearPoint}");

            if (closestIndex >= 0 && isNearPoint)
            {
                // 🎯 같은 인덱스면 Show() 호출 안 함
                if (currentTooltipIndex != closestIndex)
                {
                    currentTooltipIndex = closestIndex;
                    var upperData = sensorLogs.upper[closestIndex];
                    var lowerData = sensorLogs.lower[closestIndex];
                    ShowDualTooltip(upperData, lowerData, eventData.position);
                }
                else
                {
                    //같은 점이면 위치만 업데이트
                    if (currentTooltip != null)
                    {
                        var tooltipDisplay = currentTooltip.GetComponent<DualTooltipDisplay>();
                        tooltipDisplay?.SetPosition(eventData.position, uiCamera);
                    }
                }
            }
            else
            {
                currentTooltipIndex = -1; // 🎯 리셋
                HideTooltip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("[TrendLineTooltip] OnPointerExit called");
            HideTooltip();
        }

        /// <summary>
        /// 점 찾기 알고리즘
        /// </summary>
        /// <returns></returns>
        private (int index, bool isNearPoint) FindClosestPointIndex(Vector2 mousePos)
        {
            float minDistance = float.MaxValue;
            int closestIndex = -1;
            bool isNearPoint = false;

            // 점 감지를 위한 허용 반경 (픽셀 단위)
            float pointRadius = 25f; // 이 값을 조정해서 감도 변경 가능

            // 상류 점들 검사
            for (int i = 0; i < dots.upper.Count; i++)
            {
                Vector2 dotPos = new Vector2(
                    dots.upper[i].localPosition.x + dots.upper[i].parent.localPosition.x,
                    dots.upper[i].localPosition.y + dots.upper[i].parent.localPosition.y
                );

                float distance = Vector2.Distance(mousePos, dotPos);
                Debug.Log($"[TrendLineTooltip] Upper dot {i}: pos={dotPos}, distance={distance}");

                if (distance <= pointRadius && distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                    isNearPoint = true;
                }
            }

            // 하류 점들 검사
            for (int i = 0; i < dots.lower.Count; i++)
            {
                Vector2 dotPos = new Vector2(
                    dots.lower[i].localPosition.x + dots.lower[i].parent.localPosition.x,
                    dots.lower[i].localPosition.y + dots.lower[i].parent.localPosition.y
                );

                float distance = Vector2.Distance(mousePos, dotPos);
                Debug.Log($"[TrendLineTooltip] Lower dot {i}: pos={dotPos}, distance={distance}");

                if (distance <= pointRadius && distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                    isNearPoint = true;
                }
            }

            return (closestIndex, isNearPoint);
        }

        private void ShowDualTooltip(MeasureModel upperData, MeasureModel lowerData, Vector2 screenPosition)
        {
            if (tooltipPrefab == null)
            {
                Debug.LogWarning("[TrendLineTooltip] Tooltip prefab이 할당되지 않았습니다!");
                return;
            }

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogWarning("[TrendLineTooltip] Parent canvas not found!");
                return;
            }

            // 툴팁이 없으면 새로 생성
            if (currentTooltip == null)
            {
                Debug.Log($"[TrendLineTooltip] Creating new tooltip at {screenPosition}");
                currentTooltip = Instantiate(tooltipPrefab, parentCanvas.transform);
            }

            // 툴팁 내용 업데이트 (기존 툴팁이든 새 툴팁이든 항상 업데이트)
            var tooltipDisplay = currentTooltip.GetComponent<DualTooltipDisplay>();
            if (tooltipDisplay != null)
            {
                tooltipDisplay.Show(upperData, lowerData, screenPosition, uiCamera);
                Debug.Log($"[TrendLineTooltip] Tooltip updated - Upper: {upperData.measured_value}, Lower: {lowerData.measured_value}");
            }
            else
            {
                Debug.LogError("[TrendLineTooltip] DualTooltipDisplay component not found on tooltip prefab!");
            }
        }

        private void HideTooltip()
        {
            if (currentTooltip != null)
            {
                var tooltipDisplay = currentTooltip.GetComponent<DualTooltipDisplay>();
                if (tooltipDisplay != null)
                {
                    //Destroy 대신 Hide만 호출
                    tooltipDisplay.Hide(); // onComplete 콜백 제거
                }
                //currentTooltip = null; 제거 (오브젝트 유지)
                //Destroy 제거 (오브젝트 유지)
            }
        }

        private void CleanupTooltip()
        {
            if (currentTooltip != null)
            {
                Destroy(currentTooltip);
                currentTooltip = null;
            }
        }

        private void OnDestroy()
        {
            CleanupTooltip();
        }
        #endregion

        #region [기존 차트 그리기 로직 - 변경 없음]
        protected override void OnRectTransformDimensionsChange() => UpdateUi();

        void Update()
        {
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var upperPoints = dots.upper.Select(dot =>
                new Vector2(dot.localPosition.x, dot.localPosition.y) +
                new Vector2(dot.parent.localPosition.x, dot.parent.localPosition.y)).ToList();
            var lowerPoints = dots.lower.Select(dot =>
                new Vector2(dot.localPosition.x, dot.localPosition.y) +
                new Vector2(dot.parent.localPosition.x, dot.parent.localPosition.y)).ToList();

            DrawLines(vh, upperPoints, color.upper);
            DrawLines(vh, lowerPoints, color.lower);
        }

        private void DrawLines(VertexHelper vh, List<Vector2> points, Color color)
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
            vh.AddVert(start + normal, colorStart, new Vector2(0, 0));
            vh.AddVert(start - normal, colorStart, new Vector2(0, 1));
            vh.AddVert(end - normal, colorEnd, new Vector2(1, 1));
            vh.AddVert(end + normal, colorEnd, new Vector2(1, 0));

            int baseIndex = vh.currentVertCount;
            vh.AddTriangle(baseIndex - 4, baseIndex - 3, baseIndex - 2);
            vh.AddTriangle(baseIndex - 2, baseIndex - 1, baseIndex - 4);
        }

        private void OnRequestSearch(object obj)
        {
            if (obj is not (int sensorId, DateTime fromDt, DateTime toDt)) return;

            sensorData = modelProvider.GetSensor(1, sensorId);
            datetime = (fromDt, toDt);

            UpdateUi();
        }

        private void OnChangeTrendLine(object obj)
        {
            var rawLogs = modelProvider.GetMeasureHistoryList();
            sensorLogs = (
                rawLogs.Where(log => log.board_id == 1).ToList(),
                rawLogs.Where(log => log.board_id == 2).ToList());

            while (sensorLogs.lower.Count > dots.lower.Count)
                sensorLogs.lower.Remove(sensorLogs.lower.First());
            while (sensorLogs.upper.Count > dots.upper.Count)
                sensorLogs.upper.Remove(sensorLogs.upper.First());

            UpdateUi();
        }

        void UpdateUi()
        {
            if (sensorData is null) return;

            UpdateAmountLabels();
            UpdateTimeLabels();

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
                float value = GetCleanLabelValue(GetFixedMaxValue(), index, lblAmountList.Count);
                DOTween.To(() => lbl.rectTransform.anchoredPosition.y,
                    value => lbl.text = value.ToString("F0"),
                    value, 0.4f);
            }
        }

        float GetCleanLabelValue(float maxValue, int index, int totalCount)
        {
            float ratio = (float)(totalCount - 1 - index) / (totalCount - 1);
            float rawValue = maxValue * ratio;

            return sensorData.sensor_id switch
            {
                1 or 2 => Mathf.Round(rawValue / 60f) * 60f,
                3 => Mathf.Round(rawValue / 500f) * 500f,
                _ => Mathf.Round(rawValue / 50f) * 50f
            };
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

        void UpdateTrendLine(List<RectTransform> dots, List<MeasureModel> sensorLogs)
        {
            if (dots.Count < 1) return;

            RectTransform parentRect = dots.First().parent.GetComponent<RectTransform>();
            float parentHeight = parentRect.rect.height;

            for (int i = dots.Count - 1; i >= 0; i--)
            {
                RectTransform childRect = dots[i];
                float measuredValue = sensorLogs[i].measured_value;
                float measuredRatio = measuredValue / GetFixedMaxValue();

                Vector2 anchorPos = childRect.anchorMin;

                float bottomY = anchorPos.y;
                float topY = anchorPos.y + parentHeight;
                float targetY = Mathf.Lerp(bottomY, topY, measuredRatio);
                Vector2 newPos = new(anchorPos.x, targetY);

                childRect.DOAnchorPos(newPos, 0.4f);
            }
        }
        #endregion
    }
}