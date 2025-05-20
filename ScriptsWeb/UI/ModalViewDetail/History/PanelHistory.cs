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
        //Components
        List<Button> btnMonthList;
        (TMP_InputField from, TMP_InputField to) txbDatetime;
        List<Button> btnSensorList;
        Button btnConfirm;

        //Data
        DateTime dtFrom, dtTo;
        (int boardId, int sensorId)? sensorAddress = null;    

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
            btnMonthList = transform.Find("pnlBtnsMonth").GetComponentsInChildren<Button>().ToList();
            txbDatetime = (
                    transform.Find("pnlDatetimeRange").Find("txbDatetimeFrom").GetComponent<TMP_InputField>(),
                    transform.Find("pnlDatetimeRange").Find("txbDatetimeTo").GetComponent<TMP_InputField>()
                );
            btnSensorList = transform.Find("pnlBtnsSensor").GetComponentsInChildren<Button>().ToList();
            btnConfirm = transform.Find("btnSearch").GetComponent<Button>();
        }

        public void Start()
        {
            txbDatetime.from.onValueChanged.AddListener(value => OnChangeDateTime(true, value));
            txbDatetime.from.onEndEdit.AddListener(value => OnEndEditDateTime(true, value));
            txbDatetime.from.text = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd");

            txbDatetime.to.onValueChanged.AddListener(value => OnChangeDateTime(false, value));
            txbDatetime.to.onEndEdit.AddListener(value => OnEndEditDateTime(false, value));
            txbDatetime.to.text = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd");

            btnMonthList.ForEach(item =>
            {
                int months = (btnMonthList.IndexOf(item) + 1) * 3;
                item.onClick.AddListener(() => OnClickMonth(months));
            });

            btnSensorList.ForEach(item =>
            {
                item.onClick.AddListener(() => OnClickSensor(item));
            });
            btnSensorList.First().onClick.Invoke();

            btnConfirm.onClick.AddListener(OnClickSearch);
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
        private void OnClickMonth(int months)
        {
            txbDatetime.from.text = DateTime.UtcNow.AddHours(9).AddMonths(-months).ToString("yyyy-MM-dd");
            txbDatetime.to.text = DateTime.UtcNow.AddHours(9).ToString("yyyy-MM-dd");
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
            if (!sensorAddress.HasValue) throw new Exception("기록을 조회하기위한 센서가 선택되지 않았습니다.");

            UiManager.Instance.Invoke(UiEventType.RequestSearchHistory, (sensorAddress.Value.boardId, sensorAddress.Value.sensorId, dtFrom, dtTo));
        }

        #endregion


    }
}
