using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.Annotations;
using ScienceAlert.Game;
using UnityEngine;

namespace ScienceAlert
{
    public class SensorController : ISensorController
    {
        private readonly IResearchAndDevelopment _rnd;
        private readonly ISensorFactory _sensorFactory;
        private List<ISensor> _experimentSensors = new List<ISensor>();
        private IEnumerator _updateLoop;

        public SensorController(
            [NotNull] IResearchAndDevelopment rnd, 
            [NotNull] ISensorFactory sensorFactory)
        {
            if (rnd == null) throw new ArgumentNullException("rnd");
            if (sensorFactory == null) throw new ArgumentNullException("sensorFactory");

            _rnd = rnd;
            _sensorFactory = sensorFactory;
            _updateLoop = PollSensors();
        }


        public void Update()
        {
            _updateLoop.MoveNext();
        }


        public void CreateSensors()
        {
            _experimentSensors = _rnd.Experiments.Select(_sensorFactory.Create).ToList();
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
        }
    }
}
