using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using ReeperCommon.Logging;
using strange.extensions.command.impl;
using ScienceAlert.Game;
using ScienceAlert.VesselContext.Experiments;

namespace ScienceAlert.Core
{
    class CommandLoadDefaultRuleConfigs : Command
    {
        private readonly IGameDatabase _gameDatabase;
        private readonly ICriticalShutdownEvent _shutdown;
        private const string DefaultConditionNodeName = "SA_DEFAULT_CONDITION_RULE";
        private const string DefaultAvailabilityNodeName = "SA_DEFAULT_AVAILABILITY_RULE";
        private const string DefaultOnboardNodeName = "SA_DEFAULT_ONBOARD_RULE";

        public CommandLoadDefaultRuleConfigs([NotNull] IGameDatabase gameDatabase,
            [NotNull] ICriticalShutdownEvent shutdown)
        {
            if (gameDatabase == null) throw new ArgumentNullException("gameDatabase");
            if (shutdown == null) throw new ArgumentNullException("shutdown");
            _gameDatabase = gameDatabase;
            _shutdown = shutdown;
        }

        public override void Execute()
        {
            try
            {
                var defaultOnboardRule = GetDefaultRuleConfigFromGameDatabase(DefaultOnboardNodeName);
                var defaultAvailabilityRule = GetDefaultRuleConfigFromGameDatabase(DefaultAvailabilityNodeName);
                var defaultConditionRule = GetDefaultRuleConfigFromGameDatabase(DefaultConditionNodeName);

                injectionBinder.Bind<ConfigNode>()
                    .To(defaultOnboardRule)
                    .ToName(CrossContextKeys.DefaultOnboardRule)
                    .CrossContext();

                injectionBinder.Bind<ConfigNode>()
                    .To(defaultAvailabilityRule)
                    .ToName(CrossContextKeys.DefaultAvailabilityRule)
                    .CrossContext();

                injectionBinder.Bind<ConfigNode>()
                    .To(defaultConditionRule)
                    .ToName(CrossContextKeys.DefaultConditionRule)
                    .CrossContext();

                Log.Verbose("Default rule configs loaded successfully");
            }
            catch (Exception e)
            {
                Log.Error("Failed to create default rule configs: " + e);
                Fail();
                _shutdown.Dispatch(); // we definitely need those defaults since most sensors will probably be created from them, so quit now
            }
        }


        private ConfigNode GetDefaultRuleConfigFromGameDatabase(string nodeName)
        {
            var configs = _gameDatabase.GetConfigs(nodeName);

            var firstOrOnlyConfigFound = configs.FirstOrDefault(u => u.Config.HasData);

            if (configs.Length > 1)
            {
                Log.Warning("Multiple configs matching " + nodeName + " found:");
                foreach (var url in configs)
                    Log.Warning("Duplicate " + nodeName + " found at " + url.Url);

                if (firstOrOnlyConfigFound != null)
                    Log.Warning("The one at " + firstOrOnlyConfigFound.Url + " will be used");
            }
            else if (configs.Length == 0)
                throw new DefaultConfigNotFoundException(nodeName);

            if (firstOrOnlyConfigFound == null)
                throw new ArgumentException("No ConfigNodes matching " + nodeName + " contain data");

            // we'll assume the user meant all of the rules specified since multiple rules are mentioned
            // without telling us how to interpret them as a set...
            if (firstOrOnlyConfigFound.Config.CountNodes > 1)
            {
                var composite = new ConfigNode("ALL");
                firstOrOnlyConfigFound.Config.nodes.Cast<ConfigNode>().ToList().ForEach(rule => composite.AddNode(rule));

                return composite;
            }
            
            if (firstOrOnlyConfigFound.Config.CountNodes == 0)
                throw new FormatException(nodeName + " is in an incorrect format. At least one rule must be specified.");

            return firstOrOnlyConfigFound.Config.nodes[0]; // because root node is SA_DEFAULT_*
        }
    }
}
