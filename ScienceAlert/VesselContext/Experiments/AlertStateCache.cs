using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using ScienceAlert.UI;

namespace ScienceAlert.VesselContext.Experiments
{
    class AlertStateCache : IAlertStateCache
    {
        private readonly ExperimentIdentifierProvider _identifierProvider;
        private readonly Dictionary<IExperimentIdentifier, ExperimentAlertStatus> _statuses;

        public AlertStateCache([NotNull] ReadOnlyCollection<ScienceExperiment> experiments,
            [NotNull] ExperimentIdentifierProvider identifierProvider)
        {
            _identifierProvider = identifierProvider;
            if (experiments == null) throw new ArgumentNullException("experiments");
            if (identifierProvider == null) throw new ArgumentNullException("identifierProvider");

            _statuses = experiments.ToDictionary(se => (IExperimentIdentifier)new KspExperimentIdentifier(se), se => ExperimentAlertStatus.None, new ExperimentIdentifierComparer());
        }


        public ExperimentAlertStatus GetStatus([NotNull] IExperimentIdentifier identifier)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");

            return _statuses[identifier];
        }


        public void OnAlertStatusChange(SensorStatusChange sensorChange, AlertStatusChange alertChange)
        {
            _statuses[_identifierProvider.Get(sensorChange.CurrentState.Experiment)] = alertChange.CurrentStatus;
        }
    }
}
