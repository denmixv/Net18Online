namespace WebPortalEverthing.Models.Surveys
{
    public class SurveyStatisticsViewAnswerViewModel
    {
        public string SurveyName { get; set; }
        public List<SurveyStatisticsQuestionsWithAnswersViewModel> Questions { get; set; } = [];
    }
}
