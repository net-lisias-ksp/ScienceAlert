using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Events;
using ReeperCommon.Logging;
using ReeperKSP.Serialization;
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace ScienceAlert
{
    public class LocalConfiguration : IPersistenceLoad, IPersistenceSave
    {
        public class ExperimentSettings : IPersistenceLoad, IPersistenceSave
        {
            public enum Setting
            {
                EnableAlerts,
                StopWarpOnAlert,
                AlertOnRecoverable,
                AlertOnTransmittable,
                AlertOnLabValue,
                SoundOnAlert,
                AnimationOnAlert,
                ResearchThreshold,
                IgnoreThreshold
            }

            public readonly DisposableEvent<Setting> Changed = new DisposableEvent<Setting>();

            [ReeperPersistent] private string _experimentId = string.Empty;
            [ReeperPersistent] private bool _alertsEnabled = true;
            [ReeperPersistent] private bool _stopWarpOnAlert = true;
            [ReeperPersistent] private bool _alertOnRecoverable = true;
            [ReeperPersistent] private bool _alertOnTransmittable = true;
            [ReeperPersistent] private bool _alertOnLabValue = true;
            [ReeperPersistent] private bool _soundOnAlert = true;
            [ReeperPersistent] private bool _animationOnAlert = true;
            [ReeperPersistent] private float _subjectResearchThreshold = 0.5f;
            [ReeperPersistent] private float _ignoreThreshold = 0f;

            // ReSharper disable once UnusedMember.Global
            public ExperimentSettings()
            {
            }

            public ExperimentSettings(ScienceExperiment experiment)
            {
                _experimentId = experiment.id;
                Experiment = Maybe<ScienceExperiment>.With(experiment);
            }


            // it's possible for the experiment this entry is for no longer exists (mod was removed, experiment disabled by a patch, etc)
            public Maybe<ScienceExperiment> Experiment { get; private set; }

            public bool AlertsEnabled
            {
                get { return _alertsEnabled; }
                set { SetParameter(Setting.EnableAlerts, ref _alertsEnabled, value); }
            }

            public bool StopWarpOnAlert
            {
                get { return _stopWarpOnAlert; }
                set { SetParameter(Setting.StopWarpOnAlert, ref _stopWarpOnAlert, value); }
            }

            public bool AlertOnRecoverable
            {
                get { return _alertOnRecoverable; }
                set { SetParameter(Setting.AlertOnRecoverable, ref _alertOnRecoverable, value); }
            }

            public bool AlertOnTransmittable
            {
                get { return _alertOnTransmittable; }
                set { SetParameter(Setting.AlertOnTransmittable, ref _alertOnTransmittable, value); }
            }

            public bool AlertOnLabValue
            {
                get { return _alertOnLabValue; }
                set { SetParameter(Setting.AlertOnLabValue, ref _alertOnLabValue, value); }
            }

            public bool SoundOnAlert
            {
                get { return _soundOnAlert; }
                set { SetParameter(Setting.SoundOnAlert, ref _soundOnAlert, value); }
            }

            public bool AnimationOnAlert
            {
                get { return _animationOnAlert; }
                set { SetParameter(Setting.AnimationOnAlert, ref _animationOnAlert, value); }
            }

            public float SubjectResearchThreshold
            {
                get { return _subjectResearchThreshold; }
                set { SetParameter(Setting.ResearchThreshold, ref _subjectResearchThreshold, value); }
            }

            public float IgnoreThreshold
            {
                get { return _ignoreThreshold; }
                set { SetParameter(Setting.IgnoreThreshold, ref _ignoreThreshold, value); }
            }


            private void SetParameter<TParamType>(Setting which, ref TParamType existing, TParamType newValue)
                where TParamType : struct
            {
                var changed = existing.Equals(newValue);
                existing = newValue;

                if (changed) Changed.Fire(which);
            }

            public void PersistenceLoad()
            {
                Log.Warning("ExperimentSettings.PersistenceLoad");

                MatchExperimentIdToExperiment();
                FireChangedForAllParameters();
            }

            public void PersistenceSave()
            {
                Log.Warning("ExperimentSettings.PersistenceSave " + _experimentId);
            }


            private void MatchExperimentIdToExperiment()
            {
                Experiment = GetExperiment(_experimentId);
            }


            private void FireChangedForAllParameters()
            {
                foreach (var enumValue in Enum.GetValues(typeof(Setting)))
                    Changed.Fire((Setting)enumValue);
            }


            private static Maybe<ScienceExperiment> GetExperiment(string experimentId)
            {
                if (string.IsNullOrEmpty(experimentId)) return Maybe<ScienceExperiment>.None;

                return ResearchAndDevelopment.GetExperimentIDs().Select(ResearchAndDevelopment.GetExperiment)
                    .FirstOrDefault(exp => string.Equals(exp.id, experimentId))
                    .ToMaybe();
            }
        }

        // ReSharper disable once CollectionNeverUpdated.Global
        [Inject] public ReadOnlyCollection<ScienceExperiment> Experiments { get; set; }

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [ReeperPersistent] private List<ExperimentSettings> _experimentSettings = new List<ExperimentSettings>();

        public void PersistenceLoad()
        {
            Log.Warning("LocalConfiguration.PersistenceLoad");

            AddDefaultSettingsForExperimentsWithoutThem();
        }

        public void PersistenceSave()
        {
            Log.Warning("LocalConfiguration.PersistenceSave with " + _experimentSettings.Count + " settings");
            RemoveDuplicateExperimentSettings();
        }


        private void AddDefaultSettingsForExperimentsWithoutThem()
        {
            var experimentsWithSettings =
                _experimentSettings
                    .Where(s => s.Experiment.HasValue)
                    .Select(s => s.Experiment.Value.id)
                    .ToList();

            // these are experiments that exist but no settings exist for them yet; default values need to be created
            var missingExperiments = Experiments
                .ToList()
                .Where(se => !experimentsWithSettings.Contains(se.id));

            _experimentSettings.AddRange(missingExperiments
                .Select(se => new ExperimentSettings(se)));
        }


        // discard any duplicate experiment settings before saving, if we've somehow managed to get some. Be noisy about it
        // because something must have broken somewhere
        private void RemoveDuplicateExperimentSettings()
        {
            var knownIds = new HashSet<string>();

            for (int i = 0; i < _experimentSettings.Count;)
            {
                var item = _experimentSettings[i];

                if (!item.Experiment.HasValue)
                    continue;

                var experimentId = item.Experiment.Value.id;

                if (knownIds.Contains(item.Experiment.Value.id))
                {
                    _experimentSettings.RemoveAt(i);
                    Log.Warning("Duplicate experiment setting for " + item.Experiment.Value.id +
                                " was found and discarded");
                }
                else
                {
                    knownIds.Add(experimentId);
                    ++i;
                }
            }
        }
    }
}
