using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Onthesys.WebBuild;

public class RealtimeStatusAlarmBorderVisualizer : MonoBehaviour
{
    public enum AlarmLevel
    {
        Normal,
        Warning,
        Alert
    }

    [Header("센서 설정")]
    public int sensorId;

    [Header("테두리 설정")]
    public float borderWidth = 8f;
    public bool useInnerBorder = false;

    [Header("색상 설정")]
    public Color warningColor = new Color(1f, 1f, 0f, 1f); 
    public Color alertColor = new Color(1f, 0f, 0f, 0.9f);

    [Header("애니메이션 설정")]
    public bool useAnimation = false;
    public AnimationType animationType = AnimationType.Pulse;
    public float animationSpeed = 2f;
    public float minAlpha = 0.4f;
    public float maxAlpha = 1f;

    public enum AnimationType
    {
        Pulse,
        Rotate,
        Scale
    }

    // 성능 최적화를 위한 정적 변수
    private static Sprite cachedBorderSprite;
    private static int spriteReferenceCount = 0;

    private GameObject borderObject;
    private Image borderImage;
    private RectTransform borderRectTransform;
    private AlarmLevel currentAlarmLevel = AlarmLevel.Normal;
    private SensorModel sensorData;

    // 애니메이션 관련
    private Color baseColor;
    private float animationTime;
    private Vector3 originalScale;
    private bool isAnimationActive = false;

    void Start()
    {
        CreateOptimizedBorder();
        SetAlarmLevel(AlarmLevel.Normal);
        StartCoroutine(InitializeAfterDataLoad());
    }

    System.Collections.IEnumerator InitializeAfterDataLoad()
    {
        while (UiManager.Instance == null ||
               UiManager.Instance.modelProvider == null ||
               UiManager.Instance.modelProvider.GetSensors().Count == 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        var sensors = UiManager.Instance.modelProvider.GetSensors();
        sensorData = sensors.FirstOrDefault(s => s.sensor_id == sensorId);

        if (sensorData == null)
        {
            Debug.LogError($"센서 {sensorId} 데이터를 찾을 수 없음!");
            yield break;
        }

        Debug.Log($"센서 {sensorId} 초기화 성공: {sensorData.sensor_name}");

        RegisterEvents();
        CheckCurrentSensorStatus();
    }

    void RegisterEvents()
    {
        if (UiManager.Instance != null)
        {
            // 실시간 측정값이 업데이트될 때마다 상태 확인
            UiManager.Instance.Register(UiEventType.ChangeRecentValue, OnRecentValueChanged);
        }
    }

    void UnregisterEvents()
    {
        if (UiManager.Instance != null)
        {
            UiManager.Instance.Unregister(UiEventType.ChangeRecentValue, OnRecentValueChanged);
        }
    }

    void OnDestroy()
    {
        UnregisterEvents();
        DecrementSpriteReference();
    }

    void OnDisable()
    {
        UnregisterEvents();
    }

    void OnEnable()
    {
        if (sensorData != null)
        {
            RegisterEvents();
        }
    }

    // 실시간 측정값 변경 이벤트 핸들러
    private void OnRecentValueChanged(object obj)
    {
        CheckCurrentSensorStatus();
    }

    // 현재 센서 상태 확인 (실시간 측정값 기반)
    private void CheckCurrentSensorStatus()
    {
        if (UiManager.Instance?.modelProvider == null || sensorData == null)
            return;

        try
        {
            // 상하류 측정값 가져오기
            MeasureModel upperData = UiManager.Instance.modelProvider.GetMeasureRecentBySensor(1, sensorId);
            MeasureModel lowerData = UiManager.Instance.modelProvider.GetMeasureRecentBySensor(2, sensorId);

            if (upperData == null || lowerData == null)
            {
                Debug.LogWarning($"센서 {sensorId} 측정값을 가져올 수 없음 (upper: {upperData != null}, lower: {lowerData != null})");
                return;
            }

            // 차이값 계산 (하류 - 상류)
            float diffValue = lowerData.measured_value - upperData.measured_value;

            // 임계값과 비교하여 상태 판정
            StatusType currentStatus = StatusType.NORMAL;
            if (diffValue > sensorData.threshold_warning)
                currentStatus = StatusType.WARNING;
            else if (diffValue > sensorData.threshold_serious)
                currentStatus = StatusType.SERIOUS;

            AlarmLevel newLevel = ConvertStatusToAlarmLevel(currentStatus);

            Debug.Log($"센서 {sensorId}({sensorData.sensor_name}) - 상류:{upperData.measured_value:F1}, 하류:{lowerData.measured_value:F1}, 차이:{diffValue:F1}, 상태:{currentStatus}");

            if (currentAlarmLevel != newLevel)
            {
                AlarmLevel oldLevel = currentAlarmLevel;
                SetAlarmLevel(newLevel);
                Debug.Log($"[실시간] 센서 {sensorId} 상태 변경: {oldLevel} → {newLevel} (StatusType: {currentStatus})");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"실시간 센서 상태 확인 중 에러: {e.Message}");
        }
    }

    // StatusType을 AlarmLevel로 변환
    private AlarmLevel ConvertStatusToAlarmLevel(StatusType statusType)
    {
        return statusType switch
        {
            StatusType.WARNING => AlarmLevel.Alert,      // Warning = 경보 (빨간색)
            StatusType.SERIOUS => AlarmLevel.Warning,    // Serious = 경계 (노란색)
            StatusType.NORMAL => AlarmLevel.Normal,      // 정상
            _ => AlarmLevel.Normal
        };
    }

    void CreateOptimizedBorder()
    {
        if (cachedBorderSprite == null)
        {
            cachedBorderSprite = CreateBorderSprite();
        }
        spriteReferenceCount++;

        borderObject = new GameObject("AlarmBorder");
        borderObject.transform.SetParent(transform, false);

        borderRectTransform = borderObject.AddComponent<RectTransform>();
        borderRectTransform.anchorMin = Vector2.zero;
        borderRectTransform.anchorMax = Vector2.one;

        if (!useInnerBorder)
        {
            borderRectTransform.sizeDelta = new Vector2(borderWidth * 2, borderWidth * 2);
        }
        else
        {
            borderRectTransform.sizeDelta = Vector2.zero;
        }
        borderRectTransform.anchoredPosition = Vector2.zero;

        borderImage = borderObject.AddComponent<Image>();
        borderImage.sprite = cachedBorderSprite;
        borderImage.type = Image.Type.Sliced;
        borderImage.pixelsPerUnitMultiplier = 1f;

        // Z-Order: 알람 레벨에 따라 우선순위 조정
        borderObject.transform.SetAsLastSibling(); // 가장 앞에 표시
        borderObject.SetActive(false);

        originalScale = borderRectTransform.localScale;
    }

    private void DecrementSpriteReference()
    {
        spriteReferenceCount--;
        if (spriteReferenceCount <= 0 && cachedBorderSprite != null)
        {
            if (cachedBorderSprite.texture != null)
            {
                DestroyImmediate(cachedBorderSprite.texture);
            }
            DestroyImmediate(cachedBorderSprite);
            cachedBorderSprite = null;
            spriteReferenceCount = 0;
        }
    }

    Sprite CreateBorderSprite()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[size * size];
        int borderPixels = Mathf.RoundToInt(borderWidth);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool isHorizontalBorder = (x < borderPixels || x >= size - borderPixels);
                bool isVerticalBorder = (y < borderPixels || y >= size - borderPixels);

                if (isHorizontalBorder || isVerticalBorder)
                {
                    float distanceFromEdgeX = Mathf.Min(x, size - x - 1);
                    float distanceFromEdgeY = Mathf.Min(y, size - y - 1);
                    float minDistance = Mathf.Min(distanceFromEdgeX, distanceFromEdgeY);
                    float alpha = Mathf.Clamp01(minDistance / 2f);
                    pixels[x + y * size] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    pixels[x + y * size] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.Tight,
            new Vector4(borderPixels, borderPixels, borderPixels, borderPixels)
        );
    }

    public void SetAlarmLevel(AlarmLevel level)
    {
        if (currentAlarmLevel == level) return;

        currentAlarmLevel = level;
        isAnimationActive = false;

        // Transform 리셋
        if (borderRectTransform != null)
        {
            borderRectTransform.localRotation = Quaternion.identity;
            borderRectTransform.localScale = originalScale;
        }

        switch (level)
        {
            case AlarmLevel.Normal:
                borderObject.SetActive(false);
                break;

            case AlarmLevel.Warning:
                borderObject.SetActive(true);
                baseColor = warningColor;
                borderImage.color = warningColor;
                // 경계는 낮은 우선순위
                borderObject.transform.SetAsLastSibling();
                if (useAnimation)
                {
                    isAnimationActive = true;
                    animationTime = 0f;
                }
                break;

            case AlarmLevel.Alert:
                borderObject.SetActive(true);
                baseColor = alertColor;
                borderImage.color = alertColor;
                // 경보는 최고 우선순위 - 가장 앞에 표시
                borderObject.transform.SetAsLastSibling();
                if (useAnimation)
                {
                    isAnimationActive = true;
                    animationTime = 0f;
                }
                break;
        }
    }

    void Update()
    {
        if (!isAnimationActive || currentAlarmLevel == AlarmLevel.Normal)
            return;

        animationTime += Time.deltaTime;

        switch (animationType)
        {
            case AnimationType.Pulse:
                UpdatePulseAnimation();
                break;
            case AnimationType.Rotate:
                UpdateRotateAnimation();
                break;
            case AnimationType.Scale:
                UpdateScaleAnimation();
                break;
        }
    }

    private void UpdatePulseAnimation()
    {
        float t = (Mathf.Sin(animationTime * animationSpeed) + 1f) / 2f;
        Color newColor = baseColor;
        newColor.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        borderImage.color = newColor;
    }

    private void UpdateRotateAnimation()
    {
        borderRectTransform.Rotate(0, 0, animationSpeed * 30 * Time.deltaTime);
    }

    private void UpdateScaleAnimation()
    {
        float t = (Mathf.Sin(animationTime * animationSpeed) + 1f) / 2f;
        float scale = Mathf.Lerp(0.95f, 1.05f, t);
        borderRectTransform.localScale = originalScale * scale;
    }
}