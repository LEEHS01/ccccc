using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemThreshold : MonoBehaviour
{
    
    SensorModel sensorData;
    TMP_Text lblSensorName;
    TMP_InputField txbWarningValue, txbSeriousValue;
    Toggle tglIsFixing;

    [SerializeField] public int sensorId;
    // readonly static (float max, float min) allowedRange = (3000f, 0f); // 허용된 임계값 범위

    public static (float max, float min) GetAllowedRange(int sensorId)
    {
        return sensorId switch
        {
            1 or 2 => (300f, 0f),    // 센서 1, 2: 0~300 범위
            3 => (4000f, 0f),        // 센서 3: 0~4000 범위
            _ => (300f, 0f)          // 기본값
        };
    }

    private void Start()
    {
        // UI 컴포넌트 찾기
        lblSensorName = transform.Find("txtSensorname").GetComponent<TMP_Text>();
        txbWarningValue = transform.Find("InputWarningvalue").GetComponent<TMP_InputField>();
        txbSeriousValue = transform.Find("InputSeriousvalue").GetComponent<TMP_InputField>();
        tglIsFixing = transform.Find("tglIsFixing").GetComponent<Toggle>();
    }

    // 지정된 sensorId로 데이터 로드
    public void LoadData()
    {
        var sensors = UiManager.Instance.modelProvider.GetSensors();
        var targetSensor = sensors.FirstOrDefault(s => s.sensor_id == sensorId);

        if (targetSensor != null)
        {
            SetData(targetSensor);
            Debug.Log($"센서 로드 완료: {targetSensor.sensor_name} (ID: {targetSensor.sensor_id})");
        }
        else
        {
            Debug.LogWarning($"센서 ID {sensorId}를 찾을 수 없습니다!");
        }
    }

    public void SetData(SensorModel sensor)
    {
        if (lblSensorName == null) Start();

        this.sensorData = sensor;
        lblSensorName.text = sensor.sensor_name;
        txbWarningValue.text = sensor.threshold_warning.ToString("F1");
        txbSeriousValue.text = sensor.threshold_serious.ToString("F1");
        tglIsFixing.SetIsOnWithoutNotify(sensor.is_fixing);
    }

    public SensorModel GetUpdatedSensorData()
    {
        if (sensorData == null) return null;

        // 🎯 센서별 허용 범위 가져오기
        var allowedRange = GetAllowedRange(sensorData.sensor_id);

        // 입력값 검증 및 변환
        if (float.TryParse(txbWarningValue.text, out float warningValue) &&
            float.TryParse(txbSeriousValue.text, out float seriousValue))
        {
            // 🎯 경계값 > 경보값 검증
            if (seriousValue >= warningValue)
            {
                UiManager.Instance.Invoke(UiEventType.PopupError, ("임계값 수정 실패", $"경계값은 경보값보다 작아야 합니다."));
                return null;
            }

            // 🎯 센서별 범위 검증
            if (seriousValue > allowedRange.max || allowedRange.min > seriousValue ||
                warningValue > allowedRange.max || allowedRange.min > warningValue)
            {
                string sensorName = sensorData.sensor_name;
                UiManager.Instance.Invoke(UiEventType.PopupError, ("임계값 수정 실패",
                    $"{sensorName}의 임계값이 허용된 범위({allowedRange.min}~{allowedRange.max})를 초과했습니다."));
                return null;
            }

            // 업데이트된 센서 데이터 생성
            SensorModel updatedSensor = new SensorModel
            {
                board_id = sensorData.board_id,
                sensor_id = sensorData.sensor_id,
                sensor_name = sensorData.sensor_name,
                threshold_warning = warningValue,
                threshold_serious = seriousValue,
                is_using = sensorData.is_using,
                is_fixing = tglIsFixing.isOn
            };

            return updatedSensor;
        }
        else
        {
            UiManager.Instance.Invoke(UiEventType.PopupError, ("임계값 수정 실패", $"올바른 숫자를 입력해주세요."));
            return null;
        }
    }

    /* public SensorModel GetUpdatedSensorData()
     {
         if (sensorData == null) return null;

         var allowedRange = GetAllowedRange(sensorData.sensor_id);

         // 입력값 검증 및 변환
         if (float.TryParse(txbWarningValue.text, out float warningValue) &&
             float.TryParse(txbSeriousValue.text, out float seriousValue))
         {
             // 유효성 검사

             if (seriousValue >= warningValue)
             {
                 UiManager.Instance.Invoke(UiEventType.PopupError, ("임계값 수정 실패", $"경계값은 경보값보다 작아야 합니다."));
                 return null;
             }
             if (seriousValue > allowedRange.max || allowedRange.min > seriousValue || warningValue > allowedRange.max || allowedRange.min > warningValue)
             {
                 UiManager.Instance.Invoke(UiEventType.PopupError, ("임계값 수정 실패", $"임계값이 허용된 실수 범위({allowedRange.min}~{allowedRange.max})를 초과했습니다."));
                 return null;
             }


             // 업데이트된 센서 데이터 생성
             SensorModel updatedSensor = new SensorModel
             {
                 board_id = sensorData.board_id,
                 sensor_id = sensorData.sensor_id,
                 sensor_name = sensorData.sensor_name,
                 threshold_warning = warningValue,
                 threshold_serious = seriousValue,
                 is_using = sensorData.is_using,
                 is_fixing = tglIsFixing.isOn
             };

             return updatedSensor;
         }
         else
         {
             UiManager.Instance.Invoke(UiEventType.PopupError, ("임계값 수정 실패", $"올바른 숫자를 입력해주세요."));
             return null;
         }
     }*/
}