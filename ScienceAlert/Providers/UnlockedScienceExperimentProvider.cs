using System;
using System.Collections.Generic;
using System.Linq;
using ScienceAlert.Annotations;
using ScienceAlert.Game;

namespace ScienceAlert.Providers
{
    public class UnlockedScienceExperimentProvider : IScienceExperimentProvider
    {
        private readonly IResearchAndDevelopment _rnd;

        public UnlockedScienceExperimentProvider([NotNull] IResearchAndDevelopment rnd)
        {
            if (rnd == null) throw new ArgumentNullException("rnd");
            _rnd = rnd;
        }


        public IEnumerable<ScienceExperiment> Get()
        {
            return _rnd.Experiments
                .Where(exp => exp.IsUnlocked());
        }
    }
}
