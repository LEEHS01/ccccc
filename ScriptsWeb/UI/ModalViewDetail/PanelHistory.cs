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
        (TMP_Text max, TMP_Text min, TMP_Text avg, TMP_Text std) lblStats;

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

            lblStats = (
                pnl.Find("lbShowPanel").Find("LbMax").GetComponentInChildren<Image>().GetComponentInChildren<TMP_Text>(),
                pnl.Find("lbShowPanel").Find("LbMin").GetComponentInChildren<Image>().GetComponentInChildren<TMP_Text>(),
                pnl.Find("lbShowPanel 2").Find("LbAvg").GetComponentInChildren<Image>().GetComponentInChildren<TMP_Text>(),
                pnl.Find("lbShowPanel 2").Find("LbDeviation").GetComponentInChildren<Image>().GetComponentInChildren<TMP_Text>()
            );

            btnTimespanList = transform.parent.parent.Find("btnCon").GetComponentsInChildren<Button>().ToList();
            txbDatetime = (
                    transform.parent.parent.Find("pnlDatetimeRange").Find("txbDatetimeFrom").GetComponent<TMP_InputField>(),
                    transform.parent.parent.Find("pnlDatetimeRange").Find("txbDatetimeTo").GetComponent<TMP_InputField>()
                );
            //btnSensorList = transform.Find("pnlBtnsSensor").GetComponentsInChildren<Button>().ToList();
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


            Debug.Log("UiManager.Instance.Register try");
            UiManager.Instance.Register(UiEventType.SelectSensorWithinTab, OnSelectSensorWithinTab);
            UiManager.Instance.Register(UiEventType.ChangeTrendLineHistory, OnChangeTrendLineHistory);
            Debug.Log("UiManager.Instance.Register try");
        }

        private void OnChangeTrendLineHistory(object obj)
        {
            List<MeasureModel> measures = modelProvider.GetMeasureHistoryList();

            lblStats.max.text = measures.Max(m => m.measured_value).ToString("F2");
            lblStats.min.text = measures.Min(m => m.measured_value).ToString("F2");
            lblStats.avg.text = measures.Average(m => m.measured_value).ToString("F2");
            lblStats.std.text = Math.Sqrt(measures.Average(m => Math.Pow(m.measured_value - measures.Average(m2 => m2.measured_value), 2))).ToString("F2");
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
