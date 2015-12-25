using System;

namespace ScienceAlert.Game
{
// ReSharper disable once ClassNeverInstantiated.Global
    [Implements(typeof(IGameFactory))]
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

            return new KspModuleScienceExperiment(this, mse);
        }


        public IPart Create(Part part)
        {
            if (part == null) throw new ArgumentNullException("part");

            return new KspPart(part);
        }
    }
}
