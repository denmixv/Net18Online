using Everything.Data.Models.Surveys;
using Microsoft.AspNetCore.Mvc;
using WebPortalEverthing.Models.Surveys;
using Everything.Data.Interface.Models.Surveys;
using Everything.Data.Repositories.Surveys;
using Enums.Users;
using WebPortalEverthing.Controllers.AuthAttributes;
using WebPortalEverthing.Services;
using Everything.Data.Repositories;
using WebPortalEverthing.Localizations;
using WebPortalEverthing.Models.Surveys.Profile;

namespace WebPortalEverthing.Controllers
{
    public class SurveysController : Controller
    {
        private ISurveyGroupRepositoryReal _surveyGroupRepository;
        private IStatusRepositoryReal _statusRepository;
        private ISurveysRepositoryReal _surveysRepository;
        private IQuestionRepositoryReal _questionRepository;
        private AuthService _authService;
        private IUserRepositryReal _userRepositryReal;
        private FileProvider _fileProvider;

        public SurveysController(ISurveyGroupRepositoryReal surveyGroupRepository, IStatusRepositoryReal statusRepository, ISurveysRepositoryReal surveysRepository, AuthService authService, IQuestionRepositoryReal questionRepository, IUserRepositryReal userRepositryReal, FileProvider fileProvider = null)
        {
            _statusRepository = statusRepository;
            _surveyGroupRepository = surveyGroupRepository;
            _surveysRepository = surveysRepository;
            _authService = authService;
            _questionRepository = questionRepository;
            _userRepositryReal = userRepositryReal;
            _fileProvider = fileProvider;
        }

        [IsAuthenticated]
        public IActionResult UpdateLocale(Language language)
        {
            var userId = _authService.GetUserId()!.Value;
            _userRepositryReal.UpdateLocal(userId, language);

            return RedirectToAction(nameof(Index), nameof(Surveys));
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SurveysAll()
        {
            if (!_statusRepository.Any())
            {
                GenerateDefaultStatuses();
            }

            if (!_surveyGroupRepository.Any())
            {
                GenerateDefaultGroups();
            }

            if (!_surveysRepository.Any())
            {
                GenerateDefaultSurveysInGroups();
            }

            var groupsFromDb = _surveyGroupRepository.GetAll();

            var surveyGroupsViewModels = groupsFromDb
                .Select(GetSurveyGroupViewModelFromData)
                .ToList();

            var surveyExpandGroups = HttpContext.Request.Cookies["survey-expand-groups"];
            var isExpandGroups = false;
            bool.TryParse(surveyExpandGroups, out isExpandGroups);

            var viewModel = new SurveysAllViewModel()
            {
                SurveyGroups = surveyGroupsViewModels,
                IsExpandGroups = isExpandGroups
            };

            return View(viewModel);
        }

        private SurveyGroupViewModel GetSurveyGroupViewModelFromData(ISurveyGroupData surveyGroup)
        {
            var isAllowSurveyCreation = _authService.HasRole(Role.SurveysCreatorOrEditor);
            var surveysFromDb = _surveysRepository.GetAll();

            return new SurveyGroupViewModel
            {
                Id = surveyGroup.Id,
                Title = surveyGroup.Title,
                IsAllowSurveyCreation = isAllowSurveyCreation,
                Surveys = surveysFromDb
                            .Where(survey => survey.SurveyGroup.Id == surveyGroup.Id)
                            .Select(GetSurveyViewModelFromData)
                            .ToList(),
            };
        }

        private SurveyViewModel GetSurveyViewModelFromData(ISurveyData survey)
        {
            return new SurveyViewModel
            {
                Id = survey.Id,
                Status = GetSurveyStatusViewModelFromData(survey),
                Title = survey.Title,
                Actions = GetSurveyActionModelFromData(survey)
            };
        }

        private SurveyStatusViewModel GetSurveyStatusViewModelFromData(ISurveyData survey)
        {
            var statusesFromDb = _statusRepository.GetAll();
            var statusFromDb = statusesFromDb
                .Where(i => i.Id == survey.IdStatus)
                .FirstOrDefault();

            return new SurveyStatusViewModel()
            {
                Title = statusFromDb.Title,
                ImagesSrc = statusFromDb.ImagesSrc
            };
        }

        private List<SurveyActionViewModel> GetSurveyActionModelFromData(ISurveyData survey)
        {
            var actions = new List<SurveyActionViewModel>();

            if (!_authService.IsAuthenticated())
            {
                return actions;
            }

            var buttonEdit = new SurveyActionViewModel()
            {
                Title = "Редактировать",
                Href = $"Edit?idSurvey={survey.Id}"
            };

            var buttonApprove = new SurveyActionViewModel()
            {
                Title = "Утвердить",
                Href = $"Approve/{survey.Id}"
            };

            var buttonTakeSurvey = new SurveyActionViewModel()
            {
                Title = "Пройти",
                Href = $"/TakingSurvey/Index?surveyId={survey.Id}"
            };

            // Хак с id-шниками статусов, позже будет сделано правильно
            switch (survey.IdStatus)
            {
                case 1:
                    actions.Add(buttonEdit);
                    actions.Add(buttonApprove);
                    break;
                case 2:
                    actions.Add(buttonTakeSurvey);
                    break;
            };

            return actions;
        }

        private void GenerateDefaultStatuses()
        {
            var statusNew = new StatusData()
            {
                Title = "Новый опрос",
                ImagesSrc = "/images/Surveys/status/new-48.png",
            };
            _statusRepository.Add(statusNew);

            var statusCompleted = new StatusData()
            {
                Title = "Опрос пройден",
                ImagesSrc = "/images/Surveys/status/status-50.png",
            };
            _statusRepository.Add(statusCompleted);
        }

        private void GenerateDefaultGroups()
        {
            var group = new SurveyGroupData()
            {
                Title = "Оценка / самооценка"
            };
            _surveyGroupRepository.Add(group);

            group = new SurveyGroupData()
            {
                Title = "Удовлетворенность"
            };
            _surveyGroupRepository.Add(group);

            group = new SurveyGroupData()
            {
                Title = "Прочее"
            };
            _surveyGroupRepository.Add(group);
        }

        private void GenerateDefaultSurveysInGroups()
        {
            var surveyGroup = _surveyGroupRepository
                .Get(1);

            var survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 2,
                Title = "Самооценка сотрудника"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 2,
                Title = "Карьерные ожидания сотрудников"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 2,
                Title = "Диагностики синдрома выгорания"
            };
            _surveysRepository.Add(survey);

            surveyGroup = _surveyGroupRepository
                .Get(2);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 2,
                Title = "Анкета удовлетворенности сотрудников"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 2,
                Title = "Удовлетворенность работой и вознаграждениями"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 2,
                Title = "Удовлетворенность условиями труда с оценкой важности"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 1,
                Title = "Название опроса 6"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 1,
                Title = "Название опроса 7"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 1,
                Title = "Название опроса 8"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 1,
                Title = "Название опроса 9"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 1,
                Title = "Название опроса 10"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 1,
                Title = "Название опроса 11"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 1,
                Title = "Название опроса 12"
            };
            _surveysRepository.Add(survey);

            surveyGroup = _surveyGroupRepository
                .Get(3);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 1,
                Title = "Корпоративная культура"
            };
            _surveysRepository.Add(survey);

            survey = new SurveyData()
            {
                SurveyGroup = surveyGroup,
                IdStatus = 2,
                Title = "Диагностика аврала"
            };
            _surveysRepository.Add(survey);
        }

        [HttpGet]
        [HasRole(Role.SurveysCreatorOrEditor)]
        public ActionResult Create(int idGroup)
        {
            var title = "Новый опрос";
            var surveyId = _surveysRepository.CreateSurvey(title, idGroup, null);

            var surveyGroup = _surveyGroupRepository
                .Get(idGroup);

            var surveyCreate = new SurveyCreateViewModel()
            {
                Id = surveyId,
                Title = title,
                SurveyGroup = new SurveyGroupForListViewModel()
                {
                    Id = surveyGroup.Id,
                    Title = surveyGroup.Title,
                },
                Questions = new()
            };

            return View(surveyCreate);
        }

        [HttpPost]
        [HasRole(Role.SurveysCreatorOrEditor)]
        public ActionResult Create(SurveyCreateViewModel surveyCreate)
        {
            if (!ModelState.IsValid)
            {
                surveyCreate.Questions = _questionRepository.GetQuestionsForSurvey(surveyCreate.Id)
                    .Select(question => new QuestionViewModel
                    {
                        Id = question.Id,
                        Title = question.Title,
                        IsRequired = question.IsRequired,
                        AnswerType = question.AnswerType
                    }).ToList();

                return View(surveyCreate);
            }

            _surveysRepository.UpdateTitle(surveyCreate.Id, surveyCreate.Title);
            _surveysRepository.UpdateDescription(surveyCreate.Id, surveyCreate.Description);

            return RedirectToAction(nameof(SurveysAll));
        }

        [HttpGet]
        [HasRole(Role.SurveysCreatorOrEditor)]
        public ActionResult Edit(int idSurvey)
        {
            var surveyFromDb = _surveysRepository.GetWithGroupAndQuestions(idSurvey);

            var surveyCreate = new SurveyCreateViewModel()
            {
                Id = idSurvey,
                Title = surveyFromDb.Title,
                Description = surveyFromDb.Description,
                SurveyGroup = new SurveyGroupForListViewModel()
                {
                    Id = surveyFromDb.SurveyGroup.Id,
                    Title = surveyFromDb.SurveyGroup.Title,
                },
                Questions = surveyFromDb
                    .Questions
                    .Select(question => new QuestionViewModel
                    {
                        Id = question.Id,
                        Title = question.Title,
                        IsRequired = question.IsRequired,
                        AnswerType = question.AnswerType
                    })
                    .ToList()
            };

            return View(nameof(Create), surveyCreate);
        }

        [HttpPost]
        [HasRole(Role.SurveysCreatorOrEditor)]
        public ActionResult Edit(SurveyCreateViewModel surveyCreate)
        {
            if (!ModelState.IsValid)
            {
                surveyCreate.Questions = _questionRepository.GetQuestionsForSurvey(surveyCreate.Id)
                    .Select(question => new QuestionViewModel
                    {
                        Id = question.Id,
                        Title = question.Title,
                        IsRequired = question.IsRequired,
                        AnswerType = question.AnswerType
                    }).ToList();

                return View(nameof(Create), surveyCreate);
            }

            _surveysRepository.UpdateTitle(surveyCreate.Id, surveyCreate.Title);
            _surveysRepository.UpdateDescription(surveyCreate.Id, surveyCreate.Description);

            return RedirectToAction(nameof(SurveysAll));
        }

        [HasRole(Role.SurveysCreatorOrEditor)]
        public IActionResult Approve(int id)
        {
            var survey = _surveysRepository.GetWithGroupAndQuestions(id);

            _surveysRepository.SetStatus(id, 2);

            return RedirectToAction(nameof(SurveysAll));
        }

        [IsAuthenticated]
        public IActionResult Profile()
        {
            var userId = _authService.GetUserId()!.Value;
            var user = _userRepositryReal.Get(userId);

            var viewModel = new ProfileViewModel
            {
                UserName = _authService.GetName()!,
                Age = user.Age,
                Coins = user.Coins,
                AvatarUrl = _userRepositryReal.GetAvatarUrl(userId)
            };

            return View(viewModel);
        }

        [IsAuthenticated]
        [HttpPost]
        public IActionResult Profile(ProfileViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var userId = _authService.GetUserId()!.Value;

            if (viewModel.NewAvatarFile is null)
            {
                ModelState.AddModelError(
                    nameof(ProfileViewModel.NewAvatarFile),
                    "Файл для загрузки не выбран");

                return View(viewModel);
            }

            var newFileName = _fileProvider.GetNewFileName(viewModel.NewAvatarFile.FileName);

            if (!_fileProvider.Save(viewModel.NewAvatarFile, newFileName))
            {
                ModelState.AddModelError(
                    nameof(ProfileViewModel.NewAvatarFile),
                    "Не удалось сохранить файл, обратитесь к администратору портала");

                return View(viewModel);
            }

            var avatarUrl = $"/files/{newFileName}";
            _userRepositryReal.UpdateAvatarUrl(userId, avatarUrl);

            return RedirectToAction(nameof(Profile));
        }
    }
}
