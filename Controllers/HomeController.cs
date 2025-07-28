using Microsoft.AspNetCore.Mvc;

namespace test2.Controllers
{
    public class HomeController : Controller
    {
        #region action
        public IActionResult Index() { return View(); }
        #endregion
    }
}
