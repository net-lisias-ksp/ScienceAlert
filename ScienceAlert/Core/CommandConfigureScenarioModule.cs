using System;
using System.Linq;
using ReeperCommon.Containers;
using strange.extensions.command.impl;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandConfigureScenarioModule : Command
    {
        private readonly SignalScenarioModuleLoad _saveSignal;
        private readonly SignalScenarioModuleLoad _loadSignal;

        public CommandConfigureScenarioModule(
            SignalScenarioModuleLoad saveSignal,
            SignalScenarioModuleLoad loadSignal)
        {
            if (saveSignal == null) throw new ArgumentNullException("saveSignal");
            if (loadSignal == null) throw new ArgumentNullException("loadSignal");

            _saveSignal = saveSignal;
            _loadSignal = loadSignal;
        }


        public override void Execute()
        {
            var scienceAlert = GetScienceAlert();

            if (!scienceAlert.Any())
                throw new ScenarioModuleNotFoundException("ScienceAlert");

            var sa = scienceAlert.Single();

            sa.SaveSignal.AddListener(node => _saveSignal.Dispatch(node));
            sa.LoadSignal.AddListener(node => _loadSignal.Dispatch(node));

            injectionBinder.Bind<ScienceAlert>().Bind<ScenarioModule>().To(sa).CrossContext();
        }


        private static Maybe<ScienceAlert> GetScienceAlert()
        {
            return ScenarioRunner.fetch.GetComponent<ScienceAlert>().ToMaybe();
        }
    }
}
