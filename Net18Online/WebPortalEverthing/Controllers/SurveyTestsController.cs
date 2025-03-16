using Microsoft.AspNetCore.Mvc;

namespace WebPortalEverthing.Controllers
{
    public class SurveyTestsController : Controller
    {
        public SurveyTestsController()
        {
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}
