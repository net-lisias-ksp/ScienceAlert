using System;

namespace ScienceAlert.Game
{
    // Note to self: make sure this isn't in a cross context binding; it's meant to have the same lifetime as the
    // dependencies its given (this prevents us from having to explicitly unregister listeners)
// ReSharper disable once ClassNeverInstantiated.Global
    public class KspFactory : IGameFactory
    {
        private readonly SignalVesselModified _modifiedSignal;
        private readonly SignalCrewOnEva _evaSignal;
        private readonly SignalCrewTransferred _transferredSignal;

        public KspFactory(
            SignalVesselModified modifiedSignal,
            SignalCrewOnEva evaSignal,
            SignalCrewTransferred transferredSignal)
        {
            if (modifiedSignal == null) throw new ArgumentNullException("modifiedSignal");
            if (evaSignal == null) throw new ArgumentNullException("evaSignal");
            if (transferredSignal == null) throw new ArgumentNullException("transferredSignal");
            _modifiedSignal = modifiedSignal;
            _evaSignal = evaSignal;
            _transferredSignal = transferredSignal;
        }


        public IVessel Create(Vessel vessel)
        {
            if (vessel == null) throw new ArgumentNullException("vessel");

            var v = new KspVessel(this, vessel);

            _modifiedSignal.AddListener(v.OnVesselModified);
            _evaSignal.AddListener(v.OnCrewOnEva);
            _transferredSignal.AddListener(v.OnCrewTransferred);

            v.Rescan();

            return v;
        }


        public IModuleScienceExperiment Create(ModuleScienceExperiment mse)
        {
            if (mse == null) throw new ArgumentNullException("mse");

            return new KspScienceExperimentModule(this, mse);
        }


        public IPart Create(Part part)
        {
            if (part == null) throw new ArgumentNullException("Part");

            return new KspPart(part, Create(part.vessel));
        }


        public IScienceSubject Create(ScienceSubject subject)
        {
            if (subject == null) throw new ArgumentNullException("subject");

            return new KspScienceSubject(subject);
        }


        public ICelestialBody Create(CelestialBody body)
        {
            if (body == null) throw new ArgumentNullException("body");

            return new KspCelestialBody(body);
        }


        public IScienceLab Create(ModuleScienceLab lab)
        {
            if (lab == null) throw new ArgumentNullException("lab");

            return new KspScienceLabModule(lab);
        }


        public IUrlConfig Create(UrlDir.UrlConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");

            return new KspUrlConfig(config);
        }
    }
}
