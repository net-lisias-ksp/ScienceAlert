using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ScienceAlert.Game
{
    public interface IPart
    {
        GameObject gameObject { get; }

        ReadOnlyCollection<ProtoCrewMember> EvaCapableCrew { get; } // must be a list to prevent unnecessary garbage or extra logic since Part exposes this directly
        ReadOnlyCollection<PartModule> Modules { get; }
    }
}
