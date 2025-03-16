using Everything.Data.Interface.Repositories;
using Everything.Data.Models.Surveys;
using Microsoft.EntityFrameworkCore;

namespace Everything.Data.Repositories.Surveys
{
    public interface ISurveysRepositoryReal : ISurveysRepository<SurveyData>
    {
    }

    public class SurveysRepository : BaseRepository<SurveyData>, ISurveysRepositoryReal
    {
        public SurveysRepository(WebDbContext webDbContext) : base(webDbContext)
        {
        }

        public SurveyData GetWithGroupAndQuestions(int id)
        {
            return _dbSet
                .Include(x => x.SurveyGroup)
                .Include(x => x.Questions)
                .First(x => x.Id == id);
        }

        public int CreateSurvey(string title, int groupId, string? description)
        {
            var group = _webDbContext
                .SurveyGroups
                .First(x => x.Id == groupId);

            var survey = new SurveyData()
            {
                SurveyGroup = group,
                IdStatus = 1, // Пока такой хардкод, пока не сделана связка между таблицами
                Title = title,
                Description = description
            };

            _dbSet.Add(survey);
            _webDbContext.SaveChanges();

            return survey.Id;
        }

        public void UpdateTitle(int id, string newTitle)
        {
            var survey = _webDbContext
                .Surveys
                .First(x => x.Id == id);

            survey.Title = newTitle;

            _webDbContext.SaveChanges();
        }

        public void UpdateDescription(int id, string? description)
        {
            var survey = _dbSet.First(x => x.Id == id);

            survey.Description = description;

            _webDbContext.SaveChanges();
        }

        public void SetStatus(int id, int idStatus)
        {
            var survey = Get(id);

            survey.IdStatus = idStatus;

            _webDbContext.SaveChanges();
        }
    }
}
