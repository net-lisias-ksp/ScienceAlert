using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.Annotations;
using ScienceAlert.Game;

namespace ScienceAlert.Providers
{
    public class VesselScienceExperimentModuleProvider : IScienceExperimentModuleProvider
    {
        private readonly IVesselProvider _vessel;

        public VesselScienceExperimentModuleProvider([NotNull] IVesselProvider vessel)
        {
            if (vessel == null) throw new ArgumentNullException("vessel");
            _vessel = vessel;
        }


        public IEnumerable<ModuleScienceExperiment> Get()
        {
            var vessel = _vessel.Get();

            return !vessel.Any() ? Enumerable.Empty<ModuleScienceExperiment>() : vessel.Single().FindPartModulesImplementing<ModuleScienceExperiment>();
        }
    }
}
