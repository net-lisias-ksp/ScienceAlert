using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ScienceAlert.UI;

namespace ScienceAlert.VesselContext
{
    /// <summary>
    /// To avoid generating garbage 
    /// </summary>
    public class ExperimentIdentifierProvider
    {
        private readonly Dictionary<string, IExperimentIdentifier> _identifiers =
            new Dictionary<string, IExperimentIdentifier>();
 
        public IExperimentIdentifier Get([NotNull] ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            IExperimentIdentifier result;

            return _identifiers.TryGetValue(experiment.id, out result) ? result : Create(experiment.id);
        }


        private IExperimentIdentifier Create(string experimentId)
        {
            var identifier = new KspExperimentIdentifier(ResearchAndDevelopment.GetExperiment(experimentId));
            _identifiers.Add(experimentId, identifier);

            return identifier;
        }
    }
}
