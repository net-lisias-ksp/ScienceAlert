using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ScienceAlert.UI;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
// ReSharper disable once ClassNeverInstantiated.Global
    class SensorUpdateAgent : MonoBehaviour, ISensorStateCache
    {
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable once CollectionNeverUpdated.Global
        [Inject] public ReadOnlyCollection<IExperimentSensor> Sensors { get; set; }
        [Inject] public SignalExperimentSensorStatusChanged SensorStatusChanged { get; set; }
        [Inject] public ICriticalShutdownEvent CriticalFail { get; set; }


        private class StateCacheKey : 
            IEquatable<IExperimentSensor>, 
            IEquatable<ScienceExperiment>,
            IEquatable<IExperimentIdentifier>,
            IEquatable<StateCacheKey>
        {
            private readonly IExperimentSensor _sensor;

            public StateCacheKey([NotNull] IExperimentSensor sensor)
            {
                if (sensor == null) throw new ArgumentNullException("sensor");
                _sensor = sensor;
            }

            public bool Equals(IExperimentSensor other)
            {
                if (other == null) return false;
                return _sensor.Experiment.id == other.Experiment.id;
            }

            public bool Equals(ScienceExperiment other)
            {
                if (other == null) return false;
                return _sensor.Experiment.id == other.id;
            }

            public bool Equals(IExperimentIdentifier other)
            {
                return other != null && other.Equals(_sensor.Experiment.id);
            }

            public bool Equals(StateCacheKey other)
            {
                return other != null && _sensor.Experiment.id == other._sensor.Experiment.id;
            }

            public override int GetHashCode()
            {
                return _sensor.Experiment.id.GetHashCode();
            }

            public override string ToString()
            {
                return "StateCacheKey: " + _sensor.Experiment.id;
            }
        }


        private Dictionary<StateCacheKey, ExperimentSensorState> _sensorStateCache =
            new Dictionary<StateCacheKey, ExperimentSensorState>();




        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _sensorStateCache = Sensors.ToDictionary(sensor => new StateCacheKey(sensor),
                sensor => new ExperimentSensorState(sensor.Experiment, sensor.Subject, 0f, 0f, 0f, false, false, false));

            foreach (var sensor in Sensors)
                DispatchChangedSignal(sensor);
        }


// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            try
            {
                // todo: split across several frames?
                foreach (var m in Sensors)
                {
                    m.ClearChangedFlag();
                    m.UpdateSensorValues();

                    if (m.HasChanged)
                        DispatchChangedSignal(m);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error while updating sensors: " + e);

                ShutdownDueToError();
            }
        }


        private StateCacheKey GetKey<T>(T target)
        {
            foreach (var key in _sensorStateCache.Keys)
            {
                var eq = key as IEquatable<T>;

                if (eq != null && eq.Equals(target))
                    return key;
            }
            throw new KeyNotFoundException("No key matches: " + target);
        }


        private void DispatchChangedSignal(IExperimentSensor sensor)
        {
            var newState = sensor.State;

            try
            {
                var key = GetKey(sensor);
                var oldState = _sensorStateCache[key];

                SensorStatusChanged.Dispatch(new SensorStatusChange(newState, oldState));

                _sensorStateCache[key] = newState;
            }
            catch (KeyNotFoundException e)
            {
                throw new ArgumentException("No sensors match " + sensor, "sensor", e);
            }
        }


        private void ShutdownDueToError()
        {
            enabled = false;

            Log.Warning("Attempting to shut down ScienceAlert because something went wrong while updating sensors and it will probably spam the log if we attempt to continue");

            CriticalFail.Do(s => s.Dispatch()); 
        }


        public ExperimentSensorState GetCachedState(ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            var key = GetKey(experiment);

            if (key == null)
                throw new ArgumentException("No sensor state for " + experiment.id, "experiment");

            return _sensorStateCache[key];
        }


        public ExperimentSensorState GetCachedState(IExperimentIdentifier identifier)
        {
            var key = GetKey(identifier);

            if (key == null)
                throw new ArgumentException("No sensor state for " + identifier);

            return _sensorStateCache[key];
        }
    }
}
