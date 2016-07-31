using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI;
using ScienceAlert.UI.ExperimentWindow;

namespace ScienceAlert.VesselContext.Gui
{
    class ExperimentWindowListItemMediator : Mediator
    {
        [Inject] public ExperimentListItemView View { get; set; }

        [Inject] public SignalSetTooltip TooltipSignal { get; set; }
        [Inject] public SignalDeployExperiment DeployExperiment { get; set; }

        [Inject] public ReadOnlyCollection<ScienceExperiment> Experiments { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            View.ChangeTooltip.AddListener(OnTooltipChanged);
            View.Deploy.AddListener(OnDeployButtonClicked);
        }


        public override void OnRemove()
        {
            View.ChangeTooltip.RemoveListener(OnTooltipChanged);
            View.Deploy.RemoveListener(OnDeployButtonClicked);
            base.OnRemove();
        }


        private void OnTooltipChanged(ExperimentListItemView.Indicator indicator)
        {
            TooltipSignal.Dispatch(View.Identifier, indicator);
        }


        private void OnDeployButtonClicked()
        {
            Experiments.FirstOrDefault(exp => View.Identifier.Equals(exp.id))
                .Do(exp => DeployExperiment.Dispatch(exp))
                .IfNull(() =>
                {
                    throw new ArgumentException("'" + View.Identifier.Return(i => i.ToString(), "<not set>") + "' is an unrecognized experiment identifier");
                });
        }
    }
}
