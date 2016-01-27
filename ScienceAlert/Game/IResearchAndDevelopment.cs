using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert.Game
{
    public interface IResearchAndDevelopment
    {
        List<IScienceSubject> GetSubjects();
    }
}
