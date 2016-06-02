namespace ScienceAlert.Game
{
    public interface IExistingScienceSubjectProvider
    {
        IScienceSubject GetExistingSubject(ScienceExperiment experiment, ExperimentSituations situation, ICelestialBody body, string biome);
    }
}
