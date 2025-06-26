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
            if (parentCanvas != null)
            {
                Debug.Log($"[Canvas] sortingOrder: {parentCanvas.sortingOrder}");
                // parentCanvas.sortingOrder = 100;
            }
            uiCamera = parentCanvas?.worldCamera ?? Camera.main;
        }

        protected override void Start()
        {
            if (!Application.isPlaying) return;

            UiManager.Instance.Register(UiEventType.ChangeTrendLineHistory, OnChangeTrendLine);
            UiManager.Instance.Register(UiEventType.RequestSearchHistory, OnRequestSearch);

            base.Start();
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
        public void OnPointerMove(PointerEventData eventData)
        {

            Debug.Log($"[OnPointerMove] 호출됨! 마우스: {eventData.position}");
            if (sensorLogs.upper.Count == 0 || sensorLogs.lower.Count == 0)
                return;

            Vector2 screenMousePos = eventData.position;

            // 실제 점(노드) 근처에서만 툴팁 표시
            var (closestIndex, isNearPoint) = FindClosestPointIndex(screenMousePos);

            if (closestIndex >= 0 && isNearPoint)
            {
                Debug.Log($"[Step 1] 조건문 통과 - 인덱스: {closestIndex}");

                Debug.Log($"[Step 2] 배열 크기 확인 - 상류: {sensorLogs.upper?.Count}, 하류: {sensorLogs.lower?.Count}");

                Debug.Log($"[Step 3] upperData 추출 시작");
                var upperData = sensorLogs.upper[closestIndex];
                Debug.Log($"[Step 4] upperData 추출 완료: {upperData?.measured_value}");

                Debug.Log($"[Step 5] lowerData 추출 시작");
                var lowerData = sensorLogs.lower[closestIndex];
                Debug.Log($"[Step 6] lowerData 추출 완료: {lowerData?.measured_value}");

                Debug.Log($"[Step 7] ShowDualTooltip 호출 시작");
                ShowDualTooltip(upperData, lowerData, eventData.position);
                Debug.Log($"[Step 8] ShowDualTooltip 호출 완료");

            }
            else
            {
                Debug.Log($"[OnPointerMove] 툴팁 숨김");
                HideTooltip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        /// <summary>
        /// 점 찾기 알고리즘
        /// </summary>
        /// <returns></returns>
        private (int index, bool isNearPoint) FindClosestPointIndex(Vector2 screenMousePos)
        {
            float minDistance = float.MaxValue;
            int closestIndex = -1;
            bool isNearPoint = false;
            float pointRadius = 50f;

            Debug.Log($"[Debug] 마우스 Screen 위치: ({screenMousePos.x:F1}, {screenMousePos.y:F1})");

            // 상류 점들 검사
            for (int i = 0; i < dots.upper.Count; i++)
            {
                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, dots.upper[i].position);
                float distance = Vector2.Distance(screenMousePos, screenPos);

                /*if (distance <= pointRadius * 2f)
                {
                    Debug.Log($"[Debug] 상류[{i}]: Screen위치({screenPos.x:F1}, {screenPos.y:F1}), 거리: {distance:F1}");
                }*/

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
                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, dots.lower[i].position);
                float distance = Vector2.Distance(screenMousePos, screenPos);

                /*if (distance <= pointRadius * 2f)
                {
                    Debug.Log($"[Debug] 하류[{i}]: Screen위치({screenPos.x:F1}, {screenPos.y:F1}), 거리: {distance:F1}");
                }*/

                if (distance <= pointRadius && distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                    isNearPoint = true;
                }
            }

            if (isNearPoint)
            {
                Debug.Log($"[Debug] 최종 선택: 인덱스[{closestIndex}], 거리: {minDistance:F1}");
            }


            return (closestIndex, isNearPoint);
        }

        private void ShowDualTooltip(MeasureModel upperData, MeasureModel lowerData, Vector2 screenPosition)
        {
            Debug.Log($"[ShowDualTooltip] 메서드 진입 확인!");
            Debug.Log($"[ShowDualTooltip] 요청 위치: {screenPosition}");
            if (tooltipPrefab == null)
                return;

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
                return;

            // 툴팁이 없으면 새로 생성
            if (currentTooltip == null)
            {
                currentTooltip = Instantiate(tooltipPrefab, parentCanvas.transform);
                Debug.Log($"[ShowDualTooltip] 툴팁 생성 완료: {currentTooltip.name}");
            }

            // 툴팁 내용 업데이트
            var tooltipDisplay = currentTooltip.GetComponent<DualTooltipDisplay>();
            if (tooltipDisplay != null)
            {
                tooltipDisplay.Show(upperData, lowerData, screenPosition, uiCamera);
            }

            // 최종 위치 확인
            Debug.Log($"[ShowDualTooltip] 최종 툴팁 위치: {currentTooltip.transform.position}");
        }

        private void HideTooltip()
        {
            if (currentTooltip != null)
            {
                var tooltipDisplay = currentTooltip.GetComponent<DualTooltipDisplay>();
                tooltipDisplay?.Hide();
            }
        }

        #endregion

        #region [기존 차트 그리기 로직]
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