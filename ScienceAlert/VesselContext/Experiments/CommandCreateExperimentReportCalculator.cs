using System;
using ReeperCommon.Containers;
using ScienceAlert.Game;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext.Experiments
{
    public class CommandCreateExperimentReportCalculator : Command
    {
        private readonly IVessel _activeVessel;

        public CommandCreateExperimentReportCalculator(IVessel activeVessel)
        {
            if (activeVessel == null) throw new ArgumentNullException("activeVessel");
            _activeVessel = activeVessel;
        }

        public override void Execute()
        {
            injectionBinder.Bind<ExperimentReportValueCalculator>()
                .ToSingleton()
                .With(binding => injectionBinder.GetInstance<ExperimentReportValueCalculator>())
                .Do(calc => _activeVessel.Modified += calc.OnActiveVesselModified);
        }
    }
}
