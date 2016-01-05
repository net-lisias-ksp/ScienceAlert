using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensorMonitorUpdater : MonoBehaviour
    {
        [Inject] public List<IExperimentSensorMonitor> SensorMonitors { get; set; }
       

        private void Update()
        {
            foreach (var m in SensorMonitors)
                m.Update();
        }
    }
}
