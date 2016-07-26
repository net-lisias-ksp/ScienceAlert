using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI.OptionsWindow;

namespace ScienceAlert.VesselContext.Gui
{
    class OptionsWindowMediator : Mediator
    {
        [Inject] public LocalConfiguration Configuration { get; set; }

        [Inject] public OptionsWindowView View { get; set; }
        [Inject] public SignalContextIsBeingDestroyed ContextDestroyed { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            // View signals
            View.CloseButtonClicked.AddListener(OnCloseButtonClicked);

            // other signals
            ContextDestroyed.AddOnce(OnContextDestroyed);


            CreateOptionEntriesForConfiguration();
        }


        public override void OnRemove()
        {
            View.CloseButtonClicked.RemoveListener(OnCloseButtonClicked);

            base.OnRemove();
        }


        private void CreateOptionEntriesForConfiguration()
        {
            foreach (var setting in Configuration.Where(c => c.Experiment.HasValue))
                View.AddEntry(
                    new OptionDisplayStatus(
                        new KspExperimentIdentifier(setting.Experiment.Value),
                        setting.Experiment.Value.experimentTitle,
                        setting.AlertsEnabled,
                        setting.StopWarpOnAlert,
                        setting.AlertOnRecoverable,
                        setting.AlertOnTransmittable,
                        setting.AlertOnLabValue,
                        setting.SoundOnAlert,
                        setting.AnimationOnAlert,
                        setting.SubjectResearchThreshold,
                        setting.IgnoreThreshold));
        }


        private void OnCloseButtonClicked()
        {
            View.gameObject.SetActive(false);
        }


        private void OnContextDestroyed()
        {
            Log.Verbose(GetType().Name + " destroying view");
            Destroy(gameObject);
        }
    }
}
