using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperCommon.Repositories;
using ReeperCommon.Utilities;
using ReeperKSP.Extensions;
using ReeperKSP.FileSystem;
using ReeperKSP.FileSystem.Providers;
using ReeperKSP.Repositories;
using ReeperKSP.Serialization;
using strange.extensions.context.api;
using strange.extensions.injector.api;
using ScienceAlert.Core.Gui;
using ScienceAlert.Game;
using ScienceAlert.VesselContext;
using ScienceAlert.VesselContext.Experiments.Sensors;
using ScienceAlert.VesselContext.Experiments.Sensors.Rules;
using ScienceAlert.VesselContext.Experiments.Triggers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScienceAlert.Core
{
    class CoreContext : SignalContext
    {
        private const string ExperimentDefinitionConfigNodeName = "SCIENCE_EXPERIMENT";
        private const string ExperimentDefinitionIdValueName = "id";
        

        public CoreContext(MonoBehaviour view)
            : base(view, ContextStartupFlags.MANUAL_MAPPING | ContextStartupFlags.MANUAL_LAUNCH)
        {
        }


        protected override void mapBindings()
        {
            base.mapBindings();

            injectionBinder.Bind<SignalVesselModified>().ToSingleton();
            injectionBinder.Bind<SignalActiveVesselModified>().ToSingleton();
            injectionBinder.Bind<SignalActiveVesselDestroyed>().ToSingleton();
            injectionBinder.Bind<SignalVesselChanged>().ToSingleton();

            injectionBinder.Bind<SignalActiveVesselCrewModified>().ToSingleton();
            injectionBinder.Bind<SignalDominantBodyChanged>().ToSingleton();

            injectionBinder.Bind<SignalGameSceneLoadRequested>().ToSingleton();
            injectionBinder.Bind<SignalScienceReceived>().ToSingleton();

            injectionBinder.Bind<SignalApplicationQuit>().ToSingleton();
            injectionBinder.Bind<SignalGameTick>().ToSingleton();

            injectionBinder.Bind<ITemporaryBindingFactory>().To<TemporaryBindingFactory>().ToSingleton();




            MapCrossContextBindings();

            // Have one or more dependencies on cross context bindings

            injectionBinder.Bind<ConfigNode>()
                .To(injectionBinder.GetInstance<SharedConfiguration>().SoundConfig)
                .ToName(CoreContextKeys.SoundConfig);


            SetupCommandBindings();

            injectionBinder.GetInstance<ILog>().Verbose("ScienceAlert Core created successfully");
        }


        private void MapCrossContextBindings()
        {
            Log.Instance = 
#if DEBUG
                new DebugLog("ScienceAlert");
#else 
                new NormalLog("ScienceAlert");
#endif
            var shutdownRunner = new CriticalShutdownEventRunner();

            injectionBinder.Bind<ILog>().To(Log.Instance).CrossContext();
            injectionBinder.Bind<ICriticalShutdownEvent>().To(shutdownRunner).CrossContext();

            injectionBinder.Bind<SignalScenarioModuleLoad>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalScenarioModuleSave>().ToSingleton().CrossContext();

            injectionBinder.Bind<string>().To("SA_EXPERIMENT_CONFIGURATION").ToName(CoreContextKeys.ExperimentConfigurationNodeName).CrossContext();

            injectionBinder.Bind<IUrlDirProvider>().To<KSPGameDataUrlDirProvider>().ToSingleton().CrossContext();
            injectionBinder.Bind<IUrlDir>().To(new KSPUrlDir(injectionBinder.GetInstance<IUrlDirProvider>().Get())).CrossContext();
            injectionBinder.Bind<IFileSystemFactory>().To<KSPFileSystemFactory>().ToSingleton().CrossContext();

            var assembly = Assembly.GetExecutingAssembly();
            var us = GetAssemblyLocation(assembly, injectionBinder.GetInstance<IFileSystemFactory>()).SingleOrDefault();

            if (us == null)
                throw new AssemblyNotFoundException(assembly);

            injectionBinder.Bind<IFile>().To(us).CrossContext();
            injectionBinder.Bind<IDirectory>().To(us.Directory).CrossContext();
            injectionBinder.Bind<IDirectory>()
                .To(injectionBinder.GetInstance<IFileSystemFactory>().GameData)
                .ToName(CrossContextKeys.GameData)
                .CrossContext();
            injectionBinder.Bind<Assembly>().To(assembly).CrossContext();

            injectionBinder.Bind<ExperimentIdentifierProvider>().ToSingleton().CrossContext();
  
            injectionBinder
                .Bind<ISharedConfigurationFilePathProvider>()
                .Bind<SharedConfiguration>()
                .To<SharedConfiguration>().ToSingleton().CrossContext();


            injectionBinder.Bind<CoroutineHoster>().To(CoroutineHoster.Instance).CrossContext();


            injectionBinder.Bind<IGameFactory>().To<KspFactory>().ToSingleton().CrossContext();
            injectionBinder.Bind<IGameDatabase>().To<KspGameDatabase>().ToSingleton().CrossContext();

            injectionBinder.Bind<GameObject>()
                .To(contextView as GameObject)
                .ToName(CrossContextKeys.CoreContextView)
                .CrossContext();

            injectionBinder.Bind<float>()
                .ToValue(HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier)
                .ToName(CrossContextKeys.CareerScienceGainMultiplier)
                .CrossContext();

            injectionBinder.Bind<ICelestialBody>().To(new KspCelestialBody(FlightGlobals.GetHomeBody())).ToName(CrossContextKeys.HomeWorld).CrossContext();

            var kspRnd = new KspResearchAndDevelopment(injectionBinder.GetInstance<IGameFactory>());
            injectionBinder.GetInstance<SignalScienceReceived>().Do(signal => signal.AddListener(kspRnd.OnScienceReceived));
            kspRnd.RefreshKnownSubjects();

            injectionBinder
                .Bind<IQueryScienceValue>()
                .Bind<IResearchAndDevelopment>()
                .Bind<IExistingScienceSubjectProvider>()
                    .To(kspRnd).CrossContext();

            mediationBinder.BindView<ApplicationLauncherView>().ToMediator<ApplicationLauncherMediator>();

            ConfigureScienceAlert();
            ConfigureResourceRepository();
            ConfigureSerializer();
            ConfigureRuleTypes();
            ConfigureExperiments();

            injectionBinder.Bind<SignalSharedConfigurationSaving>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalExperimentAlertChanged>().ToSingleton().CrossContext();

            injectionBinder.Bind<LocalConfiguration>().ToSingleton().CrossContext();
        }


        private void SetupCommandBindings()
        {
            commandBinder.Bind<SignalScenarioModuleLoad>()
                .InSequence()
                .To<CommandConfigureCriticalShutdown>()
                .To<CommandLoadSounds>()
                .To<CommandLoadDefaultRuleConfigs>()
                .To<CommandCreateBuilderComposite<
                    IObjectFromConfigNodeBuilder<ExperimentConfiguration, ConfigNode, IInjectionBinder>, 
                    CompositeObjectFromConfigNodeBuilder<ExperimentConfiguration, ConfigNode, IInjectionBinder>>
                    >()
                .To<CommandCreateBuilderComposite<
                        IObjectFromConfigNodeBuilder<ISensorRule, ConfigNode, IInjectionBinder>, 
                        CompositeObjectFromConfigNodeBuilder<ISensorRule, ConfigNode, IInjectionBinder>>
                    >()
                .To<CommandCreateBuilderComposite<
                        IObjectFromConfigNodeBuilder<IExperimentSensor, ExperimentConfiguration, IInjectionBinder>, 
                        CompositeObjectFromConfigNodeBuilder<IExperimentSensor, ExperimentConfiguration, IInjectionBinder>>
                    >()
                .To<CommandCreateBuilderComposite<
                        IObjectFromConfigNodeBuilder<IExperimentTrigger, ExperimentConfiguration, IInjectionBinder>, 
                        CompositeObjectFromConfigNodeBuilder<IExperimentTrigger, ExperimentConfiguration, IInjectionBinder>>
                    >()
                .To<CommandCreateExperimentConfigurations>()
                .To<CommandLoadLocalConfiguration>()
                .To<CommandLoadSharedConfiguration>()
                .To<CommandConfigureGameEvents>()
                .To<CommandCreateAppLauncherView>()
                .To<CommandCreateActiveVesselContextBootstrapper>() // because we'll definitely have missed the initial OnVesselChanged by now
                .Once();


            commandBinder.Bind<SignalScenarioModuleSave>()
                .To<CommandSaveLocalConfiguration>()
                .To<CommandSaveSharedConfiguration>();

            commandBinder.Bind<SignalStart>()
                .InSequence()
                .Once();


            commandBinder.Bind<SignalContextIsBeingDestroyed>()
                .InSequence()
                .To<CommandSaveSharedConfiguration>()
                .Once();


            commandBinder.Bind<SignalActiveVesselDestroyed>()
                .To<CommandDestroyActiveVesselContext>();


            commandBinder.Bind<SignalVesselChanged>()
                .InSequence()
                .To<CommandDestroyActiveVesselContext>()
                .To<CommandCreateActiveVesselContextBootstrapper>();
        }


        public override void Launch()
        {
            try
            {
                base.Launch();
                injectionBinder.GetInstance<SignalStart>().Dispatch();
            }
            catch (Exception e)
            {
                Log.Error("Exception while launching core context: " + e);
                Log.Error("ScienceAlert must shut down");
                Object.Destroy(contextView as GameObject);
#if !DEBUG
                Assembly.GetExecutingAssembly().DisablePlugin();
#endif
            }
        }


        private static Maybe<IFile> GetAssemblyLocation(Assembly assembly, IFileSystemFactory fsFactory)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (fsFactory == null) throw new ArgumentNullException("fsFactory");

            var results = AssemblyLoader.loadedAssemblies.Where(la => ReferenceEquals(la.assembly, assembly)).ToList();

            if (results.Count > 1) throw new InvalidOperationException("Multiple targets found in assembly loader");
            if (!results.Any()) return Maybe<IFile>.None;

            // oddly, the urls in AssemblyLoader don't specify the filename, only the directory
            var url = new KSPUrlIdentifier(results.First().url + Path.DirectorySeparatorChar + results.First().dllName);

            return fsFactory.GameData.File(url);
        }


        private static string StripExtensionFromId(string id)
        {
            if (!Path.HasExtension(id) || string.IsNullOrEmpty(id)) return id;

            var dir = Path.GetDirectoryName(id) ?? "";
            var woExt = Path.Combine(dir, Path.GetFileNameWithoutExtension(id)).Replace('\\', '/');

            return !string.IsNullOrEmpty(woExt) ? woExt : id;
        }


        private static string ConvertUrlToResourceName(string url)
        {
            var prepend =
#if DEBUG
 "ScienceAlert" // because AssemblyReloader mangles this
#else
                Assembly.GetExecutingAssembly().GetName().Name 
#endif
                + ".";

            if (!url.StartsWith(prepend))
                url = prepend + url;

            return url.Replace('/', '.').Replace('\\', '.');
        }


        private void ConfigureScienceAlert()
        {
            var saveSignal = injectionBinder.GetInstance<SignalScenarioModuleSave>();
            var loadSignal = injectionBinder.GetInstance<SignalScenarioModuleLoad>();

            injectionBinder
                .Bind<ScienceAlert>()
                .Bind<ScenarioModule>()
                .To(ScenarioRunner.fetch.GetComponent<ScienceAlert>().IfNull(
                    () => { throw new ScenarioModuleNotFoundException("ScienceAlert"); })
                    .Do(sa =>
                    {
                        sa.SaveSignal.AddListener(saveSignal.Dispatch);
                        sa.LoadSignal.AddListener(loadSignal.Dispatch);
                    })).CrossContext();
        }


        private void ConfigureResourceRepository()
        {
            var currentAssemblyResource = new ResourceFromEmbeddedResource(Assembly.GetExecutingAssembly());

            injectionBinder.Bind<IResourceRepository>().To(
                new ResourceRepositoryComposite(
                    // search GameDatabase first
                    //   note: GameDatabase expects extensionless urls
                    new ResourceIdentifierAdapter(StripExtensionFromId, new ResourceFromGameDatabase()),

                    // then look at physical file system. These work on a list of items cached
                    // by GameDatabase rather directly searching the disk
                    new ResourceFromDirectory(injectionBinder.GetInstance<IDirectory>()),

                    // finally search embedded resource
                    // we need to handle both cases of the url containing the extension or not...
                    new ResourceRepositoryComposite(
                        // don't strip extension
                        new ResourceIdentifierAdapter(ConvertUrlToResourceName, currentAssemblyResource),

                        // strip extension
                        new ResourceIdentifierAdapter(StripExtensionFromId,
                            new ResourceIdentifierAdapter(ConvertUrlToResourceName, currentAssemblyResource)),

                        // there's a potential third issue: if the incoming id doesn't contain an extension,
                        // we might not fully match any of the manifest resource names. We'll add a special adapter
                        // that will select the most-closely matching name if there's only one match
                        new ResourceIdentifierAdapter(ConvertUrlToResourceName,
                            new ResourceIdentifierAdapter(s =>
                            {
                                var resourceNames = injectionBinder.GetInstance<Assembly>().GetManifestResourceNames()
                                    .Where(name => name.StartsWith(s, StringComparison.InvariantCulture))
                                    .ToList();

                                return resourceNames.Count == 1 ? resourceNames.First() : s;
                            }, currentAssemblyResource)
                            )))).CrossContext();
        }


        private void ConfigureSerializer()
        {
            var assembliesToScanForSurrogates = new[] { typeof(IConfigNodeSerializer).Assembly, Assembly.GetExecutingAssembly() }
                .Distinct() // in release mode, ReeperCommon gets mashed into executing assembly
                .ToArray();

            var supportedTypeQuery = new GetSurrogateSupportedTypes();
            var surrogateQuery = new GetSerializationSurrogates(supportedTypeQuery);
            var serializableFieldQuery = new GetObjectFieldsIncludingBaseTypes(); // this allows private fields that would normally not be accessible to derived classes to be included

            // this selector will try to find a surrogate for the serialized type
            var standardSerializerSelector =
                new SerializerSelector(
                    new CompositeSurrogateProvider(
                        new GenericSurrogateProvider(surrogateQuery, supportedTypeQuery, assembliesToScanForSurrogates), // selects open generic surrogates (so a surrogate that supports List<T> can be written once rather than require a concrete one for every type)
                        new SurrogateProvider(surrogateQuery, supportedTypeQuery, assembliesToScanForSurrogates)));

            // this selector will look at the serialized type and if it implements its own serialization methods, it will use those first
            // even if there's a surrogate for it
            var preferNativeSelector = new PreferNativeSerializer(standardSerializerSelector);


            // here the two selectors are combined: use the inboard serialization methods if the type implements IReeperPersistent, otherwise
            // try to find a surrogate
            var selectorOrder = new CompositeSerializerSelector(
                preferNativeSelector,          // always prefer native serializer first 
                standardSerializerSelector);   // otherwise, find any surrogate


            // the above selectors try to choose a surrogate or native serializer for a given type, but we'll want to be
            // also/or be able to serialize fields as well
            var includePersistentFieldsSelector = new SerializerSelectorDecorator(
                selectorOrder,
                s => Maybe<IConfigNodeItemSerializer>.With(new FieldSerializer(s, serializableFieldQuery)));

            // finally, add yet another decorator which will call IPersistenceSave/Load on any type serialized
            var selectorWhichCallsPersistenceMethods = new SerializerSelectorDecorator(
                includePersistentFieldsSelector,
                s => Maybe<IConfigNodeItemSerializer>.With(new PersistenceMethodCaller(s)));

            injectionBinder.Bind<IConfigNodeSerializer>()
                .To(new ConfigNodeSerializer(selectorWhichCallsPersistenceMethods))
                .CrossContext();
        }


        private void ConfigureExperiments()
        {
            var experiments = GetScienceExperiments();

            foreach (var exp in experiments)
            {
                if (injectionBinder.GetBinding<ScienceExperiment>(exp.id).ToMaybe().Any())
                    throw new DuplicateScienceExperimentException(exp);

                injectionBinder.Bind<ScienceExperiment>().To(exp).ToName(exp.id).CrossContext();
            }

            injectionBinder.Bind<ReadOnlyCollection<ScienceExperiment>>().To(experiments.AsReadOnly()).CrossContext();
        }


        // If the player has duplicate ScienceDefs and we try to
        // access R&D, the dictionary it tries to create internally will throw an exception.
        // If we're looking for this, we can better inform the player and even determine which ConfigNode(s) at which
        // location(s) are the issue
// ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private List<ScienceExperiment> GetScienceExperiments()
        {
            Func<ConfigNode, Maybe<string>> getExperimentId =
                c => c.GetValueEx(ExperimentDefinitionIdValueName, false);

            try
            {
                var ids = ResearchAndDevelopment.GetExperimentIDs();

                if (!ids.Any())
                    throw new InvalidOperationException("No ScienceExperiment definitions found -- something is wrong in your install!");

                return ids.Select(ResearchAndDevelopment.GetExperiment).ToList();
            }
            catch (ArgumentException)
            {
                
                // identify the problem node(s)
                var problemConfigs = GameDatabase.Instance.GetConfigs(ExperimentDefinitionConfigNodeName)
                    .Where(urlConfig => getExperimentId(urlConfig.config).Any())
                    .GroupBy(urlConfig => getExperimentId(urlConfig.config).Value)
                    .Where(grp => grp.Count() > 1)
                    .SelectMany(k => k)
                    .OrderBy(j => getExperimentId(j.config).Value)
                    .ToList();

                Log.Error("Multiple " + ExperimentDefinitionConfigNodeName +
                          " for the same experiment found! You need to fix your installation.");

                foreach (var problem in problemConfigs)
                    Debug.LogWarning(getExperimentId(problem.config).Return(s => s, "<experimentID not defined>") + " definition found at " +
                                     problem.url);

                throw;
            }
        }


        private void ConfigureRuleTypes()
        {
            var sensorTypes = AssemblyLoader.loadedAssemblies.SelectMany(la => la.assembly.GetTypes())
                .Where(ty => ty.GetInterfaces().Any(i => i == typeof (ISensorRule)))
                .ToList()
                .AsReadOnly();

            injectionBinder.Bind<ReadOnlyCollection<Type>>()
                .To(sensorTypes)
                .ToName(CrossContextKeys.SensorRuleTypes)
                .CrossContext();
        }


        public void SignalDestruction()
        {
            try
            {
                injectionBinder.GetInstance<SignalContextIsBeingDestroyed>().Do(ds => ds.Dispatch());
            }
            catch (Exception e)
            {
                // just swallow it, something went wrong with binding configuration and there's nothing to be done
                Log.Error("Error while signaling destruction: " + e);
            }
        }
    }
}
