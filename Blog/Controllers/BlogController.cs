using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers;

public class BlogController : Controller
{
    // GET
    public IActionResult Index(String blogname)
    {
        return View();
    }

    public IActionResult Write()
    {
        return View();
    }
}