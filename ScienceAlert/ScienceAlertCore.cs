//#define PROFILE
using System;
using System.Collections.Generic;
using System.Linq;
using ScienceAlert.Experiments;
using ScienceAlert.Experiments.Factory;
using ScienceAlert.Experiments.Science;
using ScienceAlert.KSPInterfaces.FlightGlobals;
using ScienceAlert.KSPInterfaces.FlightGlobals.Implementations;
using ScienceAlert.KSPInterfaces.GameEvents;
using ScienceAlert.KSPInterfaces.GameEvents.Implementations;
using ScienceAlert.KSPInterfaces.ResearchAndDevelopment;
using ScienceAlert.KSPInterfaces.ResearchAndDevelopment.Implementations;
using ScienceAlert.ProfileData;
using ScienceAlert.ProfileData.Implementations;
using ScienceAlert.Windows;
using UnityEngine;
using ReeperCommon;


namespace ScienceAlert
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ScienceAlertCore : MonoBehaviour
    {
        // --------------------------------------------------------------------
        //    Members of ScienceAlert
        // --------------------------------------------------------------------

        // owned objects
        private Toolbar.IToolbar button;

        // interfaces
        private Settings.ToolbarInterface buttonInterfaceType = Settings.ToolbarInterface.ApplicationLauncher;
        private Settings.ScanInterface scanInterfaceType = Settings.ScanInterface.None;

        // events
        public event Callback OnScanInterfaceChanged = delegate { };
        public event Callback OnToolbarButtonChanged = delegate { };


        private IResearchAndDevelopmentProvider _rndProvider;
        private IProfileManagerProvider _profileManagerProvider;
        private IProfileManager _profileManager;
        private IActiveVesselProvider _activeVesselProvider;
        private IGameEvents _gameEvents;
        private ISensorFactory _sensorFactory;
        private IStoredVesselScience _storedVesselScience;
        

        // need interface adapters still
        private SensorManager _sensorManager;
        private WindowEventLogic _windowLogic;


/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/
        System.Collections.IEnumerator Start()
        {
            _rndProvider = new KspResearchAndDevelopmentProvider();
            _profileManagerProvider = new ProfileManagerProvider();

            // wait for ScenarioModules like R&D and our own ProfileManager to initialize, since
            // they may lag behind our own construction
            Log.Normal("Waiting on R&D...");
            while (!_rndProvider.Instance.Any()) yield return 0;


            Log.Normal("Waiting on ProfileManager...");
            while (!_profileManagerProvider.GetProfileManager().Any()) yield return 0;
            _profileManager = _profileManagerProvider.GetProfileManager().Single();

            
            if (_profileManager.IsNull())
            {
                Log.Error("wtf, profileManager is null?");
                Log.Error("Any = {0}", _profileManagerProvider.GetProfileManager().Any());
            }


            Log.Normal("Initializing ScienceAlert");

// just in case this has unforseen consequences later...
// it should be okay since asteroidSample isn't actually defined
// in scienceDefs, who would know to mess with it?
#warning Changes asteroidSample title
            Log.Verbose("Renaming asteroidSample title");
            var exp = ResearchAndDevelopment.GetExperiment("asteroidSample");
            if (exp != null) exp.experimentTitle = "Sample (Asteroid)";


            _gameEvents = new KspGameEvents();
            _activeVesselProvider = new KspActiveVesselProvider();

            Log.Verbose("Loading sounds...");
            gameObject.AddComponent<AudioPlayer>().LoadSoundsFrom(ConfigUtil.GetDllDirectoryPath() + "/sounds");
            Log.Verbose("Sounds ready.");

            _storedVesselScience = new StoredVesselScience(_activeVesselProvider);




            CreateBiomeFilter();


            _sensorFactory = new SensorFactory(_profileManager, _storedVesselScience, BiomeFilter);
            _sensorManager = new Experiments.SensorManager(_sensorFactory, _storedVesselScience, _activeVesselProvider);



            _windowLogic = new WindowEventLogic(this, BiomeFilter, _storedVesselScience, _profileManager, _sensorManager);



            Log.Verbose("Finished creating windows");


            // set up whichever interface we're using to determine when it's
            // permissable to check for science reports
            ScanInterfaceType = Settings.Instance.ScanInterfaceType;



            Log.Verbose("ScienceAlert.Start: initializing toolbar");
            ToolbarType = Settings.Instance.ToolbarInterfaceType;
            Log.Verbose("Toolbar button ready");


            Log.Normal("Registering for events");
            RegisterEvents();
            


            Log.Normal("ScienceAlert initialization finished.");

        }



        public void OnDestroy()
        {
            DestroyBiomeFilter();

            Button.Drawable = null;
            Settings.Instance.Save();
            Log.Verbose("ScienceAlert destroyed");
        }


     

        private void CreateBiomeFilter()
        {
            BiomeFilter = new BiomeFilter();
            GameEvents.onVesselChange.Add(BiomeFilter.OnVesselChanged);
            GameEvents.onDominantBodyChange.Add(BiomeFilter.OnDominantBodyChanged);
        }

        private void DestroyBiomeFilter()
        {
            GameEvents.onDominantBodyChange.Remove(BiomeFilter.OnDominantBodyChanged);
            GameEvents.onVesselChange.Remove(BiomeFilter.OnVesselChanged);
            BiomeFilter = null;
        }


        private void RegisterEvents()
        {
            var profileManager = _profileManagerProvider.GetProfileManager();
            if (!profileManager.Any())
                throw new Exception("ProfileManager not found when registering events");

            _gameEvents.OnVesselCreate += profileManager.Single().OnVesselCreate;
            _gameEvents.OnVesselChange += profileManager.Single().OnVesselChange;
            _gameEvents.OnVesselDestroy += profileManager.Single().OnVesselDestroy;
        }

        private void UnregisterEvents()
        {

        }

        private void Update()
        {
            BiomeFilter.UpdateBiomeData();
        }




        public Toolbar.IToolbar Button
        {
            get
            {
                return button;
            }
        }


        public BiomeFilter BiomeFilter { get; private set; }


        /// <summary>
        /// Switch between toolbar at runtime as desired by the user
        /// </summary>
        public Settings.ToolbarInterface ToolbarType
        {
            get
            {
                return buttonInterfaceType;
            }
            set
            {
                if (value != buttonInterfaceType || button == null)
                {
                    if (button != null)
                    {
                        var c = gameObject.GetComponent(button.GetType());
                        if (c == null) Log.Warning("ToolbarInterface: Did not find previous interface");

                        Destroy(c);
                        button = null;
                    }

                    switch (value)
                    {
                        case Settings.ToolbarInterface.BlizzyToolbar:
                            Log.Verbose("Setting Toolbar interface to BlizzyToolbar");

                            if (ToolbarManager.ToolbarAvailable)
                            {
                                button = gameObject.AddComponent<Toolbar.BlizzyInterface>();
                            }
                            else
                            {
                                Log.Warning("Cannot set BlizzyToolbar; Toolbar not found!");
                                ToolbarType = Settings.ToolbarInterface.ApplicationLauncher;
                            }

                            break;

                        case Settings.ToolbarInterface.ApplicationLauncher:
                            Log.Verbose("Setting Toolbar interface to Application Launcher");

                            button = gameObject.AddComponent<Toolbar.AppLauncherInterface>();
                            break;
                    }

                    buttonInterfaceType = value;
                    OnToolbarButtonChanged();
                }
            }
        }



        /// <summary>
        /// Switch between scan interfaces (used to determine whether the
        /// player "knows about" a biome and can be alerted if the experiment
        /// in question uses the biome mask for a given situation)
        /// </summary>
        public Settings.ScanInterface ScanInterfaceType
        {
            get
            {
                return scanInterfaceType;
            }
            set
            {
                if (value != scanInterfaceType || ScanInterface == null)
                {
                    if (ScanInterface != null) DestroyImmediate(GetComponent<ScanInterface>());

                    Log.Normal("Setting scan interface type to {0}", value.ToString());

                    try
                    {
                        switch (value)
                        {
                            case Settings.ScanInterface.None:
                                ScanInterface = gameObject.AddComponent<DefaultScanInterface>();
                                break;

                            case Settings.ScanInterface.ScanSat:
                                if (SCANsatInterface.IsAvailable())
                                {
                                    ScanInterface = gameObject.AddComponent<SCANsatInterface>();
                                    break;
                                }
                                else
                                {
                                    Log.Write("SCANsat Interface is not available. Using default.");
                                    ScanInterfaceType = Settings.ScanInterface.None;
                                    return;
                                }

                            default:
                                throw new NotImplementedException("Unrecognized interface type");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("ScienceAlert.ScanInterfaceType failed with exception {0}", e);

                        // default interface should always be available, that should be safe
                        // unless things are completely unrecoverable
                        ScanInterfaceType = Settings.ScanInterface.None;
                        return;
                    }


                    scanInterfaceType = value;
                    OnScanInterfaceChanged();
                    Log.Normal("Scan interface type is now {0}", ScanInterfaceType.ToString());
                }
            }
        }



        public ScanInterface ScanInterface
        {
            get;
            private set;
        }



    }




}
