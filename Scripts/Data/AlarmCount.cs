using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Onthesys.ExeBuild
{
    internal class AlarmCount
    {
        internal AlarmCount() => new AlarmCount(0, 0, 0, 0);
        internal AlarmCount(int green, int yellow, int red, int purple) 
        {
            this.green = green;
            this.yellow = yellow;
            this.red = red;
            this.purple = purple;
        }

        internal int green;
        internal int red;
        internal int yellow;
        internal int purple;//수정한 부분

        /*
        internal void CreateAlramDatas()
        {
            this.green = Random.Range(0, 4);
            this.red = Random.Range(0, 2);
            this.yellow = Random.Range(0, 2);
            this.purple = Random.Range(0, 2);//수정한 부분
        }

        internal void UpdateAlramDatas()
        {
            this.green = Random.Range(0, 4);
            this.red = Random.Range(0, 2);
            this.yellow = Random.Range(0, 2);
            this.purple = Random.Range(0, 2);//수정한 부분
        }
        */
        internal int GetRedYellow()
        {
            return this.red + this.yellow;
        }

        internal int GetGreen()
        {
            return this.green;
        }

        internal int GetRed()
        {
            return this.red;
        }

        internal int GetYellow()
        {
            return this.yellow;
        }

        internal int GetPurple()//수정한 부분
        {
            return this.purple;
        }

        internal void ForceAddAlramDatas(ToxinStatus status)
        {
            if (status == ToxinStatus.Green) this.green += 1;
            else if (status == ToxinStatus.Red) this.red += 1;
            else if (status == ToxinStatus.Yellow) this.yellow += 1;
        }

        internal void ForceReset()
        {
            this.green = 0;
            this.red = 0;
            this.yellow = 0;
            this.purple = 0;//수정한 부분
        }
    }
}


