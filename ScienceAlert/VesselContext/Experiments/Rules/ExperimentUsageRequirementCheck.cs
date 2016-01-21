using System;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
// ReSharper disable once UnusedMember.Global
    public class ExperimentUsageRequirementCheck : RuleUsesRelatedScienceModuleBase, IExperimentRule
    {
        private readonly IScienceUtil _scienceUtil;

        public ExperimentUsageRequirementCheck(
            ScienceExperiment experiment, 
            IVessel vessel,
            IScienceUtil scienceUtil) : base(experiment, vessel)
        {
            if (scienceUtil == null) throw new ArgumentNullException("scienceUtil");
            _scienceUtil = scienceUtil;
        }

        public bool Get()
        {
// ReSharper disable once LoopCanBeConvertedToQuery
// ReSharper disable once ForCanBeConvertedToForeach
            for (int moduleIndex = 0; moduleIndex < ExperimentModules.Count; ++moduleIndex)
            {
                var module = ExperimentModules[moduleIndex];

                if (module.Deployed)
                    continue;

                if (_scienceUtil.RequiredUsageInternalAvailable(Vessel, module.Part,
                    ExperimentModules[moduleIndex].InternalUsageRequirements))
                    return true;
            }

            return false;
        }
    }
}
