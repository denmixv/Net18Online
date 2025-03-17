using Enums.Surveys;
using Everything.Data.Interface.Repositories;
using Everything.Data.Models.Surveys;
using Microsoft.EntityFrameworkCore;

namespace Everything.Data.Repositories.Surveys
{
    public interface ITakingUserSurveyRepositoryReal : ITakingUserSurveyRepository<TakingUserSurveyData>
    {
    }

    public class TakingUserSurveyRepository : BaseRepository<TakingUserSurveyData>, ITakingUserSurveyRepositoryReal
    {
        public TakingUserSurveyRepository(WebDbContext webDbContext) : base(webDbContext)
        {
        }

        public override int Add(TakingUserSurveyData data)
        {
            throw new NotImplementedException($"Use method {nameof(Add)} with parameters userId and surveyId to add a record");
        }

        public int? ReturnIdLastUncompletedSurvey(int userId, int surveyId)
        {
            if (!_dbSet.Any())
            {
                return null;
            }

            return _dbSet
                .Where(x => x.User.Id == userId
                   && x.Survey.Id == surveyId
                   && x.CompletionStatus == SurveyCompletionStatus.InProgress)
                .OrderByDescending(x => x.StartTime)
                .FirstOrDefault()
                ?.Id;
        }

        public int Add(int userId, int surveyId)
        {
            var user = _webDbContext.Users.First(u => u.Id == userId);
            var survey = _webDbContext.Surveys.First(s => s.Id == surveyId);

            var takingData = new TakingUserSurveyData()
            {
                StartTime = DateTime.Now,
                CompletionStatus = SurveyCompletionStatus.InProgress,
                User = user,
                Survey = survey
            };

            _dbSet.Add(takingData);
            _webDbContext.SaveChanges();

            // Создадим заглушки для ответов
            var questions = _webDbContext.Questions.Where(q => q.Survey.Id == surveyId);
            var answerData = questions.Select(q => new AnswerToQuestionData
            {
                Text = string.Empty,
                TakingUserSurvey = takingData,
                Question = q
            });

            _webDbContext.AnswerToQuestions.AddRange(answerData);
            _webDbContext.SaveChanges();

            return takingData.Id;
        }

        public void SetCompleteStatus(int takingId)
        {
            var takingData = Get(takingId);
            takingData.CompletionStatus = SurveyCompletionStatus.Completed;

            _webDbContext.SaveChanges();
        }

        public TakingUserSurveyData GetWithSurvey(int id)
        {
            return _dbSet
                .Include(x => x.Survey)
                .First(x => x.Id == id);
        }
    }
}
