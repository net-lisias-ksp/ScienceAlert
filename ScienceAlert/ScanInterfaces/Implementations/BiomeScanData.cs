using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.ScanInterfaces.Implementations
{
    abstract class BiomeScanData : IBiomeScanData
    {
        public abstract bool HaveScanData(double latitude, double longitude);
    }
}
