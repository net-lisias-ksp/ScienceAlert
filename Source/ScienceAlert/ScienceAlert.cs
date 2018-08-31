using System;
using System.Collections;
using System.Collections.Generic;
using ReeperCommon;
using ScienceAlert.Experiments;
using ScienceAlert.ProfileData;
using ScienceAlert.Toolbar;
using ScienceAlert.Windows;
using UnityEngine;

namespace ScienceAlert
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]

	public class ScienceAlert : MonoBehaviour
	{
		private IToolbar button;
		private ScanInterface scanInterface;
		public DraggableOptionsWindow optionsWindow;
		public static ScienceAlert Instance;
		private Settings.ToolbarInterface buttonInterfaceType;
		private Settings.ScanInterface scanInterfaceType;
		public event Callback OnScanInterfaceChanged = delegate{};
		public event Callback OnToolbarButtonChanged = delegate{};

		public IToolbar Button => button;

	    public Settings.ToolbarInterface ToolbarType
		{
			get
			{
				return buttonInterfaceType;
			}
			set
			{
			    if (value == buttonInterfaceType && button != null) return;
			    switch (buttonInterfaceType)
			    {
			        case Settings.ToolbarInterface.BlizzyToolbar:
			            Destroy(button as BlizzyInterface);
			            break;
			    }
			    button = null;
			    switch (value)
			    {
			        case Settings.ToolbarInterface.BlizzyToolbar:
			            if (ToolbarManager.ToolbarAvailable)
			                button = gameObject.AddComponent<BlizzyInterface>();
			            break;
			    }
			    buttonInterfaceType = value;
			    OnToolbarButtonChanged();
			}
		}

		public Settings.ScanInterface ScanInterfaceType
		{
			get
			{
				return scanInterfaceType;
			}
			set
			{
			    if (value == scanInterfaceType && scanInterface != null) return;
			    if (scanInterface != null)
			    {
			        DestroyImmediate(GetComponent<ScanInterface>());
			    }
			    try
			    {
			        switch (value)
			        {
			            case Settings.ScanInterface.None:
			                scanInterface = gameObject.AddComponent<DefaultScanInterface>();
			                break;
			            case Settings.ScanInterface.ScanSat:
			                if (!SCANsatInterface.IsAvailable())
			                {
			                    ScanInterfaceType = Settings.ScanInterface.None;
			                    return;
			                }
			                scanInterface = gameObject.AddComponent<SCANsatInterface>();
			                break;
			            default:
			                throw new NotImplementedException("Unrecognized interface type");
			        }
			    }
			    catch (Exception ex)
			    {
			        Log.Debug("[ScienceAlert]:ScienceAlert.ScanInterfaceType failed with exception {0}", ex);
			        ScanInterfaceType = Settings.ScanInterface.None;
			        return;
			    }
			    scanInterfaceType = value;
			    OnScanInterfaceChanged();
			}
		}

		private IEnumerator Start()
		{
			while (ResearchAndDevelopment.Instance == null)
			{
				yield return 0;
			}
			while (FlightGlobals.ActiveVessel == null)
			{
				yield return 0;
			}
			while (!FlightGlobals.ready)
			{
				yield return 0;
			}
			Instance = this;
			while (ScienceAlertProfileManager.Instance == null || !ScienceAlertProfileManager.Instance.Ready)
			{
				yield return 0;
			}

			try
			{
				ScienceExperiment experiment = ResearchAndDevelopment.GetExperiment("asteroidSample");
				if (experiment != null)
				{
					experiment.experimentTitle = "Sample (Asteroid)";
				}
			}
			catch (KeyNotFoundException)
			{
				Destroy(this);
			}
			gameObject.AddComponent<AudioPlayer>().LoadSoundsFrom(ConfigUtil.GetDllDirectoryPath() + "/sounds");
			gameObject.AddComponent<BiomeFilter>();
			gameObject.AddComponent<ExperimentManager>();
			gameObject.AddComponent<WindowEventLogic>();
			ScanInterfaceType = Settings.Instance.ScanInterfaceType;
			ToolbarType = Settings.Instance.ToolbarInterfaceType;
		}

		public void OnDestroy()
		{
			if (Button != null)
			{
				Button.Drawable = null;
			}
			Settings.Instance.Save();
		}
	}
}
