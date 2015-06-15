using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScienceAlert.Annotations;
using ScienceAlert.Game;
using ScienceAlert.Providers;

namespace ScienceAlert
{
    public class SensorController : ISensorController
    {
        private readonly IScienceExperimentProvider _scienceExperimentProvider;
        private readonly ISensorFactory _sensorFactory;
        private List<ISensor> _experimentSensors = new List<ISensor>();
        private IEnumerator _updateLoop;

        public SensorController(
            [NotNull] IScienceExperimentProvider scienceExperimentProvider,
            [NotNull] ISensorFactory sensorFactory)
        {
            if (scienceExperimentProvider == null) throw new ArgumentNullException("scienceExperimentProvider");
            if (sensorFactory == null) throw new ArgumentNullException("sensorFactory");

            _scienceExperimentProvider = scienceExperimentProvider;
            _sensorFactory = sensorFactory;
            _updateLoop = PollSensors();
        }


        public void Update()
        {
            _updateLoop.MoveNext();
        }


        public void CreateSensors()
        {
            _experimentSensors = _scienceExperimentProvider.Get().Select(exp => 
                _sensorFactory.Create(exp, OnSensorActivated)).ToList();


            _updateLoop = PollSensors();
        }


        private IEnumerator PollSensors()
        {
            var sensors = _experimentSensors;

            while (true)
            {
                foreach (var sensor in sensors)
                {
                    sensor.Poll();
                    yield return 0;
                }

                if (!_experimentSensors.Any()) yield return 0;
            }
// ReSharper disable once FunctionNeverReturns
        }


        private void OnSensorActivated(ISensor sensor)
        {
            
        }
    }
}
