using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
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


        public void OnStatusUpdateRequested(ScienceExperiment scienceExperiment)
        {
            if (scienceExperiment == null) throw new ArgumentNullException("scienceExperiment");

            Log.TraceMessage();

            var relatedSensor = Sensors.FirstOrDefault(s => s.Experiment.id == scienceExperiment.id).ToMaybe();

            if (!relatedSensor.Any())
                throw new ArgumentException("No matching sensor for " + scienceExperiment.id, "scienceExperiment");

            DispatchChangedSignal(relatedSensor.Value);
        }


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
                        DispatchChangedSignal(m);
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



        private void DispatchChangedSignal(ExperimentSensor sensor)
        {
            var dispatchTimerStart = Time.realtimeSinceStartup;

            SensorStatusChanged.Dispatch(new ExperimentSensorState(sensor.Experiment, sensor.CollectionValue,
                sensor.TransmissionValue, sensor.LabValue, sensor.Onboard, sensor.Available, sensor.ConditionsMet));

            print("Dispatch time: " + (Time.realtimeSinceStartup - dispatchTimerStart).ToString("F5") +
                  " for " + sensor.Experiment.id);
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
