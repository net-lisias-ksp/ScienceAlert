using System.Collections.Generic;
using System.Linq;
using ReeperCommon;

namespace ScienceAlert.Experiments.Observers
{
    /// <summary>
    /// A special observer that uses the existence of crew, not
    /// an available ModuleScienceExperiment, to determine whether
    /// the experiment is possible
    /// </summary>
    class RequiresCrew : ExperimentObserver
    {
        protected List<Part> crewableParts = new List<Part>();

        public RequiresCrew(Experiments.Data.ScienceDataCache cache, ProfileData.ExperimentSettings settings, BiomeFilter filter, ScanInterface scanInterface, string expid)
            : base(cache, settings, filter, scanInterface, expid)
        {
            this.requireControllable = false;
        }

        /// <summary>
        /// Note: ScienceAlert will look out for vessel changes for
        /// us and call Rescan() as necessary
        /// </summary>
        public override void Rescan()
        {
            base.Rescan();

            crewableParts.Clear();

            if (FlightGlobals.ActiveVessel == null)
            {
                Log.Debug("EvaReportObserver: active vessel null; observer will not function");
                return;
            }

            // cache any part that can hold crew, so we don't have to
            // wastefully go through the entire vessel part tree
            // when updating status
            FlightGlobals.ActiveVessel.parts.ForEach(p =>
            {
                if (p.CrewCapacity > 0) crewableParts.Add(p);
            });

        }


        public override bool IsReadyOnboard
        {
            get
            {
                foreach (var crewable in crewableParts)
                    if (crewable.protoModuleCrew.Count > 0)
                        return true;
                return false;
            }
        }
    }
}
