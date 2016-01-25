using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentSensorUpdater : MonoBehaviour
    {
        [Inject] public List<ExperimentSensor> Sensors { get; set; }
        [Inject] public SignalExperimentSensorStatusChanged SensorStatusChanged { get; set; }

// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            // todo: split across several frames
            foreach (var m in Sensors)
            {
                m.ClearChangedFlag();
                m.UpdateSensorValues();

                if (m.HasChanged)
                    SensorStatusChanged.Dispatch(new ExperimentSensorState(m.Experiment, m.CollectionValue,
                        m.TransmissionValue, m.LabValue, m.Onboard, m.Available));
            }
        }
    }
}
