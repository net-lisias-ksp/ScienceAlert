using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ScienceAlert.UI;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentStateCache
    {
        private readonly Dictionary<ScienceExperiment, ExperimentSensorState> _states =
            new Dictionary<ScienceExperiment, ExperimentSensorState>();
 
        public void UpdateState(ExperimentSensorState state)
        {
            if (_states.ContainsKey(state.Experiment))
                _states[state.Experiment] = state;
            else _states.Add(state.Experiment, state);
        }

        public ExperimentSensorState GetCachedState([NotNull] ScienceExperiment experiment)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");

            return !_states.ContainsKey(experiment) ? default(ExperimentSensorState) : _states[experiment];
        }


        public ExperimentSensorState GetCachedState([NotNull] IExperimentIdentifier identifier)
        {
            if (identifier == null) throw new ArgumentNullException("identifier");

            foreach (var kvp in _states)
                if (identifier.Equals(kvp.Key.id))
                    return kvp.Value;

            return default(ExperimentSensorState);
        }
    }
}
