using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
    /// <summary>
    /// Note to self: binding a pooled Command to a cross context signal is buggy, doing so without
    /// a pool works but is slow, and so this somewhat kludgy compromise is born
    /// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
    public class SensorManager : MonoBehaviour
    {
        private ISensor[] _sensors = {};

        [Inject] public IExperimentSensorFactory SensorFactory { get; set; }
        [Inject] public IEnumerable<ScienceExperiment> Experiments { get; set; }
        [Inject] public SignalActiveVesselModified ActiveVesselModified { get; set; }

        [PostConstruct(0)]
// ReSharper disable once UnusedMember.Global
        public void Initialize()
        {
            Log.Warning("Initializing SensorManager");
            _sensors = Experiments.Select(exp => SensorFactory.Create(exp)).ToArray();

            ActiveVesselModified.AddListener(OnActiveVesselModified);
        }


// ReSharper disable once UnusedMember.Local
        private void OnDestroy()
        {
            ActiveVesselModified.RemoveListener(OnActiveVesselModified);
        }


        private void OnActiveVesselModified()
        {
            Log.Warning("SensorManager.OnActiveVesselModified");

            SetOnboardStatusOfExperiments();
        }


        [PostConstruct(1)]
        public void SetOnboardStatusOfExperiments()
        {
            Log.Warning("Setting onboard status");
            foreach (var sensor in _sensors)
            {
                sensor.UpdateOnboardStatus();
            }
        }


// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            for (int i = 0; i < _sensors.Length; ++i)
            {
                // todo: vary update rate based on TimeWarp factor

                _sensors[i].Poll();
            }
        }
    }
}
