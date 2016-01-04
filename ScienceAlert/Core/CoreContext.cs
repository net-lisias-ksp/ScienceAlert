using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.FileSystem;
using ReeperCommon.FileSystem.Providers;
using ReeperCommon.Logging;
using ReeperCommon.Repositories;
using ReeperCommon.Serialization;
using ScienceAlert.Core.Gui;
using ScienceAlert.Gui;
using ScienceAlert.VesselContext;
using ScienceAlert.VesselContext.Experiments.Rules;
using strange.extensions.context.api;
using UnityEngine;

namespace ScienceAlert.Core
{
    public class CoreContext : SignalContext
    {
        private const string ExperimentRuleConfigNodeName = "SA_EXPERIMENT_RULE";

        public CoreContext(MonoBehaviour view)
            : base(view, ContextStartupFlags.MANUAL_MAPPING | ContextStartupFlags.MANUAL_LAUNCH)
        {
        }


        protected override void mapBindings()
        {
            base.mapBindings();

            MapCrossContextBindings();
            SetupCommandBindings();

            injectionBinder.GetInstance<ILog>().Normal("ScienceAlert is operating normally");
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

            injectionBinder.Bind<IFile>().ToValue(us).CrossContext();
            injectionBinder.Bind<IDirectory>().ToValue(us.Directory).CrossContext();
            injectionBinder.Bind<IDirectory>()
                .ToValue(injectionBinder.GetInstance<IFileSystemFactory>().GameData)
                .ToName(CoreKeys.GameData)
                .CrossContext();
            injectionBinder.Bind<Assembly>().ToValue(assembly).CrossContext();

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
                .ToValue(contextView as GameObject)
                .ToName(CoreKeys.CoreContextView)
                .CrossContext();

            injectionBinder.Bind<RuleDefinitionFactory>().ToSingleton().CrossContext();

            ConfigureScienceAlert();
            ConfigureResourceRepository();
            ConfigureSerializer();
            ConfigureExperiments();

            injectionBinder.Bind<SignalCriticalShutdown>().ToSingleton().CrossContext();

            injectionBinder.Bind<SignalVesselChanged>().ToSingleton();
            injectionBinder.Bind<SignalVesselModified>().ToSingleton();
            injectionBinder.Bind<SignalActiveVesselModified>().ToSingleton();
            injectionBinder.Bind<SignalVesselDestroyed>().ToSingleton();
            injectionBinder.Bind<SignalActiveVesselDestroyed>().ToSingleton();
            injectionBinder.Bind<SignalGameSceneLoadRequested>().ToSingleton();
            injectionBinder.Bind<SignalApplicationQuit>().ToSingleton();

            injectionBinder.Bind<SignalSharedConfigurationSaving>().ToSingleton().CrossContext();
        }


        private void SetupCommandBindings()
        {
            commandBinder.Bind<SignalScenarioModuleLoad>()
                .InSequence()
                .To<CommandCreateAppLauncherView>()
                .To<CommandCreateActiveVesselContextBootstrapper>() // because we'll definitely have missed the initial OnVesselChanged by now
                .Once();


            commandBinder.Bind<SignalScenarioModuleSave>()
                .To<CommandSaveSharedConfiguration>();


            commandBinder.Bind<SignalStart>()
                .InSequence()
                .To<CommandLoadSharedConfiguration>()
                .To<CommandCompileExperimentRulesets>()
                .To<CommandConfigureGuiSkinsAndTextures>()
                .To<CommandConfigureGameEvents>()
                .Once();


            commandBinder.Bind<SignalContextDestruction>()
                .InSequence()
                .To<CommandSaveSharedConfiguration>()
                .Once();

            injectionBinder.Bind<SignalCriticalShutdown>()
                .To<CommandCriticalShutdown>();

            commandBinder.Bind<SignalVesselDestroyed>()
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
                UnityEngine.Object.Destroy(contextView as GameObject);
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
                .ToValue(ScenarioRunner.fetch.GetComponent<ScienceAlert>().IfNull(
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

            injectionBinder.Bind<IResourceRepository>().ToValue(
                new ResourceRepositoryComposite(
                    // search GameDatabase first
                    //   note: GameDatabase expects extensionless urls
                    new ResourceIdentifierAdapter(StripExtensionFromId, new ResourceFromGameDatabase()),

                    // then look at physical file system. These work on a list of items cached
                    // by GameDatabase rather than working directly with the disk (unless a resource 
                    // is accessed from here, of course)
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
            var experiments =
                ResearchAndDevelopment.GetExperimentIDs().Select(ResearchAndDevelopment.GetExperiment);
            var ruleConfigs = GameDatabase.Instance.GetConfigNodes(ExperimentRuleConfigNodeName);

            injectionBinder.Bind<IEnumerable<ScienceExperiment>>().ToValue(experiments).CrossContext();

            foreach (var exp in experiments)
            {
                if (injectionBinder.GetBinding<ScienceExperiment>(exp.id).ToMaybe().Any())
                    throw new DuplicateScienceExperimentException(exp);

                injectionBinder.Bind<ScienceExperiment>().ToValue(exp).ToName(exp.id).CrossContext();
            }


            injectionBinder.Bind<IEnumerable<ConfigNode>>()
                .ToValue(ruleConfigs)
                .ToName(CoreKeys.ExperimentRuleConfigs)
                .CrossContext();
        }


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
