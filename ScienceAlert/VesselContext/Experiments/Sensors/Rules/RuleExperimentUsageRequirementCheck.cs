using System;
using System.Linq;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments.Sensors.Rules
{
    ///
    /// Makes sure any internal usage requirements are met
    /// 
    // ReSharper disable once UnusedMember.Global
    public class RuleExperimentUsageRequirementCheck : ScienceExperimentModuleTracker, ISensorRule
    {
        protected readonly IScienceUtil ScienceUtil;
        private readonly Func<IModuleScienceExperiment, bool> _isModuleAvailable;  // keep things lean and mean by allocating the delegate upfront

        public RuleExperimentUsageRequirementCheck(IScienceUtil scienceUtil, ScienceExperiment experiment, IVessel vessel)
            : base(experiment, vessel)
        {
            if (scienceUtil == null) throw new ArgumentNullException("scienceUtil");

            _isModuleAvailable = IsModuleAvailable;
            ScienceUtil = scienceUtil;
        }

        public virtual bool Passes()
        {
            return ExperimentModules.Any(_isModuleAvailable);
        }

        private bool IsModuleAvailable(IModuleScienceExperiment mse)
        {
            return ScienceUtil.RequiredUsageInternalAvailable(Vessel, mse.Part, mse.InternalUsageRequirements);
        }
    }
}
