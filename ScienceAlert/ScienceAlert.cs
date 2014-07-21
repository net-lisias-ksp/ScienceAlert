/******************************************************************************
 *                  Science Alert for Kerbal Space Program                    *
 *                                                                            *
 * Version 1.6                                                               *
 * Author: xEvilReeperx                                                       *
 * Created: 3/3/2014                                                          *
 * ************************************************************************** *
    Copyright (C) 2014 Allen Mrazek (amrazek@hotmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 ****************************************************************************
 * Changelog
 * 1.7 
 *      Feature: ApplicationLauncher (stock toolbar) support
 *      
 *      Feature: Blizzy's toolbar is now optional. If you have it installed,
 *               you can choose between the stock toolbar and Blizzy's toolbar
 *               in the options menu
 *               
 *      Feature: (Option) Experiment buttons can show next report value
 *      
 *      Feature: (Option) EVA report button can be set to always appear at
 *               the top of the list
 *               
 *      Feature: (Option) Experiment popup can be configured to automatically
 *               reappear on EVA if it was visible before leaving the vessel
 *      
 * 
 *      Bugfix: Fixed issue that caused the popup windows to flicker (especially
 *              the options window)
 *              
 *      
 * 1.6
 *      Bugfix: Fixed issue where alerts could appear for science reports worth
 *              no science
 *              
 *      Bugfix: Rapidly disintegrating vessels could sometimes cause an issue
 *              that spammed KeyNotFound exceptions
 *              
 *      Bugfix: Biome map filter function could sometimes be interrupted and
 *              be left in a bad state, causing ScienceAlert to stop reporting
 *              science
 *              
 *      Bugfix: Biome map filter function could, in rare circumstances, produce
 *              a wrong result
 * 
 * 1.5
 *      New feature: you can now set a minimum threshold for a science report
 *          If performing an experiment won't result in at least that much
 *          science, then it will be ignored even if it would otherwise match
 *          your filter settings
 *          
 *      Small UI improvement: added a collapsing section where new
 *          options can be added
 *          
 *      Fixed an issue where the biome filter could sometimes
 *          ignore biome
 *          
 *      Fixed an issue caused by last version's fix which could make
 *          the flight camera and map camera active at the same time if
 *          the player goes on eva with an active map view
 *          
 *          
 * 1.4a
 *      Fixed an issue where game would use the incorrect camera
 *          after ejecting an eva kerbal, potentially causing a crash
 *          
 * 1.4
 *      Fixed an issue that could cause ScienceAlert to spew NREs when
 *          the orbited body didn't have a biome map
 *          
 * 1.3
 *      Fixed a serious issue: ScienceAlert wasn't taking science
 *          multipliers into account correctly which could result in
 *          incorrect alerts
 *          
 *      Added a global warp setting option
 *      
 *      Added a global alert sound setting option
 *      
 *      Log spam due to vessel modification reduced
 * 
 *      Experiments that rely on custom code to determine availability
 *          will now be hidden from the options window, since ScienceAlert
 *          will be unable to interact with them (determined by checking
 *          if that experiment's biome and situation mask flags are zero)
 * 
 * 
 * 1.2
 *      Fixed a serious issue: "stop on discovery" could sometimes affect
 *          orbital parameters, especially at high warp
 *          
 *      Further tweaked biome detection. There should no longer be any
 *          instances of incorrect science alerts
 * 
 * 1.1
 *      Added EVA condition warning
 *      
 *      Fixed issue where base function of ModuleScienceExperiment was called
 *          instead of the correct one
 *          
 *      Fixed issue where ScienceAlert would detect the wrong biome and flash
 *          the icon at the incorrect time
 *          
 *      Stock experiments no longer have "Assume onboard" option display
 *          by default (to reduce clutter)
 *          
 *      Temporarily added a debug menu, accessable using default settings by
 *          middle-mouse clicking the toolbar button
 *          
 *      Available experiment menu polished a little; will now appear in the
 *          same location consistently
 * 
 * 1.0
 *      First public release
 *****************************************************************************/

//#define PROFILE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using Toolbar;
using ReeperCommon;


// TODO: prevent users from clicking on science button multiple times in rapid
//          succession, activating multiple science reports? Most apparent time
//          is when activating goo, which has a delay as it plays the open animation

// TODO: manually selected transmitters ignore MagicTransmitter
//          further thought: who really does this?  will think on it more

// TODO: properly figure out total science value of all onboard reports
//          (even multiple copies)

// todo: separate science observer for surface samples like the eva one?

// BUG: if an eva report is stored and the player goes on eva in the same
// situation, ScienceAlert reports that an eva report is available
//      haven't been able to verify this one




namespace ScienceAlert
{
    



    [ImprovedAddonLoader.KSPAddonImproved(ImprovedAddonLoader.KSPAddonImproved.Startup.Flight, false)]
    public class ScienceAlert : MonoBehaviour
    {
        // --------------------------------------------------------------------
        //    Members of ScienceAlert
        // --------------------------------------------------------------------
        // related controls
        private Toolbar.IToolbar button;

        // interfaces
        private Settings.ToolbarInterface buttonInterfaceType = Settings.ToolbarInterface.ApplicationLauncher;
        private ScanInterface scanInterface;



/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        void Start()
        {


// just in case this has unforseen consequences later...
// it should be okay since asteroidSample isn't actually defined
// in scienceDefs
#warning Changes asteroidSample title
            Log.Normal("Renaming asteroidSample title");
            var exp = ResearchAndDevelopment.GetExperiment("asteroidSample");
            if (exp != null) exp.experimentTitle = "Sample (Asteroid)";
            

            Log.Debug("Loading sounds...");
            AudioUtil.LoadSoundsFrom("/ScienceAlert/sounds", AudioType.WAV);
            Log.Debug("Sounds ready.");

            Log.Normal("Creating options window");
            gameObject.AddComponent<OptionsWindow>();
            //optionsWindow = new OptionsWindow(this/*, effects.AudioController*/);

            Log.Normal("Creating experiment manager");
            gameObject.AddComponent<ExperimentManager>();

            Log.Normal("Creating debug window");
            gameObject.AddComponent<DebugWindow>();

            Log.Normal("Finished creating windows");


            // set up whichever interface we're using to determine when it's
            // permissable to check for science reports
            // 
            // it's delayed to make sure whichever interface (if not the
            // default) has initialized
            ScheduleInterfaceChange(Settings.Instance.ScanInterfaceType);


            Log.Debug("ScienceAlert.Start: initializing toolbar");
            ToolbarType = Settings.Instance.ToolbarInterfaceType;
            Log.Normal("Toolbar button ready");
        }



        public void OnDestroy()
        {
            Log.Debug("ScienceAlert destroyed");

        }


        /// <summary>
        /// The main reason this exists is that I found it's a good idea to
        /// wait for ScenarioModules to be loaded before trying to interact
        /// with them...
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private System.Collections.IEnumerator ScheduledInterfaceChange(Settings.ScanInterface type)
        {
            Log.Normal("Scheduled interface change");


            if (SCANsatInterface.IsAvailable())
            {
                Log.Debug("Waiting for SCANsat ScenarioModule to exist");

                while (!SCANsatInterface.ScenarioReady())
                    yield return 0;

                Log.Debug("SANsat ScenarioModule ready");
            }


            while (!FlightGlobals.ready || HighLogic.CurrentGame.scenarios.Any(psm =>
            {
                // it's possible for a moduleRef to never become a valid value if
                // an invalid ScenarioModule is left in the persistent file. The easiest
                // way to find out if it'll ever exist is to examine ScenarioRunner's
                // types directly
                if (psm.moduleRef == null)
                {
#if DEBUG
                    ScenarioRunner.fetch.gameObject.PrintComponents();
#endif
                    return ScenarioRunner.fetch.gameObject.GetComponents<ScenarioModule>().ToList().Any(sm =>
                    {
#if DEBUG
                        Log.Debug("checking {0} against {1}", sm.GetType().FullName, psm.moduleName);

                        if (sm.GetType().FullName == psm.moduleName)
                            Log.Error("ScenarioModule '{0}' exists and so will be loaded; continue waiting", psm.moduleName);
#endif
                        return sm.GetType().FullName == psm.moduleName;
                    });
                }
                else return false;
            }))
            {
                Log.Debug("waiting on scenario moduleRefs");
                yield return 0;
            }


            Log.Normal("Performing interface change to {0}", type.ToString());
            SetScanInterfaceType(type);
        }



        /// <summary>
        /// Only purpose is to kick off the ScheduledInterfaceChange 
        /// Coroutine without bothering to make the caller do it
        /// </summary>
        /// <param name="type"></param>
        internal void ScheduleInterfaceChange(Settings.ScanInterface type)
        {
            StartCoroutine(ScheduledInterfaceChange(type));
        }



        /// <summary>
        /// Sets scan interface to specified type, assuming it's available.
        /// If it's not, will default to stock ("none") behaviour.
        /// 
        /// Precondition: SCANsat ScenarioModule (if available) has an existing
        /// instance.
        /// </summary>
        /// <param name="type"></param>
        private void SetScanInterfaceType(Settings.ScanInterface type)
        {
            // remove any existing interfaces
            StripComponent<DefaultScanInterface>(gameObject);
            StripComponent<SCANsatInterface>(gameObject);

            scanInterface = null;

            Log.Normal("Setting scan interface type to {0}", type.ToString());

            try
            {
                switch (type)
                {
                    case Settings.ScanInterface.None:
                        scanInterface = gameObject.AddComponent<DefaultScanInterface>();
                        break;

                    case Settings.ScanInterface.ScanSat:
                        if (!SCANsatInterface.IsAvailable())
                        {
                            Log.Error("SCANsatInterface unavailable! Using default.");
                            SetScanInterfaceType(Settings.ScanInterface.None);
                            break;
                        }

                        var ssInterface = gameObject.AddComponent<SCANsatInterface>();
                        ssInterface.creator = this;
                        scanInterface = ssInterface;

                        break;

                    default:
                        throw new Exception(string.Format("Interface type '{0}' unrecognized", Settings.Instance.ScanInterfaceType));

                }
            }
            catch (Exception e)
            {
                Log.Error("Exception in ScienceAlert.UpdateInterfaceType: {0}", e);
                Log.Warning("Using default interface");

                SetScanInterfaceType(Settings.ScanInterface.None);
            }

            gameObject.SendMessage("Notify_ScanInterfaceChanged");
        }



        private bool StripComponent<T>(GameObject target) where T : Component
        {
            T c = target.GetComponent<T>();

            if (c != null)
            {
                GameObject.Destroy(c);
                return true;
            }
            else return false;
        }



        #region properties


        public Toolbar.IToolbar Button
        {
            get
            {
                return button;
            }
        }



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
                    gameObject.SendMessage("Notify_ToolbarInterfaceChanged");
                }
            }
        }


#endregion 
    }
}
