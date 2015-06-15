using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Game
{
    public interface IVessel
    {
        IEnumerable<T> FindPartModulesImplementing<T>() where T: class;
    }
}
