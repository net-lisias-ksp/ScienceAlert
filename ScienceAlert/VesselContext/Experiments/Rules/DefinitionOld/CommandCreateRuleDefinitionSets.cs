//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Text;
//using ReeperCommon.Containers;
//using ReeperCommon.Extensions;
//using ScienceAlert.VesselContext.Experiments.Rules;
//using strange.extensions.command.impl;

//namespace ScienceAlert.Core
//{
//    public class CommandCreateRuleDefinitionSets : Command
//    {


//        private readonly IEnumerable<ScienceExperiment> _experiments;
//        private readonly RuleDefinitionSetFactory _setFactory;

//        public CommandCreateRuleDefinitionSets(IEnumerable<ScienceExperiment> experiments, RuleDefinitionSetFactory setFactory)
//        {
//            if (experiments == null) throw new ArgumentNullException("experiments");
//            if (setFactory == null) throw new ArgumentNullException("setFactory");
//            _experiments = experiments;
//            _setFactory = setFactory;
//        }


//        private static Maybe<string> GetExperimentIdFromRuleConfig(ConfigNode ruleConfig)
//        {
//            return ruleConfig.GetValueEx(ExperimentRuleExperimentId, false);
//        }


//        private IDictionary<string, ConfigNode> GetRuleDefinitionSetConfigs()
//        {
//            var acceptedConfigs = new Dictionary<string, ConfigNode>();

//            foreach (var ruleConfig in GameDatabase.Instance.GetConfigs(ExperimentRuleConfigNodeName))
//            {
//                var experimentId = GetExperimentIdFromRuleConfig(ruleConfig.config);

//                if (!experimentId.Any())
//                {
//                    Log.Error(ExperimentRuleConfigNodeName + " at " + ruleConfig.url + " does not contain " +
//                              ExperimentRuleExperimentId);
//                    continue;
//                }

//                if (acceptedConfigs.ContainsKey(experimentId.Value))
//                {
//                    Log.Warning("Duplicate rule definition sets for " + experimentId.Value + " at " + ruleConfig.url);
//                    continue;
//                }

//                acceptedConfigs.Add(experimentId.Value, ruleConfig.config);
//            }

//            return acceptedConfigs;
//        }


//        public override void Execute()
//        {
//            var configSets = GetRuleDefinitionSetConfigs();

//            var ruleDefinitionSets = _experiments.Select(experiment =>
//            {
//                try
//                {
//                    var ruleDefinitionSetConfig = Maybe<ConfigNode>.None;
//                    ConfigNode config;

//                    if (configSets.TryGetValue(experiment.id, out config))
//                        ruleDefinitionSetConfig = config.ToMaybe();

//                    return _setFactory.Create(experiment, ruleDefinitionSetConfig);
//                }
//                catch (Exception)
//                {
//                    Log.Error("Exception while creating rule definition set for " + experiment.id);
//                    throw;
//                }
//            }).ToList();

//            Bind(ruleDefinitionSets);
//        }


//        private void Bind(List<RuleDefinitionSet> sets)
//        {
//            injectionBinder.Bind<IEnumerable<RuleDefinitionSet>>()
//                .Bind<List<RuleDefinitionSet>>()
//                .ToValue(sets)
//                .CrossContext();
//        }
//    }
//}
