using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperCommon.Repositories;
using ReeperKSP.Extensions;
using ReeperKSP.FileSystem;
using ReeperKSP.FileSystem.Providers;
using ReeperKSP.Repositories;
using ReeperKSP.Serialization;
using strange.extensions.context.api;
using ScienceAlert.Core.Gui;
using ScienceAlert.Game;
using ScienceAlert.Gui;
using ScienceAlert.SensorDefinitions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScienceAlert.Core
{
    public class CoreContext : SignalContext
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

            injectionBinder.Bind<SignalActiveVesselModified>().ToSingleton();
            injectionBinder.Bind<SignalActiveVesselDestroyed>().ToSingleton();
            injectionBinder.Bind<SignalActiveVesselChanged>().ToSingleton();

            injectionBinder.Bind<SignalCrewOnEva>().ToSingleton();
            injectionBinder.Bind<SignalCrewTransferred>().ToSingleton();

            injectionBinder.Bind<SignalGameSceneLoadRequested>().ToSingleton();
            injectionBinder.Bind<SignalScienceReceived>().ToSingleton();

            injectionBinder.Bind<SignalApplicationQuit>().ToSingleton();
            injectionBinder.Bind<SignalGameTick>().ToSingleton();

            injectionBinder.Bind<ITemporaryBindingFactory>().To<TemporaryBindingFactory>().ToSingleton();


            injectionBinder.Bind<IGameFactory>().To<KspFactory>().ToSingleton();

            injectionBinder.Bind<IGameDatabase>().To<KspGameDatabase>().ToSingleton();

            MapCrossContextBindings();
            SetupCommandBindings();

            injectionBinder.GetInstance<ILog>().Verbose("ScienceAlert CoreCore created successfully");
        }


        private void MapCrossContextBindings()
        {
            Log.Instance = 
#if DEBUG
                new DebugLog("ScienceAlert");
#else 
                new NormalLog("ScienceAlert");
#endif

            injectionBinder.Bind<ILog>().To(Log.Instance).CrossContext();

            injectionBinder.Bind<SignalScenarioModuleLoad>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalScenarioModuleSave>().ToSingleton().CrossContext();


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
                .ToName(CoreKeys.GameData)
                .CrossContext();
            injectionBinder.Bind<Assembly>().To(assembly).CrossContext();

            injectionBinder
                .Bind<IGuiConfiguration>()
                .Bind<ISharedConfigurationFilePathProvider>()
                .Bind<SharedConfiguration>()
                .To<SharedConfiguration>().ToSingleton().CrossContext();


            injectionBinder.Bind<ICoroutineRunner>()
                .To<CoroutineRunner>()
                .ToSingleton()
                .CrossContext();

            injectionBinder.Bind<GameObject>()
                .To(contextView as GameObject)
                .ToName(CoreKeys.CoreContextView)
                .CrossContext();

            injectionBinder.Bind<float>()
                .ToValue(HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier)
                .ToName(CoreKeys.CareerScienceGainMultiplier)
                .CrossContext();

            injectionBinder.Bind<ICelestialBody>().To(new KspCelestialBody(FlightGlobals.GetHomeBody())).ToName(CoreKeys.HomeWorld).CrossContext();

            injectionBinder
                .Bind<IQueryScienceValue>()
                .Bind<IResearchAndDevelopment>()
                .To<KspResearchAndDevelopment>().ToSingleton().CrossContext();


            ConfigureScienceAlert();
            ConfigureResourceRepository();
            ConfigureSerializer();
            ConfigureExperiments();


            injectionBinder.Bind<SignalCriticalShutdown>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalSharedConfigurationSaving>().ToSingleton().CrossContext();
        }


        private void SetupCommandBindings()
        {
            commandBinder.Bind<SignalScenarioModuleLoad>()
                .InSequence()
                .To<CommandConfigureSensorDefinitionBuilder>()
                .To<CommandCreateSensorDefinitions>()
                .To<CommandConfigureRuleFactories>()
                .To<CommandConfigureTriggerFactories>()
                .To<CommandLoadSharedConfiguration>()
                .To<CommandConfigureGuiSkinsAndTextures>()
                .To<CommandConfigureGameEvents>()
                .To<CommandCreateAppLauncherView>()
                .To<CommandCreateActiveVesselContextBootstrapper>() // because we'll definitely have missed the initial OnVesselChanged by now
                .Once();


            commandBinder.Bind<SignalScenarioModuleSave>()
                .To<CommandSaveSharedConfiguration>();


            commandBinder.Bind<SignalStart>()
                .InSequence()
                .Once();


            commandBinder.Bind<SignalContextDestruction>()
                .InSequence()
                .To<CommandSaveSharedConfiguration>()
                .Once();


            injectionBinder.Bind<SignalCriticalShutdown>()
                .To<CommandCriticalShutdown>();


            commandBinder.Bind<SignalActiveVesselDestroyed>()
                .To<CommandDestroyActiveVesselContext>();


            commandBinder.Bind<SignalActiveVesselChanged>()
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
            var serializableFieldQuery = new GetObjectFieldsIncludingBaseTypes();

            var standardSerializerSelector =
                new SerializerSelector(
                    new CompositeSurrogateProvider(
                        new GenericSurrogateProvider(surrogateQuery, supportedTypeQuery, assembliesToScanForSurrogates),
                        new SurrogateProvider(surrogateQuery, supportedTypeQuery, assembliesToScanForSurrogates)));


            var preferNativeSelector = new PreferNativeSerializer(standardSerializerSelector);

            var selectorOrder = new CompositeSerializerSelector(
                preferNativeSelector,          // always prefer native serializer first 
                standardSerializerSelector);   // otherwise, find any surrogate


            var includePersistentFieldsSelector = new SerializerSelectorDecorator(
                selectorOrder,
                s => Maybe<IConfigNodeItemSerializer>.With(new FieldSerializer(s, serializableFieldQuery)));

            injectionBinder.Bind<IConfigNodeSerializer>()
                .To(new ConfigNodeSerializer(includePersistentFieldsSelector))
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

            injectionBinder.Bind<List<ScienceExperiment>>().To(experiments).CrossContext();
            injectionBinder.Bind<ReadOnlyCollection<ScienceExperiment>>().To(experiments.AsReadOnly()).CrossContext();
        }


        // Why is this bit off on its down? If the player has duplicate ScienceDefs and we're the first one to try
        // and access R&D (initializing it), the dictionary it tries to create internally will throw an exception.
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
                    Debug.LogWarning(getExperimentId(problem.config).Value + " definition found at " +
                                     problem.url);

                throw;
            }
        }



        //private IConfigNodeObjectBuilder<IRuleFactory> CreateRuleFactoryBuilder(ITemporaryBindingFactory temporaryBinder)
        //{
        //    // Get all Types that implement IExperimentRule and create builders which will construct factories to produce them 
        //    // I know that sounds insane but this way we can do all the processing of ConfigNodes up front and use those
        //    // factory types to bundle all the info necessary to actually create the rule in one place (data such as: ConfigNode
        //    // to deserialize the concrete type that implements IExperimentRule)
        //    //
        //    // All of these rule factory builders can then be hidden behind this composite interface and it'll
        //    // look like we have a magic IRuleFactory builder that can handle all kinds of nodes and types

        //    var allTypes = AssemblyLoader.loadedAssemblies.SelectMany(la => la.assembly.GetTypes()).ToList();

        //    // these types will be wrapped by RuleFactoryBuilders that will build a factory for the type
        //    var experimentRuleTypes =
        //        allTypes
        //            .Where(
        //                t =>
        //                    !t.IsAbstract && !t.IsInterface &&
        //                    t.GetInterfaces().Any(it => typeof(IExperimentRule) == it)).ToList();

            

        //    // these are concrete builder types for specific type(s). We'll try to handle factory construction requests
        //    // through these first
        //    var explicitFactoryBuilders =
        //        allTypes.Where(
        //            t =>
        //                !t.IsAbstract &&
        //                t.GetInterfaces().Any(it => it == typeof(IConfigNodeObjectBuilder<IRuleFactory>) && !it.IsGenericTypeDefinition && !it.IsAbstract))
        //                .Where(t => temporaryBinder.CanCreate(t))
        //                .ToList();

        //    experimentRuleTypes.ForEach(t => Log.Debug("IExperimentRule type: " + t.FullName));
        //    explicitFactoryBuilders.ForEach(bt => Log.Debug("IRuleFactory builder type: " + bt.FullName));

        //    var builder = new CompositeObjectFromConfigNodeFactory<IRuleFactory>(
        //        // we'll prefer explicit builders first
        //        explicitFactoryBuilders.Select(explicitFactoryType =>
        //        {
        //            using (var binding = temporaryBinder.Create(explicitFactoryType))
        //            {
        //                return (IConfigNodeObjectBuilder<IRuleFactory>) binding.GetInstance();
        //            }
        //        })

        //        // then we'll use our generic rule factory builder to handle any unprocessed requests after that
        //        .Union(
        //            experimentRuleTypes
        //                .Select(concreteRuleType => typeof(RuleFactoryBuilder<>).MakeGenericType(concreteRuleType))
        //                .Select(
        //                    builderType =>
        //                    {
        //                        using (var binding = temporaryBinder.Create(builderType))
        //                        {
        //                            return (IConfigNodeObjectBuilder<IRuleFactory>)binding.GetInstance();
        //                        }
        //                    }))

        //        // add ability to AND rules together
        //        .Union(new [] { (IConfigNodeObjectBuilder<IRuleFactory>)(new CompositeAndRule.CompositeAndRuleFactoryBuilder()) }));

            
        //    return builder;
        //}



        public void SignalDestruction()
        {
            try
            {
                injectionBinder.GetInstance<SignalContextDestruction>().Do(ds => ds.Dispatch());
            }
            catch (Exception e)
            {
                // just swallow it, something went wrong with binding configuration and there's nothing to be done
                Log.Error("Error while signalling destruction: " + e);
            }
        }
    }
}
