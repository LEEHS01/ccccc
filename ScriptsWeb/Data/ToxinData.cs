using Newtonsoft.Json;
using Onthesys.ExeBuild;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

namespace Onthesys.ExeBuild
{
    internal class ToxinData
    {
        internal int boardid;
        internal int hnsid;
        internal string hnsName;
        internal float serious;
        internal float warning;
        internal float duration;
        internal List<float> values;
        internal List<float> aiValues;
        internal List<float> diffValues;
        internal bool on = true;
        internal bool fix = false;
        internal ToxinStatus status = ToxinStatus.Green;

        

        internal ToxinData(HnsResourceModel model)
        {
            this.boardid = model.boardidx;
            this.hnsid = model.hnsidx;
            this.hnsName = model.hnsnm;
            this.serious = model.alahival == null ? 0 : (float)model.alahival;
            this.warning = model.alahihival == null ? 0 : (float)model.alahihival;
            this.duration = model.alahihisec == null ? 0 : (float)model.alahihisec;
            this.on = Convert.ToInt32(model.useyn) == 1;
            this.fix = Convert.ToInt32(model.inspectionflag) == 1;
            this.status = ToxinStatus.Green;
            this.values = new List<float>();
            this.aiValues = new List<float>();
            this.diffValues = new List<float>();
        }

        internal void UpdateValue(CurrentDataModel model)
        {
            //Debug.Log("ToxinData.UpdateValue");
            if (model != null)
            {
                this.serious = model.hi;
                this.warning = model.hihi;
                this.on = Convert.ToInt32(model.useyn) == 1;
                this.fix = Convert.ToInt32(model.fix) == 1;
                this.SetLastValue(model.val);
            }
        }

        private void SetLastValue(float? val)
        {
            //Chart가 24Point임 ---- 파악내용
            int countExpected = Mathf.FloorToInt((Option.TREND_DURATION_REALTIME * 60f) / Option.TREND_TIME_INTERVAL);
            //Debug.Log("countExpected : " + countExpected);
            if (this.values.Count >= countExpected)
            {
                this.values.RemoveAt(0);
            }

            //값이 없다면 무작위값을 추가.
            //실제로 값 들어오고 있음 ex. ToxinData.SetLastValue.val == 3.99
            //Debug.Log("ToxinData.SetLastValue.val == " + val.ToString());
            if (val == null)
            {
                int r = UnityEngine.Random.Range(0, (int)(warning * Option.TOXIN_STATUS_GREEN));
                this.values.Add(Mathf.Floor(((float)r / (float)warning) * 100f) / 100f);
            }
            else
            {
                this.values.Add((float)val);
            }
        }
        internal void CreateRandomValues()
        {
            int countExpected = Mathf.FloorToInt((Option.TREND_DURATION_REALTIME * 60f) / Option.TREND_TIME_INTERVAL);
            for (int i = 0; i < countExpected; i++)
            {
                int r = UnityEngine.Random.Range(0, (int)(warning * Option.TOXIN_STATUS_GREEN));
                values.Add(Mathf.Floor(((float)r / (float)warning) * 100f) / 100f);
            }
        }

        internal void CreateRandomValue(DateTime dt)
        {
            int r = UnityEngine.Random.Range(0, (int)(warning * Option.TOXIN_STATUS_GREEN));
            values.Add(Mathf.Floor(((float)r / (float)warning) * 100f) / 100f);
        }

        internal ToxinStatus GetStatus(string cd)
        {
            if (this.values.Count > 0)
            {
                var value = this.values.Last();
                if (value >= warning && !fix)
                {
                    return ToxinStatus.Red;
                }
                else if (serious > 0 && value >= serious && !fix)
                {
                    return ToxinStatus.Red;
                }
                else
                {
                    if (cd.Trim().Equals("0"))
                        return ToxinStatus.Green;
                    else
                        return fix ? ToxinStatus.Green : ToxinStatus.Yellow;
                }
            }
            return ToxinStatus.Green;
        }

        internal float GetLastValue()
        {
            return values.Count > 0? values.Last() : 0f;
        }

        internal float GetLastValuePercent()
        {
            return this.values.Last() / this.warning;
        }

        internal ToxinStatus GetStatus()
        {
            if ((this.values.Last() >= Option.TOXIN_STATUS_RED) && fix == false) 
                return ToxinStatus.Red;
            else if (this.values.Last() >= Option.TOXIN_STATUS_YELLOW) 
                return ToxinStatus.Yellow;
            else 
                return ToxinStatus.Green;
        }

    }

    internal enum ToxinStatus
    {
        Green,
        Yellow,
        Red,
        Purple//수정한 부분
    }
}


