namespace ScienceAlert.Game
{
    public interface IScienceSubjectProvider
    {
        IScienceSubject GetSubject(ScienceExperiment experiment);
    }
}
