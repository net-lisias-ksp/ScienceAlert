using System;
using System.Collections.ObjectModel;
using System.Reflection;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperKSP.Extensions;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{

// ReSharper disable once ClassNeverInstantiated.Global
    class ExperimentSensorUpdater : MonoBehaviour
    {
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
        [Inject] public ReadOnlyCollection<ExperimentSensor> Sensors { get; set; }
        [Inject] public SignalExperimentSensorStatusChanged SensorStatusChanged { get; set; }

        [Inject] public SignalCriticalShutdown CriticalFail { get; set; }


// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            Profiler.BeginSample("ExperimentSensorUpdater.Update");

            try
            {
                //var start = Time.realtimeSinceStartup;

                // todo: split across several frames
                foreach (var m in Sensors)
                {
                    m.ClearChangedFlag();
                    m.UpdateSensorValues();



                    if (m.HasChanged)
                        DispatchChangedSignal(m);
                }

                //print("Time used: " + ((Time.realtimeSinceStartup - start) * 1000f).ToString("F2") + " ms for " + Sensors.Count +
                //      " sensors");
            }
            catch (Exception e)
            {
                Log.Error("Error while updating sensors: " + e);

                ShutdownDueToError();
            }
            finally
            {
                Profiler.EndSample();
            }
        }



        private void DispatchChangedSignal(ExperimentSensor sensor)
        {
            var dispatchTimerStart = Time.realtimeSinceStartup;

            SensorStatusChanged.Dispatch(new ExperimentSensorState(sensor.Experiment, sensor.CurrentSubject, sensor.CollectionValue,
                sensor.TransmissionValue, sensor.LabValue, sensor.Onboard, sensor.Available, sensor.ConditionsMet));

            print("Dispatch time: " + (Time.realtimeSinceStartup - dispatchTimerStart).ToString("F5") +
                  " for " + sensor.Experiment.id);
        }

        private void ShutdownDueToError()
        {
            enabled = false;
            Assembly.GetExecutingAssembly().DisablePlugin();

            Log.Warning("Attempting to shut down ScienceAlert");

            CriticalFail.Do(s => s.Dispatch()); // relatively high chance of failure; if something is wrong with either context, another exception will be thrown but we're hosed already
        }
    }
}
