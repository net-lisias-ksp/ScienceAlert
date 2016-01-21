﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using ScienceAlert.VesselContext.Experiments.Sensors.Queries;

namespace ScienceAlert.Game
{
    public class KspVessel :  IVessel
    {
        public event Callback Modified = delegate { };

        private readonly IGameFactory _factory;
        private readonly Vessel _vessel;

        private ReadOnlyCollection<ProtoCrewMember> _evaCapableCrew = new ReadOnlyCollection<ProtoCrewMember>(new ProtoCrewMember[] {}.ToList());

        private ReadOnlyCollection<IModuleScienceExperiment> _scienceModules =
            new ReadOnlyCollection<IModuleScienceExperiment>(Enumerable.Empty<IModuleScienceExperiment>().ToList());

        private ReadOnlyCollection<IScienceDataContainer> _scienceContainers =
            new ReadOnlyCollection<IScienceDataContainer>(Enumerable.Empty<IScienceDataContainer>().ToList()); 
        
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
            ScanForScienceContainers();

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

            Log.Debug("KspVessel: found " + _scienceModules.Count + " ModuleScienceExperiment modules");
        }


        private void ScanForScienceContainers()
        {
            _scienceContainers = new ReadOnlyCollection<IScienceDataContainer>(
                _vessel.FindPartModulesImplementing<IScienceDataContainer>().ToList());

            Log.Debug("KspVessel: found " + _scienceContainers.Count + " IScienceDataContainers");
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

        public string Biome
        {
            get
            {
                return string.IsNullOrEmpty(_vessel.landedAt)
                    ? ScienceUtil.GetExperimentBiome(_vessel.mainBody, Latitude, Longitude)
                    : _vessel.landedAt;
            }
        }
    }
}
