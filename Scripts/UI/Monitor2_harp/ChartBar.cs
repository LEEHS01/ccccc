using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;


internal class ChartBar :MonoBehaviour
{
    internal UILineRenderer2 line;
    internal List<TMP_Text> hours;
    internal List<TMP_Text> verticals;

    internal void CreatAxis(DateTime dt, float max)
    {
        SetMins(dt);
        SetVertical(max);
    }

    void SetMins(DateTime dt)
    {
        DateTime startDt = dt.AddHours(-4);
        var turm = (dt - startDt).TotalMinutes / this.hours.Count;

        for (int i = 0; i < this.hours.Count; i++)
        {
            var t = dt.AddMinutes(-(turm * i));
            this.hours[i].text = t.ToString("HH:mm");
        }
    }

    void SetVertical(float max)
    {
        var verticalMax = ((max + 1) / (verticals.Count - 1));

        for (int i = 0; i < this.verticals.Count; i++)
        {
            this.verticals[i].text = Math.Round((verticalMax * i), 2).ToString();
        }
    }
}

