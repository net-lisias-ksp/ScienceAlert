namespace ScienceAlert.Game
{
    public interface IScienceLab
    {
        bool HasAnalyzedSubject(IScienceSubject subject);

        float SurfaceBonus { get; }
        float ContextBonus { get; }
        float HomeworldMultiplier { get; }
    }
}
