using System.Collections.Generic;
using ScienceAlert.UI;

namespace ScienceAlert.VesselContext.Experiments
{
    class ExperimentIdentifierComparer : IEqualityComparer<IExperimentIdentifier>
    {
        public bool Equals(IExperimentIdentifier x, IExperimentIdentifier y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(IExperimentIdentifier obj)
        {
            return obj.GetHashCode();
        }
    }
}
