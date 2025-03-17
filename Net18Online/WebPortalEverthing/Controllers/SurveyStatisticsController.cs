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

        public SurveyStatisticsController(ISurveysRepositoryReal surveysRepository)
        {
            _surveysRepository = surveysRepository;
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
    }
}
