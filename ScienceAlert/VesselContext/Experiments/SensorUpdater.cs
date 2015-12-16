using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using strange.extensions.mediation.impl;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
    /// <summary>
    /// Note to self: binding a pooled Command to a cross context signal is buggy, doing so without
    /// a pool works but is slow, and so this somewhat kludgy compromise is born
    /// </summary>
    public class SensorUpdater : MonoBehaviour
    {
        private ISensor[] _sensors;

        [Inject]
        public IEnumerable<ISensor> Sensors
        {
            get { return _sensors; }
            set { _sensors = value.ToArray(); }
        }

        private void Awake()
        {
            enabled = false;
            _sensors = Enumerable.Empty<ISensor>().ToArray();
        }


        [PostConstruct]
        public void Initialize()
        {
            Log.Warning("SensorUpdater.Initialize");
            enabled = true;
        }


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
