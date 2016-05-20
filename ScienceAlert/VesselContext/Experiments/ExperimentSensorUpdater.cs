using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ScienceAlert.Game;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{

// ReSharper disable once ClassNeverInstantiated.Global
    class ExperimentSensorUpdater : MonoBehaviour
    {
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable once CollectionNeverUpdated.Global
        [Inject] public ReadOnlyCollection<IExperimentSensor> Sensors { get; set; }
        [Inject] public SignalExperimentSensorStatusChanged SensorStatusChanged { get; set; }
        [Inject] public ICriticalShutdownEvent CriticalFail { get; set; }

        private Dictionary<IExperimentSensor, IExperimentSensorState> _sensorStateCache =
            new Dictionary<IExperimentSensor, IExperimentSensorState>(new ExperimentSensorComparer());

        private class InitialSensorState : IExperimentSensorState
        {
            public ScienceExperiment Experiment { get; private set; }
            public IScienceSubject Subject { get; private set; }

            public float CollectionValue { get { return 0f; }}
            public float TransmissionValue { get { return 0f; }}
            public float LabValue { get { return 0f; }}
            public bool Onboard { get { return false; }}
            public bool Available { get { return false; }}
            public bool ConditionsMet {get { return false; }}

            public InitialSensorState([NotNull] ScienceExperiment experiment, [NotNull] IScienceSubject subject)
            {
                if (experiment == null) throw new ArgumentNullException("experiment");
                if (subject == null) throw new ArgumentNullException("subject");

                Experiment = experiment;
                Subject = subject;
            }
        }


        private class ExperimentSensorComparer : IEqualityComparer<IExperimentSensor>
        {
            public bool Equals(IExperimentSensor x, IExperimentSensor y)
            {
                return x.Experiment.id == y.Experiment.id;
            }

            public int GetHashCode(IExperimentSensor obj)
            {
                return obj.Experiment.GetHashCode();
            }
        }


        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _sensorStateCache = Sensors.ToDictionary(sensor => sensor,
                sensor => new InitialSensorState(sensor.Experiment, sensor.Subject) as IExperimentSensorState, new ExperimentSensorComparer());

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

                // todo: split across several frames?
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



        private void DispatchChangedSignal(IExperimentSensor sensor)
        {
            var dispatchTimerStart = Time.realtimeSinceStartup;

            var newState = sensor.State;

            try
            {
                var oldState = _sensorStateCache[sensor];

                SensorStatusChanged.Dispatch(new SensorStatusChange(newState, oldState));

                _sensorStateCache[sensor] = newState;

                print("Dispatch time: " + (Time.realtimeSinceStartup - dispatchTimerStart).ToString("F5") +
                      " for " + sensor.Experiment.id);
            }
            catch (KeyNotFoundException)
            {
                Log.Error("Couldn't find key: " + sensor.With(s => s.Experiment).Return(e => e.id, "<null>"));
                Log.Error("Current keys:");
                foreach (var kvp in _sensorStateCache)
                    Log.Error("Key: " + kvp.Key.Experiment.id);

                Log.Error("Finished listing keys");
                throw;
            }
        }

        private void ShutdownDueToError()
        {
            enabled = false;

            Log.Warning("Attempting to shut down ScienceAlert because something went wrong while updating sensors and it will probably spam the log if we attempt to continue");

            CriticalFail.Do(s => s.Dispatch()); 
        }
    }
}
