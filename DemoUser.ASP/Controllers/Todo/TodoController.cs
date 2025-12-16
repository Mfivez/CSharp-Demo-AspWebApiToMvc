using Microsoft.AspNetCore.Mvc;

namespace DemoUser.ASP.Controllers.Todo
{
    public class TodoController : Controller
    {
        private bool IsAuthenticated()
            => HttpContext.Session.GetString("JWT") != null;

        public IActionResult Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Auth");

            return View();
        }

        public IActionResult Create()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Auth");

            return View();
        }

        public IActionResult Edit(Guid id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Auth");

            ViewBag.TodoId = id;
            return View();
        }
    }
}
