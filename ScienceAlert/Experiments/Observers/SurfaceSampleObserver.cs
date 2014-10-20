using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Experiments.Observers
{
    /// <summary>
    /// This object works a bit like the EVA report observer: it checks if a particular experiment is available
    /// even though a ModuleScienceExperiment for that experiment doesn't exist. This way we can alert to experiments
    /// that are technically available since the player can EVA a kerbal at any time that does have the right
    /// experiment module
    /// </summary>
    internal class SurfaceSampleObserver : EvaReportObserver
    {
        public SurfaceSampleObserver(StorageCache cache, ProfileData.ExperimentSettings settings, BiomeFilter filter, ScanInterface scanInterface)
            : base(cache, settings, filter, scanInterface, "surfaceSample")
        {

        }

        public override bool IsReadyOnboard
        {
            get
            {
                if (FlightGlobals.ActiveVessel != null)
                {
                    if (FlightGlobals.ActiveVessel.isEVA)
                    {
                        return settings.AssumeOnboard || this.GetNextOnboardExperimentModule() != null;
                    }
                    else return Settings.Instance.CheckSurfaceSampleNotEva && base.IsReadyOnboard;

                }
                else return false;
            }
        }
    }
}
