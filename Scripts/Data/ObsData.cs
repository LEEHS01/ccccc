using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Onthesys.ExeBuild
{
    internal class ObsData
    {
        internal int id;
        internal AreaData.AreaType type;
        internal int areaId;
        internal string areaName;
        internal string obsName;
        internal int step;

        internal string src_video1 = "rtsp://admin:HNS_qhdks_!Q@W3@192.168.1.108:554/video1?profile=high";//"rtsp://admin:HNS_qhdks_!Q@W3@115.91.85.42/video1?profile=high";
        internal string src_video2 = "rtsp://admin:HNS_qhdks_!Q@W3@192.168.1.108:554/video1?profile=high";//"C:\\Users\\onthesys\\Downloads\\happyCat.mp4";//"rtsp://admin:HNS_qhdks_!Q@W3@115.91.85.42/video1?profile=high";
        internal string src_video_up = "";
        internal string src_video_down = "";
        internal string src_video_left = "";
        internal string src_video_right = "";

        internal static ObsData FromObsModel(ObservatoryModel model)
            => new ObsData(model.areanm, model.areaidx, model.obsnm, (AreaData.AreaType)model.areatype, model.obsidx, model.in_cctvUrl, model.out_cctvUrl);

        internal ObsData(string areaName, int areaidx, string obsName, AreaData.AreaType type, int id, string src_video1, string src_video2)
        {
            this.areaName = areaName;
            this.areaId = areaidx;
            this.obsName = obsName;
            this.type = type;
            this.id = id;
            this.step = UnityEngine.Random.Range(0, 5);

            this.src_video1 = src_video1;
            this.src_video2 = src_video2;
        }

        private void UpdateStep(string step)
        {
            if (step != null)
            {
                switch (step.Trim())
                {
                    case "0020":
                        this.step = 1;
                        break;
                    case "0021":
                        this.step = 2;
                        break;
                    case "0023":
                        this.step = 3;
                        break;
                    case "0024":
                        this.step = 4;
                        break;
                    case "0025":
                        this.step = 5;
                        break;
                    default:
                        this.step = 5;
                        break;
                }
            }
        }
    }


    internal enum ToolStatus
    {
        STAY_0,
        START_1,
        PRE_2,
        WORK_3,
        WASH_4
    }

    internal enum CctvType 
    {
        OUTDOOR,
        EQUIPMENT,
    }
    

}


