using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
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
using Microsoft.AspNetCore.Cryptography.KeyDerivation;


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
            blog = await _db.Blogs
                .Where(b => EF.Functions.Collate(b.BlogAddress, "case_insensitive") == blogAddress)
                .Include(b => b.Owner)
                .Include(b => b.Categories)
                .Include(b => b.DefaultCategory)
                .FirstOrDefaultAsync();
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
        else if (category.Equals("all", StringComparison.InvariantCultureIgnoreCase))
        {
            currentCategory = new Category
                { CategoryType = CategoryTypeEnum.List, ItemsPerPage = 10, Name = "All", Owner = blog.Owner };
        }
        else
        {
            currentCategory = blog.Categories.FirstOrDefault(c => String.Equals(c.Name, category, StringComparison.InvariantCultureIgnoreCase)) ?? blog.DefaultCategory;
        }

        // Is the current logged in user the owner?
        bool IsOwner = await _db.Users.Include(x => x.Blog)
            .SingleOrDefaultAsync(x => x.Id == _userManager.GetUserId(User)) == blog.Owner;

        // Pagination
        IQueryable<Article>? blogArticles;
        if (currentCategory.Name.Equals("All"))
        {
            blogArticles = _db.Entry(blog).Collection(x => x.Articles)
                .Query()
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.Comments)
                .Include(a => a.Tags)
                .OrderByDescending(a => a.PostDate)
                .AsNoTracking();     
        }
        else
        {
            blogArticles = _db.Entry(blog).Collection(x => x.Articles)
                .Query()
                .Where(a => EF.Functions.Collate(a.Category.Name, "case_insensitive") == currentCategory.Name)
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.Comments)
                .Include(a => a.Tags)
                .OrderByDescending(a => a.PostDate)
                .AsNoTracking();          
        }
        

        var articles = await PaginatedList<Article>
            .CreateAsync(blogArticles, page ?? 1, currentCategory.ItemsPerPage);

        // viewtype
        var model = _mapper.Map<BlogIndexView>(blog);
        var tempBlog = await _db.Blogs.Include(b => b.Categories.OrderBy(c => c.Id))
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == blog.Id); // to use OrderBy here I had to use .AsNoTracking() so had to make this new variable.
        model.Category = currentCategory;
        model.IsOwner = IsOwner;
        model.Categories = tempBlog!.Categories;

        switch (currentCategory.CategoryType)
        {
            case CategoryTypeEnum.List:
                foreach (var article in articles)
                {
                    article.Body = string.Empty;
                }

                model.Articles = articles;
                return View("IndexList", model);

            case CategoryTypeEnum.View:
                foreach (var article in articles)
                {
                    if (article.Body.Length > 1000)
                    {
                        article.Body = article.Body.Substring(0, 1000);
                    }
                }

                model.Articles = articles;
                return View("IndexView", model);

            case CategoryTypeEnum.Gallery:
                break;
        }

        return View(blog);
    }

    [Authorize(Roles = "Blogger")]
    public async Task<IActionResult> Write(int? articleId)
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

        if (articleId > 0)
        {
            var fetchedArticle = await _db.Articles.Include(a => a.Tags).FirstOrDefaultAsync(a => a.Id == articleId);
            if (IsOwner(user, fetchedArticle))
            {
                _mapper.Map(fetchedArticle, model);

                if (fetchedArticle.Tags != null)
                {
                    var sb = new StringBuilder();
                    foreach (var tag in fetchedArticle.Tags)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(',');
                        }
                        sb.Append(tag.Name);
                    }

                    model.Tags = sb.ToString();
                }
            }
            else
            {
                return View("Error");
            }
        }
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
            .Include(u => u.Blog!.Articles)!
            .ThenInclude(a => a.Tags)
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

        var addressBlackList = new[]
            { "TAG", "CATEGORY", "WRITE", "EDIT", "DELETE" };
        if (addressBlackList.FirstOrDefault(elem => String.Equals(elem, model.Url, StringComparison.OrdinalIgnoreCase)) != null)
        {
            ModelState.AddModelError("Url", "That address cannot be used.");
            return View(model);
        }

        if (await _db.Articles.SingleOrDefaultAsync(a => a.Author == user && a.Url == model.Url && a.Id != model.Id) != null)
        {
            ModelState.AddModelError("Url", "There's a duplicate article address on your blog.");
            return View(model);
        }

        Article? article;
        if (model.Id > 0)
        {
            article = await _db.Articles.Include(a => a.Tags).FirstOrDefaultAsync(a => a.Id == model.Id);
            if (article != null && article.Author == user)
            {
                _mapper.Map(model, article);
                article.LastUpdate = DateTime.UtcNow;
                
                // Update only changes in tags
                if (article.Tags != null) {
                    if (string.IsNullOrWhiteSpace(model.Tags))
                    {
                        RemoveTags(article);
                    }
                    else {
                        var modelTags = model.Tags.Split(',').ToList();
                        foreach (var tag in article.Tags)
                        {
                            // exists in original article, but doesn't in the updated
                            if (!modelTags.Contains(tag.Name))
                            {
                                RemoveTag(article, tag);
                            }
                            // exists in both objects
                            else
                            {
                                modelTags.Remove(tag.Name);
                            }
                        }
                        // now only tags to be added are left.
                        model.Tags = string.Join(',', modelTags);
                    }
                }
            }
            else
            {
                return View("Error");
            }
        }
        else
        {
            article = _mapper.Map<Article>(model);
            article.Author = user;
            article.PostDate = DateTime.UtcNow;
        }

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

                if (article.Tags.FirstOrDefault(t => t.Name == newTag.Name) == null)
                {
                    article.Tags.Add(newTag);
                }
                if (user.Blog.Tags.FirstOrDefault(t => t.Name == newTag.Name) == null)
                {
                    user.Blog.Tags.Add(newTag);
                }
            }
        }

        user.Blog.Articles ??= new List<Article>();

        if (model.Id > 0)
        {
            _repository.Commit();
        }
        else
        {
            user.Blog.Articles.Add(article);
            _repository.CreateArticle(article);
        }
        return RedirectToRoute("blogView",
            new { blogAddress = user.Blog.BlogAddress, articleUrl = article.Url });
    }

    private void RemoveTags(Article article)
    {
        if (article.Tags == null)
        {
            return;
        }
        
        foreach (var tag in article.Tags)
        {
            RemoveTag(article, tag);
        }
    }

    private void RemoveTag(Article article, Tag tag)
    {
        var blog = article.Author.Blog!;
        tag.Count--;
        article.Tags!.Remove(tag);
        if (blog.Articles!.FirstOrDefault(a => a.Tags != null && a.Tags.Contains(tag)) == null)
        {
            blog.Tags!.Remove(tag);
            tag.Blogs.Remove(blog);
            if (tag.Count <= 0)
            {
                _db.Tags.Remove(tag);
                _repository.Commit();
            }
        }
    }

    public async Task<IActionResult> View(string blogAddress, string articleUrl)
    {
        if (TempData["CustomError"] != null)
        {
            ModelState.AddModelError("CommentPassword", TempData["CustomError"]!.ToString()!);

        }

        var article = await _db.Articles
            .Include(a => a.Author.Blog)
            .Include(a => a.Category)
            .Include(a => a.Tags)
            .Include(a => a.Comments!.OrderBy(c => c.Id))
            .Where(a => EF.Functions.Collate(a.Url, "case_insensitive") == articleUrl
                        && EF.Functions.Collate(a.Author.Blog!.BlogAddress, "case_insensitive") == blogAddress)
            .FirstOrDefaultAsync();
        if (article == null)
        {
            ViewBag.ErrorMessage = "There's no article with that address.";
            return View("Error");
        }
        
        var blog = article.Author.Blog;
        var viewModel = _mapper.Map<BlogView>(article);
        _mapper.Map(blog, viewModel);

        // Is the current logged in user the owner?
        viewModel.IsOwner = await _db.Users.Include(x => x.Blog)
            .SingleOrDefaultAsync(x => x.Id == _userManager.GetUserId(User)) == blog!.Owner;

        article.ViewCount++;
        blog!.VisitorCounter++;
        _repository.Commit();
        return View(viewModel);
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
            { "create", "index", "view", "cancel", "edit", "manage", "delete", "post", "jake", "admin", "view" };
        var titleBlackList = new[] { "fuck", "suck" };
        if (addressBlackList.FirstOrDefault(elem => model.BlogAddress.Contains(elem)) != null
            || titleBlackList.FirstOrDefault(elem => model.BlogAddress.Contains(elem)) != null)
        {
            ModelState.AddModelError("BlogAddress", "The address contains a forbidden word.");
            return View(model);
        }

        if (titleBlackList.FirstOrDefault(elem => model.BlogTitle.Contains(elem)) != null)
        {
            ModelState.AddModelError("BlogTitle", "The title contains a forbidden word.");
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
            { Name = "General", Display = true, Count = 0, Owner = user, ItemsPerPage = 3, CategoryType = CategoryTypeEnum.View };
        blog.DefaultCategory = defaultCategory;
        _repository.CreateBlog(blog);
        blog.Categories = new List<Category>
            { defaultCategory };
        _repository.Commit(); // have to save separately in order to avoid a circular dependency error about categories.
        await _userManager.AddToRoleAsync(user, Roles.Blogger.ToString());
        await _signInManager.RefreshSignInAsync(user); // to refresh the role claim
        return RedirectToAction("CreateComplete", new { blogAddress = blog.BlogAddress});
    }

    public IActionResult CreateComplete(string blogAddress)
    {
        ViewBag.blogAddress = blogAddress;
        return View();
    }

    [Authorize(Roles = "Blogger")]
    public async Task<IActionResult> Manage()
    {
        var user = await _db.Users
            .Include(x => x.Blog)
            .ThenInclude(b => b.Categories.OrderBy(c => c.Id))
            .FirstOrDefaultAsync(x => x.Id == _userManager.GetUserId(User));
        var blog = user!.Blog;
        var model = _mapper.Map<BlogManage>(blog);
        // using viewmodel for categories
        IList<BlogManageCategory> categories = new List<BlogManageCategory>();
        foreach (var blogCategory in blog.Categories)
        {
            categories.Add(_mapper.Map<BlogManageCategory>(blogCategory));
        }

        model.Categories = categories;
        model.DefaultCategory = blog!.DefaultCategory.Id.ToString();
        model.AddCategory = new BlogManageCategory { Display = true, CategoryType = CategoryTypeEnum.View, ItemsPerPage = 3 };
        return View(model);
    }
    
    
    [Authorize(Roles = "Blogger")]
    [HttpPost]
    public async Task<IActionResult> Manage(BlogManage model)
    {
        // if not adding a new category, exclude it from validation.
        if (string.IsNullOrWhiteSpace(model.AddCategory.Name))
        {
            ModelState.Remove("AddCategory.Name");
        }
        
        if (!ModelState.IsValid)
        {
            ViewBag.ErrorMessage = "Don't even try :)";
            return View("Error");
        }

        var user = await _db.Users
            .Include(x => x.Blog)
            .ThenInclude(b => b.Categories)
            .FirstOrDefaultAsync(x => x.Id == _userManager.GetUserId(User));
        var blog = user!.Blog;
        _mapper.Map(model, blog);
        // validate category ownership
        foreach (var category in model.Categories)
        {
            var fetchedCategory = blog!.Categories.FirstOrDefault(c => c.Id == category.Id);
            if (fetchedCategory == null)
            {
                ViewBag.ErrorMessage = "Don't even try :)";
                return View("Error");
            }

            _mapper.Map(category, fetchedCategory);
        }
        // validate default category
        if (!int.TryParse(model.DefaultCategory, out int categoryId))
        {
            ViewBag.ErrorMessage = "Don't even try :)";
            return View("Error");
        }
        var defaultCategory = blog!.Categories.FirstOrDefault(c => c.Id == categoryId);
        if (defaultCategory == null)
        {
            ViewBag.ErrorMessage = "Don't even try :)";
            return View("Error");
        }
        
        //add a new category
        if (!string.IsNullOrWhiteSpace(model.AddCategory.Name))
        {
            var newCategory = _mapper.Map<Category>(model.AddCategory);
            newCategory.Owner = user;
            _db.Categories.Add(newCategory);
            blog.Categories.Add(newCategory);
        }

        blog.DefaultCategory = defaultCategory;
        _repository.Commit();
        return RedirectToAction("Manage");
    }
    
    

    [HttpPost]
    public async Task<IActionResult> WriteComment(BlogWriteComment blogWriteComment)
    {
        if (!ModelState.IsValid)
        {
            // for debugging
            // var errors = ModelState
            //     .Where(x => x.Value.Errors.Count > 0)
            //     .Select(x => new { x.Key, x.Value.Errors })
            //     .ToArray();
            return RedirectToRoute("blogView",
                new { blogAddress = blogWriteComment.BlogAddress, articleUrl = blogWriteComment.ArticleUrl });
        }

        // Modifying a comment
        if (blogWriteComment.CommentId > 0)
        {
            var fetched = await _db.Comments.FirstOrDefaultAsync(c => c.Id == blogWriteComment.CommentId);
            if (fetched != null && VerifyHashedPassword(fetched.Password, blogWriteComment.CommentPassword))
            {
                // Deleting a comment
                if (blogWriteComment.CommentDelete)
                {
                    _db.Comments.Remove(fetched);
                    TempData["CustomSuccess"] = "Successfully deleted the comment.";
                }
                // No, just editing
                else
                {
                    fetched.Body = blogWriteComment.CommentBody;
                    fetched.LastUpdate = DateTime.UtcNow;
                    TempData["CustomSuccess"] = "Successfully edited the comment.";
                }
            }
            else
            {
                TempData["CustomError"] = "The password does not match.";

                return RedirectToRoute("blogView",
                    new { blogAddress = blogWriteComment.BlogAddress, articleUrl = blogWriteComment.ArticleUrl });
            }
        }
        // Adding a comment
        else
        {
            var newComment = new Comment
            {
                Author = blogWriteComment.CommentAuthor,
                Password = HashPassword(blogWriteComment.CommentPassword),
                Body = blogWriteComment.CommentBody,
                PostDate = DateTime.UtcNow
            };
            var article = await _db.Articles.Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.Id == blogWriteComment.ArticleId);
            if (article == null)
            {
                return View("Error");
            }

            article.Comments!.Add(newComment);
            TempData["CustomSuccess"] = "Successfully added a comment.";
        }

        _repository.Commit();

        return RedirectToRoute("blogView",
            new { blogAddress = blogWriteComment.BlogAddress, articleUrl = blogWriteComment.ArticleUrl });
    }

    private string HashPassword(string password)
    {
        const KeyDerivationPrf Pbkdf2Prf = KeyDerivationPrf.HMACSHA256;
        const int Pbkdf2IterCount = 100000;
        const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits
        const int SaltSize = 128 / 8; // 128 bits
        
        // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
        byte[] salt = new byte[SaltSize];
        using (var rngCsp = RandomNumberGenerator.Create())
        {
            rngCsp.GetNonZeroBytes(salt);
        }

        // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
        byte[] subkey = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: Pbkdf2Prf,
            iterationCount: Pbkdf2IterCount,
            numBytesRequested: Pbkdf2SubkeyLength);

        var outputBytes = new byte[1 + SaltSize + Pbkdf2SubkeyLength];
        outputBytes[0] = 0x00; // format marker
        Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);
        Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SaltSize, Pbkdf2SubkeyLength);
        return Convert.ToBase64String(outputBytes);
    }

    private bool VerifyHashedPassword(string hashedPasswordString, string password)
    {
        const KeyDerivationPrf Pbkdf2Prf = KeyDerivationPrf.HMACSHA256;
        const int Pbkdf2IterCount = 100000;
        const int Pbkdf2SubkeyLength = 256 / 8; // 256 bits
        const int SaltSize = 128 / 8; // 128 bits

        byte[] hashedPassword = Convert.FromBase64String(hashedPasswordString);

        // We know ahead of time the exact length of a valid hashed password payload.
        if (hashedPassword.Length != 1 + SaltSize + Pbkdf2SubkeyLength)
        {
            return false; // bad size
        }

        byte[] salt = new byte[SaltSize];
        Buffer.BlockCopy(hashedPassword, 1, salt, 0, salt.Length);

        byte[] expectedSubkey = new byte[Pbkdf2SubkeyLength];
        Buffer.BlockCopy(hashedPassword, 1 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

        // Hash the incoming password and verify it
        byte[] actualSubkey = KeyDerivation.Pbkdf2(password, salt, Pbkdf2Prf, Pbkdf2IterCount, Pbkdf2SubkeyLength);
        return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
    }

    
    [HttpPost]
    [Authorize(Roles = "Blogger")]
    public async Task<IActionResult> Delete(BlogDeleteArticle model)
    {
        if (!ModelState.IsValid)
        {
            return View("Error");
        }

        var user = (await _db.Users.FirstOrDefaultAsync(x => x.Id == _userManager.GetUserId(User)))!;
        var article = await _db.Articles
            .Include(a => a.Tags)
            .Include(a => a.Comments)
            .Include(a => a.Category)
            .Include(a => a.Author.Blog)
            .ThenInclude(b => b!.Articles)!
            .Include(a => a.Author.Blog)
            .ThenInclude(b => b!.Tags)
            .FirstOrDefaultAsync(a => a.Id == model.ArticleId);

        if (IsOwner(user, article))
        {
            // tag removal
            RemoveTags(article);

            // comment removal
            if (article.Comments != null)
            {
                foreach (var comment in article.Comments)
                {
                    _db.Comments.Remove(comment);
                }
                article.Comments = null;
            }

            // article removal
            _db.Articles.Remove(article);
            article.Category.Count--;

            _repository.Commit();
        }
        else
        {
            return View("Error");
        }

        return RedirectToRoute("blog",
            new { blogAddress = model.BlogAddress });
    }

    [HttpPost]
    [Authorize(Roles = "Blogger")]
    public async Task<IActionResult> DeleteCategory(BlogDeleteCategory model)
    {
        if (!ModelState.IsValid)
        {
            return View("Error");
        }

        var user = (await _db.Users
            .Include(u => u.Blog)
            .ThenInclude(b => b.Categories)
            .FirstOrDefaultAsync(x => x.Id == _userManager.GetUserId(User)))!;
        var blog = user.Blog!;

        // server-side validation
        var fetchedCategory = blog.Categories.FirstOrDefault(c => c.Id == model.CategoryId);
        if (fetchedCategory == null || blog.DefaultCategory == fetchedCategory)
        {
            return RedirectToAction("Manage");
        }

        // Articles in that category will be moved to the default category.
        if (fetchedCategory.Count > 0)
        {
            var articlesInTheCategory = _db.Articles.Where(a => a.Category == fetchedCategory);
            foreach (var article in articlesInTheCategory)
            {
                article.Category = blog.DefaultCategory;
            }

            _repository.Commit();
        }

        blog.Categories.Remove(fetchedCategory);
        _db.Categories.Remove(fetchedCategory);
        _repository.Commit();

        return RedirectToAction("Manage");
    }

    private static bool IsOwner(BlogUser user, [NotNullWhen(true)] Article? article)
    {
        if (article == null)
        {
            return false;
        }
        
        return article.Author == user;
    }
 private static bool IsOwner(BlogUser user, [NotNullWhen(true)] Models.Blog? blog)
    {
        if (blog == null)
        {
            return false;
        }
        
        return blog.Owner == user;
    }

}