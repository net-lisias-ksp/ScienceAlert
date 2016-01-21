namespace ScienceAlert.Game
{
    public interface IScienceSubject
    {
        ScienceSubject Subject { get; }
        string Id { get; }
        float DataScale { get; }
        float Science { get; }
        float ScientificValue { get; }
        float ScienceCap { get; }
        float SubjectValue { get; }
    }
}
