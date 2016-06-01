using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Logging;

namespace ScienceAlert.Game
{
    public class KspVessel :  IVessel
    {
        public event Callback Rescanned = delegate { };

        private readonly IGameFactory _factory;
        private readonly Vessel _vessel;

        private ReadOnlyCollection<ProtoCrewMember> _evaCapableCrew = new ReadOnlyCollection<ProtoCrewMember>(new ProtoCrewMember[] {}.ToList());

        private ReadOnlyCollection<IModuleScienceExperiment> _scienceModules =
            new ReadOnlyCollection<IModuleScienceExperiment>(Enumerable.Empty<IModuleScienceExperiment>().ToList());

        private ReadOnlyCollection<IScienceDataContainer> _scienceContainers =
            new ReadOnlyCollection<IScienceDataContainer>(Enumerable.Empty<IScienceDataContainer>().ToList());

        private ReadOnlyCollection<IScienceDataTransmitter> _scienceTransmitters =
            new ReadOnlyCollection<IScienceDataTransmitter>(Enumerable.Empty<IScienceDataTransmitter>().ToList());

        private ReadOnlyCollection<IScienceLab> _scienceLabs = new ReadOnlyCollection<IScienceLab>(Enumerable.Empty<IScienceLab>().ToList());
 
        public KspVessel(
            IGameFactory factory, 
            Vessel vessel)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (vessel == null) throw new ArgumentNullException("data");

            _factory = factory;
            _vessel = vessel;

            Log.Warning("KspVessel created");
        }

        ~KspVessel()
        {
            Log.Warning("KspVessel finalized");
        }


        public void OnVesselModified()
        {
            Rescan();
        }


        public void OnCrewOnEva(GameEvents.FromToAction<IPart, IPart> data)
        {
            Log.Debug("Updating KspVessel crew due to matching OnCrewOnEva event");
            Rescan();
        }


        public void OnCrewTransferred(GameEvents.HostedFromToAction<ProtoCrewMember, IPart> data)
        {
            Log.Debug("Updating KspVessel crew due to matching OnCrewTransferred event");
            Rescan();
        }


        

        public void Rescan()
        {
            ScanForCrew();
            ScanForScienceExperimentModules();
            ScanForScienceContainers();
            ScanForScienceTransmitters();
            ScanForScienceLabs();

            Rescanned();
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

            Log.Debug("KspVessel: found " + _scienceModules.Count + " ModuleScienceExperiment modules");
        }


        private void ScanForScienceContainers()
        {
            _scienceContainers = new ReadOnlyCollection<IScienceDataContainer>(
                _vessel.FindPartModulesImplementing<IScienceDataContainer>().ToList());

            Log.Debug("KspVessel: found " + _scienceContainers.Count + " IScienceDataContainers");
        }


        private void ScanForScienceTransmitters()
        {
            _scienceTransmitters = new ReadOnlyCollection<IScienceDataTransmitter>(
                _vessel.FindPartModulesImplementing<IScienceDataTransmitter>().ToList());

            Log.Debug("KspVessel: found " + _scienceTransmitters.Count + " IScienceDataTransmitters");
        }


        private void ScanForScienceLabs()
        {
            _scienceLabs = new ReadOnlyCollection<IScienceLab>(
                _vessel.FindPartModulesImplementing<ModuleScienceLab>()
                    .Select(l => _factory.Create(l))
                    .ToList());

            Log.Debug("KspVessel: found " + _scienceLabs.Count + " ModuleScienceLab modules");
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


        public ReadOnlyCollection<IScienceDataContainer> Containers
        {
            get { return _scienceContainers; }
        }

        public ReadOnlyCollection<IScienceDataTransmitter> Transmitters
        {
            get { return _scienceTransmitters; }
        }

        public ReadOnlyCollection<IScienceLab> Labs
        {
            get { return _scienceLabs; }
        }

        public ICelestialBody OrbitingBody
        {
            get { return _factory.Create(_vessel.mainBody); }
        }

        public ExperimentSituations ExperimentSituation
        {
            get { return ScienceUtil.GetExperimentSituation(_vessel); }
        }

        public double Latitude
        {
            get { return _vessel.latitude; }
        }

        public double Longitude
        {
            get { return _vessel.longitude; }
        }

        public bool SplashedDown
        {
            get { return _vessel.Splashed; }
        }

        public bool Landed
        {
            get { return _vessel.Landed; }
        }

        public string Biome
        {
            get
            {
                return string.IsNullOrEmpty(_vessel.landedAt)
                    ? ScienceUtil.GetExperimentBiome(_vessel.mainBody, Latitude, Longitude)
                    : Vessel.GetLandedAtString(_vessel.landedAt);
            }
        }

        public bool isEVA
        {
            get { return _vessel.isEVA; }
        } 
    }
}
