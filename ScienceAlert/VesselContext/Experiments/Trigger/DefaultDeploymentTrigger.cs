using System;
using ReeperCommon.Logging;
using strange.extensions.promise.api;

namespace ScienceAlert.VesselContext.Experiments.Trigger
{
    // ReSharper disable once UnusedMember.Global
    class DefaultDeploymentTrigger : ExperimentTrigger
    {
        public DefaultDeploymentTrigger(ScienceExperiment experiment) : base(experiment)
        {
        }

        public override IPromise Deploy()
        {
            Log.Warning("This is where the experiment would be deployed");

            throw new NotImplementedException();
        }

        public override bool Busy
        {
            get { throw new NotImplementedException(); }
        }
    }
}


//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using ReeperCommon.Containers;
//using strange.extensions.injector.api;
//using strange.extensions.promise.api;
//using strange.extensions.promise.impl;
//using ScienceAlert.Game;

//namespace ScienceAlert.VesselContext.Experiments.Trigger
//{
//    /// <summary>
//    /// This is actually much simpler than it looks; it will deploy the first available ModuleScienceExperiment and return once 
//    /// any related animation is complete (like mystery goo sliding open, materials bay doors opening, etc)
//    /// </summary>
//    public class DefaultDeploymentTrigger : ExperimentTrigger
//    {
//        private readonly IScienceUtil _scienceUtil;
//        private readonly ICoroutineRunner _coroutineRunner;
//        private readonly IPromise _promise = new Promise();

//        public DefaultDeploymentTrigger(IScienceUtil scienceUtil, ICoroutineRunner coroutineRunner)
//        {
//            if (scienceUtil == null) throw new ArgumentNullException("scienceUtil");
//            if (coroutineRunner == null) throw new ArgumentNullException("coroutineRunner");
//            _scienceUtil = scienceUtil;
//            _coroutineRunner = coroutineRunner;
//        }

//        protected virtual Maybe<IModuleScienceExperiment> GetSuitableModule(IVessel vessel, ScienceExperiment experiment)
//        {
//            if (vessel == null) throw new ArgumentNullException("vessel");
//            if (experiment == null) throw new ArgumentNullException("experiment");

//            return vessel.ScienceExperimentModules
//                .Where(mse => mse.ExperimentID == experiment.id)
//                .Where(mse => mse.CanBeDeployed)
//                .FirstOrDefault(mse => _scienceUtil.RequiredUsageInternalAvailable(vessel, mse.Part, mse.InternalUsageRequirements))
//                .ToMaybe();
//        }


//        public IPromise Deploy(IVessel vessel, ScienceExperiment experiment)
//        {
//            if (Busy)
//                throw new InvalidOperationException("Currently waiting on a previous deployment to complete");

//            var moduleToDeploy = GetSuitableModule(vessel, experiment);

//            if (!moduleToDeploy.Any())
//                throw new ArgumentException(experiment.id + " cannot be deployed");

//            _coroutineRunner.StartCoroutine(Deploy(moduleToDeploy.Value));

//            return _promise;
//        }


//        private IEnumerator Deploy(IModuleScienceExperiment selectedModule)
//        {
//            selectedModule.Deploy();

//            if (selectedModule.FxIndices.Any())
//                CreateAnimationCallbacks(selectedModule);

//            yield return WaitForAnimationCallbacksToFinish();
//        }


//        private readonly List<IScalarModule> _expectedCallbacks = new List<IScalarModule>();

//        private void CreateAnimationCallbacks(IModuleScienceExperiment mse)
//        {
//            var modulesOnPart = mse.Part.Modules;

//            var modulesThatSendAnimationCallbacks = mse.FxIndices.Value
//                .Select(moduleIdx => modulesOnPart[moduleIdx])
//                .OfType<IScalarModule>()
//                .ToList();

//            foreach (var module in modulesThatSendAnimationCallbacks)
//                module.OnStop.Add(CreateCallback(module));
//        }


//        private EventData<float>.OnEvent CreateCallback(IScalarModule fromModule)
//        {
//            return f => _expectedCallbacks.Remove(fromModule);
//        }


//        private IEnumerator WaitForAnimationCallbacksToFinish()
//        {
//            while (_expectedCallbacks.Any())
//                yield return 0;
//        }

//        public bool Busy
//        {
//            get { return _expectedCallbacks.Any(); }
//        }

//        private class DefaultDeploymentTriggerBuilder :
//            IConfigNodeObjectBuilder<IDeploymentTrigger>
//        {
//            private readonly IInjectionBinder _binder;
//            private readonly ITemporaryBindingFactory _temporaryBindingFactory;

//            public DefaultDeploymentTriggerBuilder(IInjectionBinder binder, ITemporaryBindingFactory temporaryBindingFactory)
//            {
//                if (binder == null) throw new ArgumentNullException("binder");
//                if (temporaryBindingFactory == null) throw new ArgumentNullException("temporaryBindingFactory");
//                _binder = binder;
//                _temporaryBindingFactory = temporaryBindingFactory;
//            }


//            public IDeploymentTrigger Build(IConfigNodeObjectBuilder<IDeploymentTrigger> builder, ConfigNode config)
//            {
//                return Build(config);
//            }

//            public IDeploymentTrigger Build(ConfigNode config)
//            {
//                if (!CanHandle(config))
//                    throw new ArgumentException("This builder can't handle supplied ConfigNode", "config");

//                using (
//                    var binding = _temporaryBindingFactory.Create(_binder,
//                        typeof(DefaultDeploymentTrigger)))
//                    return (DefaultDeploymentTrigger)binding.GetInstance();
//            }


//            public bool CanHandle(ConfigNode config)
//            {
//                if (config == null) throw new ArgumentNullException("config");

//                var configName = config.name.ToUpperInvariant();

//                return typeof(DefaultDeploymentTrigger).FullName == configName ||
//                       typeof(DefaultDeploymentTrigger).Name == configName;
//            }
//        }
//    }
//}