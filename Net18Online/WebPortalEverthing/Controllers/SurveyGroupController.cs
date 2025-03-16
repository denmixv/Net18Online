using Everything.Data.Models.Surveys;
using Microsoft.AspNetCore.Mvc;
using WebPortalEverthing.Models.Surveys;
using Everything.Data.Interface.Models.Surveys;
using Everything.Data.Repositories.Surveys;
using WebPortalEverthing.Services;
using Everything.Data.Models;

namespace WebPortalEverthing.Controllers
{
    public class SurveyGroupController : Controller
    {
        private ISurveyGroupRepositoryReal _surveyGroupRepository;
        private IStatusRepositoryReal _statusRepository;
        private AuthService _authService;

        public SurveyGroupController(ISurveyGroupRepositoryReal surveyGroupRepository, IStatusRepositoryReal statusRepository, AuthService authService)
        {
            _statusRepository = statusRepository;
            _surveyGroupRepository = surveyGroupRepository;
            _authService = authService;
        }

        public ActionResult Index()
        {
            var groupsFromDb = _surveyGroupRepository.GetAllWithСreatorUsers();

            var surveyGroupsViewModels = groupsFromDb
                .Select(GetSurveyGroupViewModelFromData)
                .ToList();

            return View(surveyGroupsViewModels);
        }

        private SurveyGroupViewModel GetSurveyGroupViewModelFromData(SurveyGroupData surveyGroup)
        {
            return new SurveyGroupViewModel
            {
                Id = surveyGroup.Id,
                Title = surveyGroup.Title,
                СreatorUser = GetСreatorUserViewModelViewModelFromData(surveyGroup.СreatorUser)
            };
        }

        private СreatorUserViewModel? GetСreatorUserViewModelViewModelFromData(UserData? userData)
        {   
            if (userData == null)
            {
                return null;
            }

            return new СreatorUserViewModel
            {
                Login = userData.Login
            };
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(SurveyGroupCreateViewModel viewModel)
        {
            if (!_surveyGroupRepository.HasUniqueTitle(viewModel.Title))
            {
                ModelState.AddModelError(
                    nameof(SurveyGroupCreateViewModel.Title),
                    "Группа опросов с таким названием уже существует");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var userId = _authService.GetUserId();
            _surveyGroupRepository.CreateSurveyGroup(viewModel.Title, userId);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var group = _surveyGroupRepository.Get(id);
            var viewModel = new SurveyGroupCreateViewModel()
            {
                Id = group.Id,
                Title = group.Title
            };

            return View(nameof(Create), viewModel);
        }

        [HttpPost]
        public ActionResult Edit(SurveyGroupCreateViewModel viewModel)
        {
            if (!_surveyGroupRepository.HasUniqueTitle(viewModel.Title, viewModel.Id))
            {
                ModelState.AddModelError(
                    nameof(SurveyGroupCreateViewModel.Title),
                    "Группа опросов с таким названием уже существует");
            }

            if (!ModelState.IsValid)
            {
                return View(nameof(Create), viewModel);
            }

            _surveyGroupRepository.UpdateTitle(viewModel.Id, viewModel.Title);

            return RedirectToAction(nameof(Index));
        }

        public ActionResult Delete(int id)
        {
            _surveyGroupRepository.Delete(id);

            return RedirectToAction(nameof(Index));
        }
    }
}
