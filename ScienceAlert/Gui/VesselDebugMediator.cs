using System;
using System.ComponentModel;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperCommon.Serialization;
using strange.extensions.injector;
using strange.extensions.mediation.impl;

namespace ScienceAlert.Gui
{
    public class VesselDebugMediator : Mediator
    {
        private const string SaveNodeName = "VesselDebugWindow";

        private VesselDebugView _view;

        [Inject]
        public VesselDebugView View
        {
            get { return _view; }
            set { _view = value; }
        }


        [Inject] public SignalSaveGuiSettings SaveSignal { get; set; }
        [Inject] public SignalLoadGuiSettings LoadSignal { get; set; }

        [Inject] public IConfigNodeSerializer Serializer { get; set; }
        [Inject] public ILog Log { get; set; }

        [PostConstruct]
        public void AfterRegister()
        {
            print("VesselDebugMediator.AfterRegister");
            OnVesselModified(FlightGlobals.ActiveVessel);

            View.Visible = true;
        }


        public override void OnRegister()
        {
            print("VesselDebugMediator.OnRegister");

            base.OnRegister();

            View.Close.AddListener(OnClose);
            View.RefreshScienceData.AddListener(OnRefreshScienceData);

            SaveSignal.AddListener(OnSave);
            LoadSignal.AddListener(OnLoad);

            GameEvents.onVesselChange.Add(OnVesselChanged);
            GameEvents.onVesselWasModified.Add(OnVesselModified);

            View.Visible = true;
        }


        public override void OnRemove()
        {
            base.OnRemove();

            View.Close.RemoveListener(OnClose);
            View.RefreshScienceData.RemoveListener(OnRefreshScienceData);

            SaveSignal.RemoveListener(OnSave);
            LoadSignal.RemoveListener(OnLoad);

            GameEvents.onVesselChange.Remove(OnVesselChanged);
            GameEvents.onVesselWasModified.Remove(OnVesselModified);
        }


        private void OnClose()
        {
            View.Visible = false;
        }


        private void OnSave(ConfigNode node)
        {
            if (node == null) throw new ArgumentNullException("node");

            node.AddNode(Serializer.CreateConfigNodeFromObject(_view).Do(n => n.name = SaveNodeName));
        }


        private void OnLoad(ConfigNode node)
        {
            if (node == null) throw new ArgumentNullException("node");

            node.GetNode(SaveNodeName)
                .IfNull(() => Log.Warning("VesselDebug: no settings found for this window; defaults will be used"))
                .Do(n => Serializer.LoadObjectFromConfigNode(ref _view, n))
                .Do(n => Log.Debug("Loaded VesselDebug configuration"));
        }


// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            UpdateVesselLocationData();
        }


        private void OnVesselModified(Vessel data)
        {
            if (!IsActiveVessel(data)) return;

            RefreshVesselScienceData();
            RefreshVesselTransmitterCount();
        }


        private void OnVesselChanged(Vessel data)
        {
            if (!IsActiveVessel(data)) return;

            RefreshVesselScienceData();
            RefreshVesselTransmitterCount();
            UpdateVesselLocationData();
        }


        private void OnRefreshScienceData()
        {
            RefreshVesselScienceData();
        }


        private void UpdateVesselLocationData()
        {
            var vessel = FlightGlobals.ActiveVessel;

            if (vessel == null) return;

            View.SetVesselCoordinates(vessel.latitude, vessel.longitude);
            View.SetVesselBiome(ScienceUtil.GetExperimentBiome(vessel.mainBody, vessel.latitude, vessel.longitude));
        }



        private void RefreshVesselTransmitterCount()
        {
            View.SetVesselTransmitterCount(
                FlightGlobals.ActiveVessel.Return(
                    v => v.FindPartModulesImplementing<IScienceDataTransmitter>().Count(), 0));
        }


        private void RefreshVesselScienceData()
        {
            var containers = FlightGlobals.ActiveVessel.FindPartModulesImplementing<IScienceDataContainer>().ToList();

            View.SetVesselStorageContainerCount(containers.Count);
            View.SetVesselContainerSubjects(
                containers.SelectMany(can => can.GetData())
                    .OrderBy(d => d.subjectID)
                    .Select(d => d.subjectID + ": " + d.dataAmount + ", xmit " + d.transmitValue));
        }


        private static bool IsActiveVessel(Vessel vessel)
        {
            return ReferenceEquals(vessel, FlightGlobals.ActiveVessel);
        }
    }
}
