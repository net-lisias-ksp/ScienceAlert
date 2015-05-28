using System;
using ReeperCommon.Logging;
using ScienceAlert.Annotations;

namespace ScienceAlert
{
    public class Core
    {
        private readonly Settings _settings;
        private readonly ILog _log;


        public Core([NotNull] Settings settings, [NotNull] ILog log)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (log == null) throw new ArgumentNullException("log");

            _settings = settings;
            _log = log;
        }


        public void Update()
        {
            
        }
    }
}
