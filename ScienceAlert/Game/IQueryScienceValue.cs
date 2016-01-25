namespace ScienceAlert.Game
{
    public interface IQueryScienceValue
    {
        float GetScienceValue(float dataAmount, IScienceSubject subject, float xmitScalar);
        float GetNextScienceValue(float dataAmount, IScienceSubject subject, float xmitScalar);
        float GetReferenceDataValue(float dataAmount, IScienceSubject subject);
    }
}
