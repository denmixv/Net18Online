using Enums.Users;
using Everything.Data.Repositories.Surveys;
using Microsoft.AspNetCore.Mvc;
using WebPortalEverthing.Controllers.AuthAttributes;
using WebPortalEverthing.Models.Surveys;

namespace WebPortalEverthing.Controllers
{
    [HasRole(Role.SurveysDataAnalyst)]
    public class SurveyStatisticsController : Controller
    {
        private readonly ISurveysRepositoryReal _surveysRepository;
        private readonly IQuestionRepositoryReal _questionRepository;
        private readonly IAnswerToQuestionRepositoryReal _answerToQuestionRepository;

        public SurveyStatisticsController(ISurveysRepositoryReal surveysRepository, IAnswerToQuestionRepositoryReal answerToQuestionRepository, IQuestionRepositoryReal questionRepository)
        {
            _surveysRepository = surveysRepository;
            _answerToQuestionRepository = answerToQuestionRepository;
            _questionRepository = questionRepository;
        }

        public ActionResult Index()
        {
            var surveys = _surveysRepository.GetWithAnyPassingUsers();

            var viewModel = new SurveyStatisticsIndexViewModel()
            {
                Surveys = surveys.Select(x => new SurveyStatisticSurveysViewModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    CountInProcess = x.PassingUsers
                        .Where(x => x.CompletionStatus == Enums.Surveys.SurveyCompletionStatus.InProgress)
                        .Count(),
                    CountIsCompleted = x.PassingUsers
                        .Where(x => x.CompletionStatus == Enums.Surveys.SurveyCompletionStatus.Completed)
                        .Count()
                })
                .OrderByDescending(x => x.CountIsCompleted)
                .ToList()
            };

            return View(viewModel);
        }

        public ActionResult ViewAnswers(int idSurvey)
        {
            var survey = _surveysRepository.Get(idSurvey);
            var questions = _questionRepository.GetQuestionsForSurvey(idSurvey);
            var answers = _answerToQuestionRepository.GetAnswersToQuestionsBySurvey(idSurvey);

            var viewModel = new SurveyStatisticsViewAnswerViewModel()
            {
                SurveyName = survey.Title,
                Questions = questions.Select(q => new SurveyStatisticsQuestionsWithAnswersViewModel()
                {
                    Title = q.Title,
                    Texts = answers
                        .Where(a => a.IdQuestion == q.Id)
                        .Select(x => x.Text)
                        .ToList()
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
