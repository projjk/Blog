using AutoMapper;
using Blog.Areas.Identity.Data;
using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
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
    private readonly SignInManager<BlogUser> _signInManager;

    public BlogController(ILogger<BlogController> logger, ISqlBlogData repository, IMapper mapper,
        UserManager<BlogUser> userManager, BlogIdentityDbContext db, IConfiguration configuration,
        SignInManager<BlogUser> signInManager)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _userManager = userManager;
        _db = db;
        _configuration = configuration;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Index(string blogAddress, int? page, string? tag, string? category)
    {
        Models.Blog? blog;
        Category currentCategory;

        // blogAddress
        if (string.IsNullOrWhiteSpace(blogAddress))
        {
            if (!int.TryParse(_configuration["Blog:DefaultBlogId"], out int blogId))
            {
                blogId = 1;
            }

            blog = await _db.Blogs.Include(b => b.Owner)
                .Include(b => b.DefaultCategory)
                .FirstOrDefaultAsync(b => b.Id == blogId);

            if (blog == null)
            {
                ViewBag.ErrorMessage = "Can't find the default blog to show.";
                _logger.LogCritical("Can't find the default blog.");
                return View("Error");
            }
        }
        else
        {
            blog = await _db.Blogs.Include(b => b.Owner)
                .Include(b => b.DefaultCategory)
                .FirstOrDefaultAsync(b => b.BlogAddress == blogAddress);
            if (blog == null)
            {
                ViewBag.ErrorMessage = "There's no blog with that address.";
                return View("Error");
            }
        }

        // category
        if (string.IsNullOrWhiteSpace(category))
        { 
            currentCategory = blog.DefaultCategory;
        }
        else
        {
            currentCategory = blog.Categories.FirstOrDefault(c => c.Name.ToUpper() == category.ToUpper()) ?? blog.DefaultCategory;
        }
        
        // viewtype
        switch (currentCategory.CategoryType)
        {
            case CategoryTypeEnum.List:
                break;
            case CategoryTypeEnum.View:
                var model = _mapper.Map<BlogIndexIndex>(blog);
                PaginatedList<Article>? articles = null;
                var blogArticles = _db.Entry(blog).Collection(x => x.Articles)
                    .Query()
                    .Where(a => a.Category.Name.ToUpper() == currentCategory.Name.ToUpper())
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .Include(a => a.Comments)
                    .Include(a => a.Tags)
                    .OrderByDescending(a => a.PostDate)
                    .AsNoTracking();
                articles =
                    await PaginatedList<Article>.CreateAsync(blogArticles, page ?? 1,
                        currentCategory.ItemsPerPage);

                model.Articles = articles;
                model.Category = currentCategory;
                return View("IndexIndex", model);

            case CategoryTypeEnum.Gallery:
                break;
        }

        return View(blog);
    }

    [Authorize(Roles = "Blogger")]
    public async Task<IActionResult> Write()
    {
        var model = new BlogWriteViewModel();
        var user = await _db.Users.Include(x => x.Blog)
            .SingleOrDefaultAsync(x => x.Id == _userManager.GetUserId(User));
        if (user!.Blog == null)
        {
            ViewBag.ErrorMessage = "You should create your blog first.";
            return View("Error");
        }

        model.Categories = await _db.Categories.Where(d => d.Owner == user).ToListAsync();
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Blogger")]
    public async Task<IActionResult> Write(BlogWriteViewModel model)
    {
        BlogUser? user;

        if (!ModelState.IsValid)
        {
            user = await _db.Users.SingleOrDefaultAsync(x => x.Id == _userManager.GetUserId(User));
            model.Categories = await _db.Categories.Where(d => d.Owner == user).ToListAsync();
            return View(model);
        }

        user = await _db.Users
            .Include(x => x.Blog)
            .ThenInclude(i => i!.Articles)
            .Include(x => x.Blog)
            .ThenInclude(i => i!.Tags)
            .SingleOrDefaultAsync(x => x.Id == _userManager.GetUserId(User));

        // Blog
        if (user!.Blog == null)
        {
            ModelState.AddModelError("Title", "You should create your blog first.");
            return View(model);
        }

        // Category
        model.Categories = await _db.Categories.Where(d => d.Owner == user).ToListAsync();
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == model.CategoryId && c.Owner == user);
        if (category == null)
        {
            ModelState.AddModelError("CategoryId", "You selected a category that isn't set by you.");
            return View(model);
        }

        category.Count++;

        // URL
        var m = Regex.Match(model.Url!, @"^[\w-]+$", RegexOptions.IgnoreCase);
        if (!m.Success)
        {
            ModelState.AddModelError("Url", "There's an unsupported letter in your address.");
            return View(model);
        }

        if (await _db.Articles.SingleOrDefaultAsync(a => a.Author == user && a.Url == model.Url) != null)
        {
            ModelState.AddModelError("Url", "There's a duplicate article address on your blog.");
            return View(model);
        }

        var article = _mapper.Map<Article>(model);
        article.Author = user;
        article.PostDate = DateTime.UtcNow;
        article.Category = category;

        // Tags
        if (model.Tags != null)
        {
            user.Blog.Tags ??= new List<Tag>();
            article.Tags ??= new List<Tag>();
            foreach (string s in model.Tags.Split(','))
            {
                var tagName = s.Trim().ToUpper();
                if (String.IsNullOrWhiteSpace(tagName))
                {
                    continue;
                }
                Tag newTag;
                var oldTag = await _db.Tags.SingleOrDefaultAsync(t => t.Name == tagName);
                if (oldTag != null)
                {
                    oldTag.Count++;
                    newTag = oldTag;
                }
                else
                {
                    newTag = new Tag { Name = tagName, Count = 1 };
                }

                user.Blog.Tags.Add(newTag);
                article.Tags.Add(newTag);
            }
        }

        user.Blog.Articles ??= new List<Article>();
        user.Blog.Articles.Add(article);
        _repository.CreateArticle(article);
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Blogger")]
    public IActionResult Edit(int id)
    {
        return View();
    }

    public IActionResult View(string blogAddress, string articleUrl)
    {
        return View();
    }

    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(BlogCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        model.BlogAddress = model.BlogAddress.ToLower();
        var addressBlackList = new[]
            { "create", "view", "cancel", "edit", "manage", "delete", "post", "jake", "admin", "view" };
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
            ModelState.AddModelError("BlogAddress",
                "This address is already used by someone. Try with another one.");
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
        var defaultCategory = new Category
            { Name = "General", Count = 0, Owner = user, CategoryType = CategoryTypeEnum.View };
        blog.DefaultCategory = defaultCategory;
        _repository.CreateBlog(blog);
        blog.Categories = new List<Category>
            { defaultCategory };
        _repository.Commit(); // have to save separately in order to avoid a circular dependency error about categories.
        await _userManager.AddToRoleAsync(user, Roles.Blogger.ToString());
        await _signInManager.RefreshSignInAsync(user); // to refresh the role claim
        return RedirectToAction("CreateComplete");
    }

    public IActionResult CreateComplete()
    {
        ViewBag.SuccessMessage = "You created a new blog!";
        return View();
    }

    [Authorize(Roles = "Blogger")]
    public IActionResult Manage()
    {
        return View();
    }
}