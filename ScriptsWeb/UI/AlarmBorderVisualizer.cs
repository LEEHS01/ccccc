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
        UpdateAlarmStatusFromServer();
    }

    void RegisterEvents()
    {
        if (UiManager.Instance != null)
        {
            // 서버에서 알람 로그가 업데이트될 때마다 상태 확인
            UiManager.Instance.Register(UiEventType.ChangeAlarmLog, OnServerAlarmStatusChanged);
        }
    }

    void UnregisterEvents()
    {
        if (UiManager.Instance != null)
        {
            UiManager.Instance.Unregister(UiEventType.ChangeAlarmLog, OnServerAlarmStatusChanged);
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

    // 서버 알람 상태 변경 이벤트 핸들러
    private void OnServerAlarmStatusChanged(object obj)
    {
        UpdateAlarmStatusFromServer();
    }

    // 서버에서 이미 판정된 알람 상태 가져오기
    private void UpdateAlarmStatusFromServer()
    {
        if (UiManager.Instance?.modelProvider == null || sensorData == null)
            return;

        try
        {
            // 서버에서 전달받은 알람 로그에서 현재 센서의 알람 상태 확인
            var alarmLogs = UiManager.Instance.modelProvider.GetAlarmLogList();
            var latestAlarm = alarmLogs
                .Where(log => log.sensor_id == sensorId && string.IsNullOrEmpty(log.solved_time))
                .OrderBy(log => log.alarm_level == "Warning" ? 0 : 1)  // Warning(경보) 우선
                .ThenByDescending(log => log.occured_time)
                .FirstOrDefault();

            AlarmLevel newLevel = AlarmLevel.Normal;

            if (latestAlarm != null)
            {
                // 서버에서 전달받은 알람 레벨 사용
                newLevel = latestAlarm.alarm_level switch
                {
                    "Warning" => AlarmLevel.Alert,    // 대소문자 수정
                    "Serious" => AlarmLevel.Warning,  // 대소문자 수정
                    _ => AlarmLevel.Normal
                };
            }

            if (currentAlarmLevel != newLevel)
            {
                AlarmLevel oldLevel = currentAlarmLevel;
                SetAlarmLevel(newLevel);
                Debug.Log($"[서버 알람] 센서 {sensorId} 상태 변경: {oldLevel} → {newLevel}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"서버 알람 상태 확인 중 에러: {e.Message}");
        }
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
        borderImage.raycastTarget = false;
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
                if (borderObject != null) borderObject.SetActive(false);
                break;

            case AlarmLevel.Warning:
                if (borderObject != null) borderObject.SetActive(true);
                baseColor = warningColor;
                if (borderImage != null) borderImage.color = baseColor;
                if (useAnimation) StartAnimation();
                break;

            case AlarmLevel.Alert:
                if (borderObject != null) borderObject.SetActive(true);
                baseColor = alertColor;
                if (borderImage != null) borderImage.color = baseColor;
                if (useAnimation) StartAnimation();
                break;
        }
    }

    void StartAnimation()
    {
        if (!useAnimation || borderImage == null) return;

        isAnimationActive = true;
        animationTime = 0f;
    }

    void Update()
    {
        if (!isAnimationActive || borderImage == null) return;

        animationTime += Time.deltaTime * animationSpeed;

        switch (animationType)
        {
            case AnimationType.Pulse:
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(animationTime) + 1f) * 0.5f);
                borderImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                break;

            case AnimationType.Rotate:
                borderRectTransform.rotation = Quaternion.Euler(0, 0, animationTime * 45f);
                break;

            case AnimationType.Scale:
                float scale = Mathf.Lerp(0.9f, 1.1f, (Mathf.Sin(animationTime) + 1f) * 0.5f);
                borderRectTransform.localScale = originalScale * scale;
                break;
        }
    }
}