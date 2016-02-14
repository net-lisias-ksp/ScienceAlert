using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.Core;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext
{
    // These rules will be instanstiated using the vessel context; if their isn't a local binding, they'll be created using
    // the cross context which won't have many local-specific (Vessel, signals, etc) bindings defined
// ReSharper disable once ClassNeverInstantiated.Global
    class CommandBindExperimentRuleTypes : Command
    {
        private readonly IEnumerable<Type> _ruleTypes;

        public CommandBindExperimentRuleTypes([Name(CoreKeys.ExperimentRuleTypes)] IEnumerable<Type> ruleTypes)
        {
            if (ruleTypes == null) throw new ArgumentNullException("ruleTypes");
            _ruleTypes = ruleTypes;
        }

        public override void Execute()
        {
            Log.Verbose("Binding " + _ruleTypes.Count() + " rule types");

            foreach (var rt in _ruleTypes)
                if (injectionBinder.GetBinding(rt) == null)
                {
                    Log.Debug("Binding " + rt.Name);
                    injectionBinder.Bind(rt).To(rt);
                }
        }
    }
}
