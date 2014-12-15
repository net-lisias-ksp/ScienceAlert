using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScienceAlert.ReeperCommon.Window
{
    abstract class WindowComponent
    {
        protected virtual void Awake() {}

        protected virtual void Start()
        {
        }

        protected virtual void OnGUI()
        {
        }

        protected virtual void OnDestroy()
        {
        }
    }
}
