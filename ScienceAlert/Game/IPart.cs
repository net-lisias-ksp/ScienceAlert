using System.Collections.Generic;
using UnityEngine;

namespace ScienceAlert.Game
{
    public interface IPart
    {
        GameObject gameObject { get; }

        List<ProtoCrewMember> EvaCapableCrew { get; } // must be a list to prevent unnecessary garbage or extra logic since Part exposes this directly
        List<PartModule> Modules { get; }
    }
}
