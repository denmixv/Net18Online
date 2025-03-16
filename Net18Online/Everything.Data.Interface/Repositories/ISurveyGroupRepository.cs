using Everything.Data.Interface.Models.Surveys;

namespace Everything.Data.Interface.Repositories
{
    public interface ISurveyGroupRepository<T> : IBaseRepository<T>
        where T : ISurveyGroupData
    {
        IEnumerable<T> GetAllWithСreatorUsers();
        bool HasUniqueTitle(string title, int id = 0);
        void CreateSurveyGroup(string title, int? userId);
        void UpdateTitle(int id, string value);
    }
}
