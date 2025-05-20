using Onthesys.ExeBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class AreaData
{
    internal int areaId;
    internal string areaName;
    internal AreaType areaType;


    internal enum AreaType
    {
        Ocean,
        Nuclear
    }



    internal static AreaData FromAreaDataModel(AreaDataModel areaModel) => new()
    {
        areaId = areaModel.areaIdx,
        areaName = areaModel.areaNm,
        areaType = (AreaType)areaModel.areaType,
    };

}
