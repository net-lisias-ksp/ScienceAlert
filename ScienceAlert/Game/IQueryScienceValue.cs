namespace ScienceAlert.Game
{
    public interface IQueryScienceValue
    {
        float GetScienceValue(float dataAmount, ScienceSubject subject, float xmitScalar);
        float GetNextScienceValue(float dataAmount, ScienceSubject subject, float xmitScalar);
    }
}
