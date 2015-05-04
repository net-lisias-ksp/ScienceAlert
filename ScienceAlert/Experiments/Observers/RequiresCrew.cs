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
using ReeperCommon;

namespace ScienceAlert.Experiments.Observers
{
    /// <summary>
    /// A special observer that uses the existence of crew, not
    /// an available ModuleScienceExperiment, to determine whether
    /// the experiment is possible
    /// </summary>
    public class RequiresCrew : ExperimentObserver
    {
        protected readonly List<Part> CrewableParts = new List<Part>();
        protected readonly List<KerbalSeat> CommandSeats = new List<KerbalSeat>();
 
        public RequiresCrew(StorageCache cache, BiomeFilter biomeFilter, ProfileData.ExperimentSettings settings, ScanInterface scanInterface, string expid)
            : base(cache, biomeFilter, settings, scanInterface, expid)
        {
            this.requireControllable = false;
        }

        /// <summary>
        /// Note: ScienceAlert will look out for vessel changes for
        /// us and call Rescan() as necessary
        /// </summary>
        public override void Rescan()
        {
            base.Rescan();

            CrewableParts.Clear();
            CommandSeats.Clear();

            if (FlightGlobals.ActiveVessel == null)
            {
                Log.Debug("EvaReportObserver: active vessel null; observer will not function");
                return;
            }

            // cache any part that can hold crew, so we don't have to
            // wastefully go through the entire vessel part tree
            // when updating status
            FlightGlobals.ActiveVessel.parts.ForEach(p =>
            {
                if (p.CrewCapacity > 0) CrewableParts.Add(p);
                if (p.GetComponent<KerbalSeat>() != null) CommandSeats.Add(p.GetComponent<KerbalSeat>());
            });

        }


        public override bool IsReadyOnboard
        {
            get
            {
                if (Experiment.IsUnlocked())
                    return CrewableParts.Any(PartHasCrewThatAreNotTourists);
                return false;
            }
        }


        private bool PartHasCrewThatAreNotTourists(Part part)
        {
            return part.protoModuleCrew.Any(crew => crew.type != ProtoCrewMember.KerbalType.Tourist);
        }


        protected bool CrewIsInCommandSeat(ProtoCrewMember pcm)
        {
            return CommandSeats
                .Any(cs => cs.Occupant != null && cs.Occupant.protoModuleCrew.Contains(pcm));
        }
    }
}
