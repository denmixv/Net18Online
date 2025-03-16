namespace WebPortalEverthing.Models.Surveys
{
    public class SurveyViewModel
    {
        public int Id { get; set; }
        public SurveyStatusViewModel Status { get; set; }
        public string Title { get; set; }
        public List<SurveyActionViewModel> Actions { get; set; } = [];
    }
}
