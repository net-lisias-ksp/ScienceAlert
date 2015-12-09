using System;
using ReeperCommon.Containers;
using UnityEngine;

namespace ScienceAlert.Core.Gui
{
    public class SkinNotCreatedException : Exception
    {
        public SkinNotCreatedException(GUISkin original)
            : base("Failed to clone " + original.Return(s => s.name, "<null>"))
        {
            
        }
    }
}
