/******************************************************************************
                   Science Alert for Kerbal Space Program                    
 ******************************************************************************
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
 *****************************************************************************/
using System.Collections.Generic;
using System.Linq;
using Experience.Effects;
using ReeperCommon;
using UnityEngine;

namespace ScienceAlert.Experiments.Observers
{
    internal class EvaReportObserver : RequiresCrew
    {
        readonly bool evaUnlocked = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public EvaReportObserver(StorageCache cache, BiomeFilter biomeFilter, ProfileData.ExperimentSettings settings, ScanInterface scanInterface, string expid = "evaReport")
            : base(cache, biomeFilter, settings, scanInterface, expid)
        {
            
            evaUnlocked = GameVariables.Instance.UnlockedEVA(
                ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.AstronautComplex));
        }



        /// <summary>
        /// This function will do one of two things: if the active vessel
        /// isn't an eva kerbal, it will choose a kerbal at random from
        /// the crew and send them on eva (unless the conditions outside
        /// would make it dangerous, in which case player will receive
        /// a dialog instead).
        /// 
        /// On the other hand, if the active vessel is an eva kerbal, it
        /// will deploy the experiment itself.
        /// </summary>
        /// <returns></returns>
        public override bool Deploy()
        {
            if (!Available || !IsReadyOnboard)
            {
                Log.Error("Cannot deploy eva experiment {0}; Available = {1}, Onboard = {2}, experiment = {3}", Available, IsReadyOnboard, experiment.id);
                return false;
            }

            if (FlightGlobals.ActiveVessel == null)
            {
                Log.Error("Deploy -- invalid active vessel");
                return false;
            }

            if (TimeWarp.CurrentRateIndex > 0)
            {
                Log.Error("Cannot deploy eva experiment because TimeWarp is active");
                return false;
            }

            // the current vessel IS NOT an eva'ing Kerbal, so
            // find a kerbal and dump him into space
            if (!FlightGlobals.ActiveVessel.isEVA)
            {
                // check conditions outside
                Log.Warning("Current static pressure: {0}", FlightGlobals.getStaticPressure());

                if (FlightGlobals.getStaticPressure() > Settings.Instance.EvaAtmospherePressureWarnThreshold)
                    if (FlightGlobals.ActiveVessel.GetSrfVelocity().magnitude > Settings.Instance.EvaAtmosphereVelocityWarnThreshold)
                    {
                        Log.Debug("setting up dialog options to warn player about eva risk");

                        var options = new DialogOption[2];

                        options[0] = new DialogOption("Science is worth a little risk", new Callback(OnConfirmEva));
                        options[1] = new DialogOption("No, it would be a PR nightmare", null);

                        var dlg = new MultiOptionDialog("It looks dangerous out there. Are you sure you want to send someone out? They might lose their grip!", "Dangerous Condition Alert", Settings.Skin, options);


                        PopupDialog.SpawnPopupDialog(dlg, false, Settings.Skin);
                        return true;
                    }



                if (!ExpelCrewman())
                {
                    Log.Error("{0}.Deploy - Did not successfully send kerbal out the airlock.  Hatch might be blocked.", GetType().Name);
                    return false;
                }

                return true;

            }
            else
            {
                // The vessel is indeed a kerbalEva, so we can expect to find the
                // appropriate science module now
                var evas = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceExperiment>();
                foreach (var exp in evas)
                    if (!exp.Deployed && exp.experimentID == experiment.id)
                    {
                        exp.DeployExperiment();
                        break;
                    }

                return true;
            }
        }



        protected void OnConfirmEva()
        {
            Log.Normal("EvaObserver: User confirmed eva despite conditions");
            Log.Normal("Expelling... {0}", ExpelCrewman() ? "success!" : "failed");
        }



        /// <summary>
        /// Toss a random kerbal out the airlock
        /// </summary>
        /// <returns></returns>
        protected virtual bool ExpelCrewman()
        {
            // You might think HighLogic.CurrentGame.CrewRoster.GetNextAvailableCrewMember
            // is a logical function to use.  Actually it's possible for it to
            // generate a crew member out of thin air and put it outside, so nope
            // 
            // luckily we can specify a particular onboard Kerbal.  We'll do so by
            // finding the possibilities and then picking one totally at 
            // pseudorandom

            var crewChoices = crewableParts.SelectMany(p => p.protoModuleCrew).ToList();


            if (!crewChoices.Any())
            {
                Log.Error("{0}.Deploy - No crew choices available.  Check logic", GetType().Name);
                return false;
            }
            else
            {
                // 1.4b bugfix for the 1.4a buxfix:
                //  if the player is in map view, SetCameraFlight won't shut it
                // down for us. Whoops
                if (MapView.MapIsEnabled)
                {
                    MapView.ExitMapView();
                }

                // 1.4a bugfix:
                //   if the player is in IVA view when we spawn eva, it looks
                // like KSP doesn't switch cameras automatically
                if ((CameraManager.Instance.currentCameraMode & (CameraManager.CameraMode.Internal | CameraManager.CameraMode.IVA)) != 0)
                {
                    Log.Normal("Detected IVA or internal cam; switching to flight cam");
                    CameraManager.Instance.SetCameraFlight();
                }


                var luckyKerbal = GetBestScienceEvaCandidiate(crewChoices);
                Log.Debug("{0} is the lucky Kerbal.  Out the airlock with him!", luckyKerbal.name);

                // out he goes!
                return FlightEVA.SpawnEVA(luckyKerbal.KerbalRef);
            }
        }

        public override bool IsReadyOnboard
        {
            get
            {
                return GameVariables.Instance.EVAIsPossible(evaUnlocked, FlightGlobals.ActiveVessel) && base.IsReadyOnboard;
            }
        }


        private ProtoCrewMember GetBestScienceEvaCandidiate(List<ProtoCrewMember> crew)
        {
            // use the best science experience available
            var bestScientist = crew
                .Where(
                    pcm =>
                        pcm.experienceTrait != null && pcm.experienceTrait.Effects.Any(effect => effect is ScienceSkill))
                .OrderByDescending(pcm => pcm.experienceLevel)
                .FirstOrDefault();

            // if we haven't got a scientist, just use the first crew we find
            return bestScientist ?? crew.First(pcm => pcm.type != ProtoCrewMember.KerbalType.Tourist);
        }
    }
}
