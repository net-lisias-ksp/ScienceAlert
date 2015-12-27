using System;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Serialization;
using strange.extensions.injector;
using strange.extensions.mediation.impl;

namespace ScienceAlert.VesselContext.Gui
{
    public class VesselDebugMediator : Mediator
    {
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
        [Inject(VesselContextKeys.VesselDebugViewConfig)] public ConfigNode Config { get; set; }


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
            OnVesselModified(FlightGlobals.ActiveVessel);
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


        private void OnSave()
        {
            try
            {
                Serializer.CreateConfigNodeFromObject(_view)
                    .Do(n =>
                    {
                        Config.ClearData();
                        n.CopyTo(Config);
                    });

                Log.Verbose("Successfully serialized VesselDebugView");
            }
            catch (Exception e)
            {
                Log.Error("Exception while serializing debug view: " + e);
            }
        }

        private void OnLoad()
        {
            try
            {
                Serializer.LoadObjectFromConfigNode(ref _view, Config);
                Log.Verbose("Successfully deserialized VesselDebugView");
            }
            catch (Exception e)
            {
                Log.Error("Exception while deserializing debug view: " + e);
            }
        }


// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            if (!View.Visible) return;

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
