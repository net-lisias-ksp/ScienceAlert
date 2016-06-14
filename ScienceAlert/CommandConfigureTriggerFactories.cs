//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using ReeperCommon.Logging;
//using strange.extensions.injector.api;
//using ScienceAlert.VesselContext.Experiments.Sensors.Trigger;

//namespace ScienceAlert
//{
//    sealed class CommandConfigureTriggerFactories :
//        CommandConfigureObjectFromConfigNodeBuilders<ITriggerFactory>
//    {
//        public CommandConfigureTriggerFactories(ITemporaryBindingFactory temporaryBinder) : base(temporaryBinder)
//        {
//        }


//        protected override ReadOnlyCollection<Type> GetBuilderTypes()
//        {
//            return base.GetBuilderTypes().Union(CreateGenericBuilderTypes()).ToList().AsReadOnly();
//        }


//        protected override void BindBuildersToCrossContext(ReadOnlyCollection<ITriggerFactory> builders)
//        {
//            base.BindBuildersToCrossContext(builders);

//            var composite = new CompositeTriggerFactory(builders
//                .Cast<IConfigNodeObjectBuilder<ExperimentTrigger, ITriggerFactory, IInjectionBinder, ITemporaryBindingFactory>>()
//                .ToList());

//            injectionBinder.Bind<ITriggerFactory>().To(composite).CrossContext();
//        }


//        // Every ExperimentTrigger will have a default builder that will try to meet any dependencies using the vessel's context
//        // If there are any custom ITriggerBuilders, they will get preference
//        private IEnumerable<Type> CreateGenericBuilderTypes()
//        {
//            var triggerTypes = GetAllTypesThatImplement<ExperimentTrigger>().ToList()
//                .Where(IsConstructable)
//                .ToList();

//            triggerTypes.ForEach(rt => Log.Debug(rt.FullName + " will have a generic builder created"));

//            return triggerTypes.Select(tExperimentRule => typeof (GenericTriggerFactory<>).MakeGenericType(tExperimentRule));
//        }
//    }
//}