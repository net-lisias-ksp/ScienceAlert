using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ReeperCommon.Logging;
using strange.extensions.injector.api;
using ScienceAlert.VesselContext.Experiments.Trigger;

namespace ScienceAlert
{
    class CommandConfigureTriggerBuilders :
        CommandConfigureObjectFromConfigNodeBuilders<ITriggerBuilder, ExperimentTrigger>
    {
        public CommandConfigureTriggerBuilders(ITemporaryBindingFactory temporaryBinder) : base(temporaryBinder)
        {
        }


        protected override ReadOnlyCollection<ITriggerBuilder> CreateBuilders(ReadOnlyCollection<Type> builderTypes)
        {
            return base.CreateBuilders(builderTypes).Union(CreateGenericBuilders()).ToList().AsReadOnly();
        }


        protected override void BindBuildersToCrossContext(ReadOnlyCollection<ITriggerBuilder> builders)
        {
            base.BindBuildersToCrossContext(builders);

            var composite = new CompositeTriggerBuilder(builders
                .Cast<IConfigNodeObjectBuilder<ExperimentTrigger, ITriggerBuilder, IInjectionBinder, ITemporaryBindingFactory>>()
                .ToList());

            injectionBinder.Bind<ITriggerBuilder>().To(composite).CrossContext();
        }


        private IEnumerable<ITriggerBuilder> CreateGenericBuilders()
        {
            var triggerTypes = GetAllTypesThatImplement<ExperimentTrigger>().ToList()
                .Where(t => t.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Any())
                .ToList();

            triggerTypes.ForEach(rt => Log.Debug(rt.FullName + " will have a generic builder created"));

            return triggerTypes
                .Select(tExperimentRule => typeof(GenericTriggerBuilder<>).MakeGenericType(tExperimentRule))
                .Select(tGenericTriggerBuilder =>
                {
                    using (var tempBinding = TemporaryBinder.Create(tGenericTriggerBuilder))
                        return (ITriggerBuilder)tempBinding.GetInstance();
                });
        }
    }
}