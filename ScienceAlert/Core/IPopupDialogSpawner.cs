using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Core
{
    public interface IPopupDialogSpawner
    {
        void CriticalError(string details);
    }
}
