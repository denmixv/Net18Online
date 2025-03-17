using Everything.Data.Repositories.Surveys;
using Microsoft.AspNetCore.SignalR;
using WebPortalEverthing.Services;

namespace WebPortalEverthing.Hubs
{
    public interface ITakingSurveyHub
    {
        Task Notify(string message);
        Task RedirectPage(string url);
        Task MarkUnansweredQuestions(List<int> ids);
    }

    public class TakingSurveyHub : Hub<ITakingSurveyHub>
    {
        private AuthService _authService;
        private ITakingUserSurveyRepositoryReal _takingUserSurveyRepository;
        private IAnswerToQuestionRepositoryReal _answerToQuestionRepository;
        private Dictionary<string /*connectionId*/, int? /*userId*/> _connections = new();

        public TakingSurveyHub(AuthService authService, IAnswerToQuestionRepositoryReal answerToQuestionRepository, ITakingUserSurveyRepositoryReal takingUserSurveyRepository)
        {
            _authService = authService;
            _answerToQuestionRepository = answerToQuestionRepository;
            _takingUserSurveyRepository = takingUserSurveyRepository;
        }

        public override async Task OnConnectedAsync()
        {
            _connections.Add(Context.ConnectionId, _authService.GetUserId());

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _connections.Remove(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public void SetAnswerValue(int answerId, string? value)
        {
            _answerToQuestionRepository.SetTextValue(answerId, value);

            ShowToCurrentUser("Ответ сохранён!");
        }

        public void SubmitSurvey(int takingId)
        {
            var unansweredQuestionsIds = _answerToQuestionRepository.GetIdsUnansweredQuestions(takingId);
            var taking = _takingUserSurveyRepository.GetWithSurvey(takingId);

            if (unansweredQuestionsIds.Count == 0)
            {
                _takingUserSurveyRepository.SetCompleteStatus(takingId);
                var message = $"Пользователь «{_authService.GetName()}» завершил прохождение опроса «{taking.Survey.Title}»";
                Clients.Others.Notify(message).Wait();
                Clients.Caller.RedirectPage("/Surveys/SurveysAll").Wait();
            }
            else
            {
                Clients.Caller.MarkUnansweredQuestions(unansweredQuestionsIds).Wait();
            }
        }

        public void ShowToCurrentUser(string message)
        {
            Clients.Caller.Notify(message).Wait();
        }
    }
}
