using System;

namespace ScienceAlert.Game
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class KspFactory : IGameFactory
    {
        private readonly SignalVesselModified _modifiedSignal;

        public KspFactory(SignalVesselModified modifiedSignal)
        {
            if (modifiedSignal == null) throw new ArgumentNullException("modifiedSignal");
            _modifiedSignal = modifiedSignal;
        }


        public IVessel Create(Vessel vessel)
        {
            if (vessel == null) throw new ArgumentNullException("vessel");

            var v = new KspVessel(this, vessel);

            _modifiedSignal.AddListener(v.OnVesselModified);

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

            return new KspPart(part);
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
