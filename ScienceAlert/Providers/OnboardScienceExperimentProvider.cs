using System;
using System.Collections.Generic;
using System.Linq;
using ScienceAlert.Annotations;

namespace ScienceAlert.Providers
{
    public class OnboardScienceExperimentProvider : IScienceExperimentProvider
    {
        private readonly IScienceExperimentProvider _experimentProvider;
        private readonly IScienceExperimentModuleProvider _scienceModuleProvider;

        public OnboardScienceExperimentProvider(
            [NotNull] IScienceExperimentProvider experimentProvider,
            [NotNull] IScienceExperimentModuleProvider scienceModuleProvider)
        {
            if (experimentProvider == null) throw new ArgumentNullException("experimentProvider");
            if (scienceModuleProvider == null) throw new ArgumentNullException("scienceModuleProvider");

            _experimentProvider = experimentProvider;
            _scienceModuleProvider = scienceModuleProvider;
        }


        public IEnumerable<ScienceExperiment> Get()
        {
            var modules = _scienceModuleProvider.Get();

            return _experimentProvider.Get()
                .Where(exp => modules.Any(mse => mse.experimentID == exp.id));
        }
    }
}
