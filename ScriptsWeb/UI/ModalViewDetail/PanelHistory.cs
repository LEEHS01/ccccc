using Onthesys.WebBuild;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.ScriptsWeb.UI
{
    internal class PanelHistory : MonoBehaviour
    {
        ModelProvider modelProvider => Onthesys.WebBuild.UiManager.Instance.modelProvider;

        //Components
        //List<Button> btnSensorList;
        List<Button> btnTimespanList;
        (TMP_InputField from, TMP_InputField to) txbDatetime;
        Button btnConfirm;
        //(TMP_Text max, TMP_Text min, TMP_Text avg, TMP_Text std) lblStats;
        ((TMP_Text threshold, TMP_Text count, TMP_Text latestTime, TMP_Text durationSum) warning,
        (TMP_Text threshold, TMP_Text count, TMP_Text latestTime, TMP_Text durationSum) serious) statLbls;

        //Data
        DateTime dtFrom, dtTo;
        int? sensorId = 1;

        //Func
        public DateTime DatetimeFrom
        {
            get => dtFrom;
            set
            {
                dtFrom = value;
                txbDatetime.from.SetTextWithoutNotify(dtFrom.ToString("yyyy-MM-dd"));
                dtFrom = dtFrom.AddHours(-dtFrom.Hour);
                dtFrom = dtFrom.AddMinutes(-dtFrom.Minute);
                dtFrom = dtFrom.AddSeconds(-dtFrom.Second);

            }
        }
        public DateTime DateTimeTo
        {
            get => dtTo;
            set
            {
                dtTo = value;
                txbDatetime.to.SetTextWithoutNotify(dtTo.ToString("yyyy-MM-dd"));

                if (dtTo.Date != DateTime.UtcNow.AddHours(9).Date)
                {
                    dtTo = dtTo.AddHours(23 - dtTo.Hour);
                    dtTo = dtTo.AddMinutes(59 - dtTo.Minute);
                    dtTo = dtTo.AddSeconds(59 - dtTo.Second);
                }
                else
                {
                    dtTo = DateTime.UtcNow.AddHours(9);
                }
            }
        }


        #region [Initiate]
        private void Awake()
        {
            Transform pnl = transform.Find("SearchPanel");
            statLbls = (
                (
                    pnl.Find("pnlWarning").Find("lblThreshold").GetComponent<TMP_Text>(),
                    pnl.Find("pnlWarning").Find("lblCount").GetComponent<TMP_Text>(),
                    pnl.Find("pnlWarning").Find("lblLatest").GetComponent<TMP_Text>(),
                    pnl.Find("pnlWarning").Find("lblDurationSum").GetComponent<TMP_Text>()
                ),
                (
                    pnl.Find("pnlSerious").Find("lblThreshold").GetComponent<TMP_Text>(),
                    pnl.Find("pnlSerious").Find("lblCount").GetComponent<TMP_Text>(),
                    pnl.Find("pnlSerious").Find("lblLatest").GetComponent<TMP_Text>(),
                    pnl.Find("pnlSerious").Find("lblDurationSum").GetComponent<TMP_Text>()
                )
            );

            btnTimespanList = transform.parent.parent.Find("btnCon").GetComponentsInChildren<Button>().ToList();
            txbDatetime = (
                transform.parent.parent.Find("pnlDatetimeRange").Find("txbDatetimeFrom").GetComponent<TMP_InputField>(),
                transform.parent.parent.Find("pnlDatetimeRange").Find("txbDatetimeTo").GetComponent<TMP_InputField>()
            );

            btnConfirm = transform.parent.parent.Find("pnlDatetimeRange").Find("Button").GetComponent<Button>();
        }

        void Start()
        {
            txbDatetime.from.onValueChanged.AddListener(value => OnChangeDateTime(true, value));
            txbDatetime.from.onEndEdit.AddListener(value => OnEndEditDateTime(true, value));
            txbDatetime.from.text = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd");

            txbDatetime.to.onValueChanged.AddListener(value => OnChangeDateTime(false, value));
            txbDatetime.to.onEndEdit.AddListener(value => OnEndEditDateTime(false, value));
            txbDatetime.to.text = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd");

            foreach(var item in btnTimespanList)
                item.onClick.AddListener(() => OnClickTimespan(btnTimespanList.IndexOf(item)));


            btnConfirm.onClick.AddListener(OnClickSearch);

            UiManager.Instance.Register(UiEventType.SelectSensorWithinTab, OnSelectSensorWithinTab);
            UiManager.Instance.Register(UiEventType.ChangeTrendLineHistory, OnChangeTrendLineHistory);
        }

        private void OnChangeTrendLineHistory(object obj)
        {
            //통계 자료 수령
            List<AlarmStatisticModel> alarmStatisticModels = modelProvider.GetAlarmStatistics()
                .Where(stat => stat.sensor_id == sensorId.Value)
                .ToList();
            AlarmStatisticModel warningStat = alarmStatisticModels.Find(stat => stat.GetAlarmLevel() == StatusType.WARNING) ??
                new AlarmStatisticModel(sensorId.Value, StatusType.WARNING.ToDbString(), 0, DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss"), 0f);
            AlarmStatisticModel seriousStat = alarmStatisticModels.Find(stat => stat.GetAlarmLevel() == StatusType.SERIOUS) ??
                new AlarmStatisticModel(sensorId.Value, StatusType.SERIOUS.ToDbString(), 0, DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss"), 0f);

            //센서 모델 획득
            SensorModel sensorModel = modelProvider.GetSensor(1, sensorId.Value);
            
            //현재 알람 상황 획득
            List<AlarmLogModel> alarms = modelProvider.GetAlarmLogList().Where(log => log.sensor_id == sensorId.Value /*&& log.solved_time == null*/).ToList();
            Debug.LogWarning("alarms.Count : " + alarms.Count);
            alarms.ForEach(ala => Debug.LogWarning(ala.alarm_level));

            StatusType statusType = StatusType.NORMAL;
            if(alarms.Find(alarm => alarm.GetAlarmLevel() == StatusType.WARNING) is not null)
                statusType = StatusType.WARNING;
            else if (alarms.Find(alarm => alarm.GetAlarmLevel() == StatusType.SERIOUS) is not null)
                statusType = StatusType.SERIOUS;


            //라벨들에 적용
            statLbls.warning.threshold.text = sensorModel.threshold_warning.ToString("F1");
            statLbls.warning.count.text = warningStat.count.ToString();
            statLbls.warning.latestTime.text = statusType == StatusType.WARNING? "진행 중" : warningStat.count != 0? warningStat.lastestTime.ToString("yy-MM-dd\nHH:mm:ss") : "찾을 수 없음";
            float durMinWarn = (warningStat.duration_sec / 60f);
            string durStrWarn = durMinWarn > 300f ? (durMinWarn / 60f).ToString("F1") + "시간" : durMinWarn.ToString("F1") + "분";
            statLbls.warning.durationSum.text = durStrWarn;

            statLbls.serious.threshold.text = sensorModel.threshold_serious.ToString("F1");
            statLbls.serious.count.text = seriousStat.count.ToString();
            statLbls.serious.latestTime.text = statusType != StatusType.NORMAL? "진행 중" : seriousStat.count != 0 ? seriousStat.lastestTime.ToString("yy-MM-dd\nHH:mm:ss") : "찾을 수 없음";
            float durMinseri = (seriousStat.duration_sec / 60f);
            string durStrseri = durMinseri > 300f ? (durMinseri / 60f).ToString("F1") + "시간" : durMinseri.ToString("F1") + "분";
            statLbls.serious.durationSum.text = durStrseri;

            //lblStats.max.text = measures.Max(m => m.measured_value).ToString("F2");
            //lblStats.min.text = measures.Min(m => m.measured_value).ToString("F2");
            //lblStats.avg.text = measures.Average(m => m.measured_value).ToString("F2");
            //lblStats.std.text = Math.Sqrt(measures.Average(m => Math.Pow(m.measured_value - measures.Average(m2 => m2.measured_value), 2))).ToString("F2");
        }

        private void OnSelectSensorWithinTab(object obj)
        {
            //Debug.Log("OnSelectSensorWithinTab try");
            if (obj is not int sensorId) return;
            this.sensorId = sensorId;
            //Debug.Log("OnSelectSensorWithinTab suc");
        }
        #endregion

        #region [EventListener]
        private void OnChangeDateTime(bool isFrom, string value)
        {
            try
            {
                DateTime dt = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                //미래 기록 조회 불가
                if (dt.Date > DateTime.UtcNow.AddHours(9).Date)
                    dt = DateTime.UtcNow.AddHours(9);

                //From < To가 참이게끔 교정
                if (!isFrom && dt < dtFrom)
                    DatetimeFrom = dt;

                if (isFrom && dt > dtTo)
                    DateTimeTo = dt;

                //저장
                if (isFrom) DatetimeFrom = dt; else DateTimeTo = dt;
            }
            catch { }
        }
        private void OnEndEditDateTime(bool isFrom, string value)
        {
            try
            {
                DateTime dt = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                //미래 기록 조회 불가
                if (dt.Date > DateTime.UtcNow.AddHours(9).Date)
                    dt = DateTime.UtcNow.AddHours(9);

                //From < To가 참이게끔 교정
                if (!isFrom && dt < dtFrom)
                    DatetimeFrom = dt;

                if (isFrom && dt > dtTo)
                    DateTimeTo = dt;

                //저장
                if (isFrom) DatetimeFrom = dt; else DateTimeTo = dt;
            }
            finally
            {
                txbDatetime.from.SetTextWithoutNotify(dtFrom.ToString("yyyy-MM-dd"));
                txbDatetime.to.SetTextWithoutNotify(dtTo.ToString("yyyy-MM-dd"));
            }
        }
        private void OnClickTimespan(int idxOfButton)
        {
            //idxOfButton
            //0,1,2... 3시간 하루 1주 보름 1달 1분기

            TimeSpan timeSpan;// = new TimeSpan(7, 0, 0, 0);
            TimeSpan rightMargin = new TimeSpan(0,0,0,0);// = new TimeSpan(7, 0, 0, 0);

            switch (idxOfButton)
            {
                case 0:
                    timeSpan = new TimeSpan(0, 0, 0, 0);
                    break;
                case 1:
                    timeSpan = new TimeSpan(7, 0, 0, 0);
                    break;
                case 2:
                    timeSpan = new TimeSpan(15, 0, 0, 0);
                    break;
                case 3:
                    timeSpan = new TimeSpan(30, 0, 0, 0);
                    break;
                case 4:
                    timeSpan = new TimeSpan(90, 0, 0, 0);
                    break;

                default: throw new Exception("사전에 정의되지 않은 버튼 인덱스가 입력되었습니다." + idxOfButton);
            }

            DateTime now = DateTime.UtcNow.AddHours(9);

            //txbDatetime.from.text = (now - rightMargin - timeSpan).ToString("yyyy-MM-dd");
            //txbDatetime.to.text = (now - rightMargin).ToString("yyyy-MM-dd");

            DatetimeFrom = now - rightMargin - timeSpan;
            DateTimeTo = now - rightMargin;
        }

        private void OnClickSensor(Button button)
        {
            //(int boardId, int sensorId) address = button.GetComponent<ModalHistoryFormSensor>().address;

            //this.sensorAddress = address;

            //btnSensorList.ForEach(item => item.interactable = true);
            //button.interactable = false;
        }

        private void OnClickSearch()
        {
            if (!sensorId.HasValue) throw new Exception("기록을 조회하기위한 센서가 선택되지 않았습니다.");

            UiManager.Instance.Invoke(UiEventType.RequestSearchHistory, (sensorId.Value, dtFrom, dtTo));
        }

        #endregion


    }
}
