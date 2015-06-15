using System;
using ReeperCommon.Logging;
using ScienceAlert.Annotations;
using ScienceAlert.Game;
using ScienceAlert.Providers;

namespace ScienceAlert
{
    public class Core
    {
        private readonly Settings _settings;
        private readonly ILog _log;
        private readonly ISensorController _sensorController;

        public Core([NotNull] Settings settings, [NotNull] ILog log)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (log == null) throw new ArgumentNullException("log");

            _settings = settings;
            _log = log;

            var rnd = new KspResearchAndDevelopment();

            _sensorController = new SensorController(new UnlockedScienceExperimentProvider(rnd), new KspSensorFactory());
            
        }


        public ~Core()
        {
            
        }


        public void Update()
        {
            _sensorController.Update();
        }
    }
}
