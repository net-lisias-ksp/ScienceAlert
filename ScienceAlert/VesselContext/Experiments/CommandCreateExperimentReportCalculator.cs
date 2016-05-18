using System;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments
{
    // ReSharper disable once ClassNeverInstantiated.Global
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
            Log.Verbose("Creating " + typeof (ExperimentReportValueCalculator).Name);

            injectionBinder
                .Bind<ExperimentReportValueCalculator>()
                .Bind<IExperimentReportValueCalculator>()
                .To<ExperimentReportValueCalculator>()
                .ToSingleton()
                .With(binding => injectionBinder.GetInstance<ExperimentReportValueCalculator>())
                .Do(calc => _activeVessel.Rescanned += calc.OnActiveVesselModified)
                .Do(calc => calc.OnActiveVesselModified());

            Log.Verbose("Created " + typeof (ExperimentReportValueCalculator).Name);
        }
    }
}
