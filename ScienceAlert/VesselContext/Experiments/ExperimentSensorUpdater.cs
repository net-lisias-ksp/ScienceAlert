using System;
using System.Collections.Generic;
using System.Reflection;
using ReeperCommon.Extensions;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{

// ReSharper disable once ClassNeverInstantiated.Global
    public class ExperimentSensorUpdater : MonoBehaviour
    {
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
        [Inject] public List<ExperimentSensor> Sensors { get; set; }
        [Inject] public SignalExperimentSensorStatusChanged SensorStatusChanged { get; set; }
        [Inject] public SignalCriticalShutdown CriticalFail { get; set; }

// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            try
            {
                //var start = Time.realtimeSinceStartup;

                // todo: split across several frames
                foreach (var m in Sensors)
                {
                    m.ClearChangedFlag();
                    m.UpdateSensorValues();

                    

                    if (m.HasChanged)
                    {
                        var dispatchTimerStart = Time.realtimeSinceStartup;

                        SensorStatusChanged.Dispatch(new ExperimentSensorState(m.Experiment, m.CollectionValue,
                            m.TransmissionValue, m.LabValue, m.Onboard, m.Available, m.ConditionsMet));

                        print("Dispatch time: " + (Time.realtimeSinceStartup - dispatchTimerStart).ToString("F5") +
                              " for " + m.Experiment.id);
                    }
                }

                //print("Time used: " + (Time.realtimeSinceStartup - start).ToString("F5") + " sec for " + Sensors.Count +
                //      " sensors");
            }
            catch (Exception e)
            {
                Log.Error("Error while updating sensors: " + e);

                ShutdownDueToError();
            }
        }


        private void ShutdownDueToError()
        {
            enabled = false;
            Assembly.GetExecutingAssembly().DisablePlugin();

            Log.Warning("Attempting to shut down ScienceAlert");

            CriticalFail.Dispatch(); // relatively high chance of failure; if something is wrong with either context, another exception will be thrown but we're hosed already
        }
    }
}
