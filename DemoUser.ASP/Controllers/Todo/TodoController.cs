using Microsoft.AspNetCore.Mvc;

namespace DemoUser.ASP.Controllers.Todo
{
    public class TodoController : Controller
    {
        public IActionResult Index()
            => View();

        public IActionResult Create() 
            => View();

        public IActionResult Edit(Guid id)
        {
            ViewBag.TodoId = id;
            return View();
        }
    }
}
