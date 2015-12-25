using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScienceAlert.Game
{
    public class KspVessel : IVessel
    {
        public event Callback Modified = delegate { };

        private readonly IGameFactory _factory;
        private readonly Vessel _vessel;

        private ReadOnlyCollection<ProtoCrewMember> _evaCapableCrew = new ReadOnlyCollection<ProtoCrewMember>(new ProtoCrewMember[] {}.ToList());

        private ReadOnlyCollection<IModuleScienceExperiment> _scienceModules =
            new ReadOnlyCollection<IModuleScienceExperiment>(Enumerable.Empty<IModuleScienceExperiment>().ToList());

        
        public KspVessel(
            IGameFactory factory, 
            Vessel vessel)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (vessel == null) throw new ArgumentNullException("data");

            _factory = factory;
            _vessel = vessel;
        }


        public override int GetHashCode()
        {
            return _vessel.GetHashCode();
        }


        public override bool Equals(object obj)
        {
            if (!(obj is KspVessel)) return false;

            var other = obj as KspVessel;

            return ReferenceEquals(_vessel, other._vessel);
        }


        public void OnVesselModified(IVessel data)
        {
            if (!data.Equals(this))
            {
                Log.Debug("Modified vessel " + data.VesselName + " does not match ours of " + VesselName);
                return;
            }
            Log.Debug("Updating KspVessel due to matching modified event");

            Rescan();
        }

        

        public void Rescan()
        {
            ScanForCrew();
            ScanForScienceExperimentModules();

            Modified();
        }


        private void ScanForCrew()
        {
            _evaCapableCrew = new ReadOnlyCollection<ProtoCrewMember>(
                _vessel.Parts.SelectMany(p => p.protoModuleCrew)
                    .Where(pcm => pcm.type != ProtoCrewMember.KerbalType.Tourist)
                    .ToList());
        }


        private void ScanForScienceExperimentModules()
        {
            _scienceModules = new ReadOnlyCollection<IModuleScienceExperiment>(
                _vessel.FindPartModulesImplementing<ModuleScienceExperiment>()
                .Select(mse => _factory.Create(mse)).ToList());
        }


        public ReadOnlyCollection<ProtoCrewMember> EvaCapableCrew
        {
            get { return _evaCapableCrew; }
        }

        public ReadOnlyCollection<IModuleScienceExperiment> ScienceExperimentModules
        {
            get { return _scienceModules; }
        }

        public bool IsControllable
        {
            get { return _vessel.IsControllable; }
        }

        public string VesselName
        {
            get { return _vessel.vesselName; }
        }

        public Vessel.Situations VesselSituation
        {
            get { return _vessel.situation; }
        }

        public ExperimentSituations ExperimentSituation
        {
            get { return ScienceUtil.GetExperimentSituation(_vessel); }
        }

        public CelestialBody Body
        {
            get { return _vessel.mainBody; }
        }
    }
}
