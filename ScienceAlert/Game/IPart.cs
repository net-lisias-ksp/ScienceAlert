using System.Collections.Generic;

namespace ScienceAlert.Game
{
    public interface IPart
    {
        List<ProtoCrewMember> EvaCapableCrew { get; } // must be a list to prevent unnecessary garbage or extra logic since Part exposes this directly
        IVessel Vessel { get; }
    }
}
