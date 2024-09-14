using Microsoft.AspNetCore.Mvc;

namespace ETICARET.WebUI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
