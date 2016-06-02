using System.Collections.Generic;

namespace ScienceAlert.Game
{
    public interface IResearchAndDevelopment
    {
        List<IScienceSubject> Subjects { get; }
    }
}
