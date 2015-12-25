using System;
using ReeperCommon.Containers;

namespace ScienceAlert.Game
{
    public class KspModuleScienceExperiment : IModuleScienceExperiment
    {
        private readonly IGameFactory _factory;
        private readonly ModuleScienceExperiment _mse;
        private readonly Lazy<IPart> _part;

        public KspModuleScienceExperiment(IGameFactory factory, ModuleScienceExperiment mse)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (mse == null) throw new ArgumentNullException("mse");

            _factory = factory;
            _mse = mse;
            _part = new Lazy<IPart>(() => _factory.Create(_mse.part));
        }


        public IPart part
        {
            get { return _part.Value; }
        }

        public bool Deployed
        {
            get { return _mse.Deployed; }
        }

        public string experimentID
        {
            get { return _mse.experimentID; }
        }

        public ExperimentUsageReqs InternalUsageRequirements
        {
            get { return (ExperimentUsageReqs) _mse.usageReqMaskInternal; }
        }
    }
}
