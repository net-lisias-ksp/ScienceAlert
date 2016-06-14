using System;
using System.Linq;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    // ReSharper disable once UnusedMember.Global
    public class RuleExperimentUsageRequirementCheck : ScienceExperimentModuleTracker, ISensorRule
    {
        private readonly IScienceUtil _scienceUtil;
        private readonly Func<IModuleScienceExperiment, bool> _isModuleAvailable;  // keep things lean and mean by allocating the delegate upfront

        public RuleExperimentUsageRequirementCheck(IScienceUtil scienceUtil, ScienceExperiment experiment, IVessel vessel)
            : base(experiment, vessel)
        {
            if (scienceUtil == null) throw new ArgumentNullException("scienceUtil");

            _isModuleAvailable = IsModuleAvailable;
            _scienceUtil = scienceUtil;
        }

        public bool Passes()
        {
            return ExperimentModules.Any(_isModuleAvailable);
        }

        private bool IsModuleAvailable(IModuleScienceExperiment mse)
        {
            return _scienceUtil.RequiredUsageInternalAvailable(Vessel, mse.Part, mse.InternalUsageRequirements);
        }
    }
}
