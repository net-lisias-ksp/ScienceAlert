using System;
using System.Linq;
using ReeperCommon.Containers;
using strange.extensions.injector.api;

namespace ScienceAlert.Game
{
// ReSharper disable BitwiseOperatorOnEnumWithoutFlags 
    public static class ExperimentUsageReqsExtension
    {
        public static bool IsFlagSet(this ExperimentUsageReqs toCheck, ExperimentUsageReqs flags)
        {
            return (((int)toCheck) & ((int)flags)) == (int)flags;
        }
    }


    [Implements(typeof(IScienceUtil), InjectionBindingScope.CROSS_CONTEXT)]
// ReSharper disable once ClassNeverInstantiated.Global
    public class KspScienceUtil : IScienceUtil
    {
        // Note: this doesn't use the game version of this method because that generates extra garbage appending
        // strings that we don't need
        public bool RequiredUsageInternalAvailable(IVessel vessel, IPart part, ExperimentUsageReqs reqs)
        {
            if (vessel == null) throw new ArgumentNullException("vessel");
            if (part == null) throw new ArgumentNullException("Part");

            if (reqs == ExperimentUsageReqs.Never) return false;

            if (reqs.IsFlagSet(ExperimentUsageReqs.VesselControl) && !vessel.IsControllable)
                return false;


            if (reqs.IsFlagSet(ExperimentUsageReqs.CrewInVessel))
                if (vessel.EvaCapableCrew.All(pcm => pcm.type != ProtoCrewMember.KerbalType.Crew))
                    return false;

            if (reqs.IsFlagSet(ExperimentUsageReqs.CrewInPart))
                if (part.EvaCapableCrew.All(c => c.type != ProtoCrewMember.KerbalType.Crew))
                    return false;

            if (!reqs.IsFlagSet(ExperimentUsageReqs.ScientistCrew)) return true;

            if (reqs.IsFlagSet(ExperimentUsageReqs.CrewInPart))
            {
                if (part.EvaCapableCrew.All(c => c.experienceTrait.With(t => t.TypeName) != "Scientist"))
                    return false;
            } 
            else
            {
                if (reqs.IsFlagSet(ExperimentUsageReqs.CrewInVessel))
                    return false;
                if (vessel.EvaCapableCrew.All(c => c.experienceTrait.With(t => t.TypeName) != "Scientist"))
                    return false;
            }

            return true;
        }
    }
}
