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

    [SerializeField] public int sensorId;

    private void Start()
    {
        // UI 컴포넌트 찾기
        lblSensorName = transform.Find("txtSensorname").GetComponent<TMP_Text>();
        txbWarningValue = transform.Find("InputWarningvalue").GetComponent<TMP_InputField>();
        txbSeriousValue = transform.Find("InputSeriousvalue").GetComponent<TMP_InputField>();

        // 실시간 검증 이벤트 등록(지워도됨)
        txbWarningValue.onValueChanged.AddListener(_ => OnWarningValueChanged());
        txbSeriousValue.onValueChanged.AddListener(_ => OnSeriousValueChanged());
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
    }

    public SensorModel GetUpdatedSensorData()
    {
        if (sensorData == null) return null;

        // 입력값 검증 및 변환
        if (float.TryParse(txbWarningValue.text, out float warningValue) &&
            float.TryParse(txbSeriousValue.text, out float seriousValue))
        {
            // 유효성 검사
            if (warningValue < 0 || seriousValue < 0)
            {
                Debug.LogWarning($"[{sensorData.sensor_name}] 임계값은 0 이상이어야 합니다.");
                return null;
            }

            if (seriousValue >= warningValue)
            {
                Debug.LogWarning($"[{sensorData.sensor_name}] 경계값은 경보값보다 작아야 합니다.");
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
                //threshold_critical = sensorData.threshold_critical, //이거 있는거 맞나요?erd사진에는 있긴하던데
                is_using = sensorData.is_using,
                is_fixing = sensorData.is_fixing
            };

            return updatedSensor;
        }
        else
        {
            Debug.LogWarning($"[{sensorData.sensor_name}] 올바른 숫자를 입력해주세요.");
            return null;
        }
    }

    // 입력값 실시간 검증
    public void OnWarningValueChanged()
    {
        if (float.TryParse(txbWarningValue.text, out float warningValue) &&
            float.TryParse(txbSeriousValue.text, out float seriousValue))
        {
            if (seriousValue >= warningValue)
            {
                txbSeriousValue.text = (warningValue - 1).ToString("F1");
            }
        }
    }

    public void OnSeriousValueChanged()
    {
        if (float.TryParse(txbWarningValue.text, out float warningValue) &&
            float.TryParse(txbSeriousValue.text, out float seriousValue))
        {
            if (seriousValue >= warningValue)
            {
                txbWarningValue.text = (seriousValue + 1).ToString("F1");
            }
        }
    }
}