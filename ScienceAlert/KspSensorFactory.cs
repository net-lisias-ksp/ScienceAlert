using System;
using ScienceAlert.Annotations;
using ScienceAlert.Providers;

namespace ScienceAlert
{
    public class KspSensorFactory : ISensorFactory
    {
        public KspSensorFactory()
        {

        }


        public ISensor Create(
            [NotNull] ScienceExperiment experiment,
            [NotNull] Action<ISensor> onTriggered)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (onTriggered == null) throw new ArgumentNullException("onTriggered");

            throw new NotImplementedException();
        }
    }
}
