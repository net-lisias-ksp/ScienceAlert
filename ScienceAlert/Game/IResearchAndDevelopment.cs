using System.Collections.Generic;
using ReeperCommon.Containers;

namespace ScienceAlert.Game
{
    public interface IResearchAndDevelopment
    {
        List<IScienceSubject> Subjects { get; }
    }
}
