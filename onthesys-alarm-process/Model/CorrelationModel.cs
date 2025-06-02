using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onthesys.WebBuild
{
    [Serializable]
    public class CorrelationModel
    {
        public string base_sensor_name;
        public string other_sensor_name;
        public float correlation;
    }

    [Serializable]
    public class CorrelationModelList
    {
        public List<CorrelationModel> items;
    }
}
