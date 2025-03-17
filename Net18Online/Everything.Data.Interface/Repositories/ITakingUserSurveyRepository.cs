using Everything.Data.Interface.Models.Surveys;

namespace Everything.Data.Interface.Repositories
{
    public interface ITakingUserSurveyRepository<T> : IBaseRepository<T>
        where T : ITakingUserSurveyData
    {
        int? ReturnIdLastUncompletedSurvey(int userId, int surveyId);
        int Add(int userId, int surveyId);
        void SetCompleteStatus(int takingId);
        T GetWithSurvey(int id);
    }
}