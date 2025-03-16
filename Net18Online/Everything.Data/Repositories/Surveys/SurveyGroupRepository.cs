using Everything.Data.Interface.Repositories;
using Everything.Data.Models.Surveys;
using Microsoft.EntityFrameworkCore;

namespace Everything.Data.Repositories.Surveys
{
    public interface ISurveyGroupRepositoryReal : ISurveyGroupRepository<SurveyGroupData>
    {
    }

    public class SurveyGroupRepository : BaseRepository<SurveyGroupData>, ISurveyGroupRepositoryReal
    {
        public SurveyGroupRepository(WebDbContext webDbContext) : base(webDbContext)
        {
        }

        public IEnumerable<SurveyGroupData> GetAllWithСreatorUsers()
        {
            return _dbSet
                .Include(x => x.СreatorUser)
                .ToList();
        }

        public void UpdateTitle(int id, string newTitle)
        {
            var surveyGroup = Get(id);

            surveyGroup.Title = newTitle;

            _webDbContext.SaveChanges();
        }

        public bool HasUniqueTitle(string title, int id = 0)
        {
            if (id == 0)
            {
                return !_dbSet.Any(x => x.Title == title);
            }

            return !_dbSet.Any(x => x.Title == title && x.Id != id);
        }

        public void CreateSurveyGroup(string title, int? userId)
        {
            var user = _webDbContext
                .Users
                .FirstOrDefault(x => x.Id == userId);

            var surveyGroup = new SurveyGroupData()
            {
                Title = title,
                СreatorUser = user
            };

            Add(surveyGroup);
        }
    }
}
