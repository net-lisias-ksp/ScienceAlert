using System.Collections.Generic;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentSensorMonitorUpdater : MonoBehaviour
    {
// ReSharper disable once MemberCanBePrivate.Global
// ReSharper disable once UnusedAutoPropertyAccessor.Global
        [Inject] public List<IExperimentSensorMonitor> SensorMonitors { get; set; }
       

// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            foreach (var m in SensorMonitors)
                m.UpdateSensorStates();
        }
    }
}
