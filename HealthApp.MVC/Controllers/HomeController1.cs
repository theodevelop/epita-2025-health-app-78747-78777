using Microsoft.AspNetCore.Mvc;

namespace HealthApp.MVC.Controllers
{
    public class HomeController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
