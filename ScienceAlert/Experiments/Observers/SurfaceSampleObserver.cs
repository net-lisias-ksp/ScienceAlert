namespace ScienceAlert.Experiments.Observers
{
    /// <summary>
    /// This object works a bit like the EVA report observer: it checks if a particular experiment is available
    /// even though a ModuleScienceExperiment for that experiment doesn't exist. It's useful because the player
    /// technically has the capability to do this experiment at their fingertips already, just by going on EVA
    /// </summary>
    internal class SurfaceSampleObserver : EvaReportObserver
    {
        public SurfaceSampleObserver(Experiments.Data.ScienceDataCache cache, ProfileData.ExperimentSettings settings, BiomeFilter filter, ScanInterface scanInterface)
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
                        return this.GetNextOnboardExperimentModule() != null;
                    }
                    else return Settings.Instance.CheckSurfaceSampleNotEva && base.IsReadyOnboard;

                }
                else return false;
            }
        }
    }
}
