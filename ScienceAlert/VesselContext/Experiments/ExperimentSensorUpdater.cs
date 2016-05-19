using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
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

        [Inject] public ICriticalShutdownEvent CriticalFail { get; set; }

        private Dictionary<ExperimentSensor, SensorState> _sensorStateCache =
            new Dictionary<ExperimentSensor, SensorState>();

        private void Start()
        {
            _sensorStateCache = Sensors.ToDictionary(sensor => sensor,
                sensor =>
                {
                    sensor.UpdateSensorValues();

                    return new SensorState(sensor.Experiment, sensor.CurrentSubject, 0f, 0f, 0f, false, false, false);
                });

            foreach (var sensor in Sensors)
                DispatchChangedSignal(sensor);
        }


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

            var newState = new SensorState(sensor.Experiment, sensor.CurrentSubject, sensor.CollectionValue,
                sensor.TransmissionValue, sensor.LabValue, sensor.Onboard, sensor.Available, sensor.ConditionsMet);

            var oldState = _sensorStateCache[sensor];

            SensorStatusChanged.Dispatch(new SensorStatusChange(newState, oldState));

            _sensorStateCache[sensor] = newState;

            print("Dispatch time: " + (Time.realtimeSinceStartup - dispatchTimerStart).ToString("F5") +
                  " for " + sensor.Experiment.id);
        }

        private void ShutdownDueToError()
        {
            enabled = false;

            Log.Warning("Attempting to shut down ScienceAlert because something went wrong while updating sensors and it will probably spam the log if we attempt to continue");

            CriticalFail.Do(s => s.Dispatch()); 
        }
    }
}
