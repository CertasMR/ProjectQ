using System.Web.Mvc;

namespace ProjectQ.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult API()
        {
            ViewBag.Message = "Add this functionality to your website";

            return View();
        }

    }
}