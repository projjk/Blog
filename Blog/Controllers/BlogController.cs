using System.Diagnostics;
using System.Security.Cryptography;
using AutoMapper;
using Blog.Areas.Identity.Data;
using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;

namespace Blog.Controllers;

public class BlogController : Controller
{
    private readonly ILogger<BlogController> _logger;
    private readonly ISqlBlogData _repository;
    private readonly IMapper _mapper;
    private readonly UserManager<BlogUser> _userManager;
    private readonly BlogIdentityDbContext _db;
    private readonly IConfiguration _configuration;

    public BlogController(ILogger<BlogController> logger, ISqlBlogData repository, IMapper mapper,
        UserManager<BlogUser> userManager, BlogIdentityDbContext db, IConfiguration configuration)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _userManager = userManager;
        _db = db;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string blogAddress)
    {
        Models.Blog? blog;

        if (string.IsNullOrWhiteSpace(blogAddress))
        {
            if (!int.TryParse(_configuration["Blog:DefaultBlogId"], out int blogId))
            {
                blogId = 1;
            }

            blog = await _db.Blogs.Include(b => b.Owner).FirstOrDefaultAsync(b => b.Id == blogId);

            if (blog == null)
            {
                ViewBag.ErrorMessage = "Can't find the default blog to show.";
                _logger.LogCritical("Can't find the default blog.");
                return View("Error");
            }
        }
        else
        {
            blog = await _db.Blogs.Include(b => b.Owner).FirstOrDefaultAsync(b => b.BlogAddress == blogAddress);
            if (blog == null)
            {
                ViewBag.ErrorMessage = "There's no blog with that address.";
                return View("Error");
            }
        }

        return View(blog);
    }

    public IActionResult Write()
    {
        return View();
    }

    public IActionResult Edit(int id)
    {
        return View();
    }

    public IActionResult View(string blogAddress, string articleUrl)
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(BlogCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            model.BlogAddress = model.BlogAddress.ToLower();
            var addressBlackList = new[] { "create", "view", "cancel", "edit", "manage", "delete", "post", "jake", "admin", "view" };
            var titleBlackList = new[] { "fuck", "suck" };
            if (addressBlackList.FirstOrDefault(elem => model.BlogAddress.Contains(elem)) != null
                || titleBlackList.FirstOrDefault(elem => model.BlogAddress.Contains(elem)) != null)
            {
                ModelState.AddModelError("BlogAddress", "The address contains forbidden words.");
                return View(model);
            }
            if (titleBlackList.FirstOrDefault(elem => model.BlogTitle.Contains(elem)) != null)
            {
                ModelState.AddModelError("BlogTitle", "The title contains forbidden words.");
                return View(model);
            }
            var exist = await _db.Blogs.FirstOrDefaultAsync(b => b.BlogAddress == model.BlogAddress);
            if (exist != null)
            {
                ModelState.AddModelError("BlogAddress", "This address is already used by someone. Try with another one.");
                return View(model);
            }

            var user = await _db.Users.Include(x => x.Blog)
                .SingleOrDefaultAsync(x => x.Id == _userManager.GetUserId(User));
            if (user == null)
            {
                ModelState.AddModelError("BlogTitle", "Only members can create a blog.");
                return View(model);
            }

            if (user.Blog != null)
            {
                // this block should not be accessed.
                ModelState.AddModelError("BlogTitle", "You already own your blog.");
                _logger.LogCritical("Cannot retrieve a user info during blog creation.");
                return View(model);
            }
            
            var blog = _mapper.Map<Models.Blog>(model);
            user.Blog = blog;
            blog.Owner = user;
            blog.Categories = new List<Category> { new Category { Name = "General", Count = 0, Owner = user } };
            _repository.CreateBlog(blog);
            return RedirectToAction("CreateComplete");
        }

        return View(model);
    }

    public IActionResult CreateComplete()
    {
        ViewBag.CompleteMessage = "Thanks for your order. You'll soon enjoy our delicious pies!";
        return View("View");
    }

    public IActionResult Manage()
    {
        return View();
    }
}