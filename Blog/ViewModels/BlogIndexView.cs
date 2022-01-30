using Blog.Areas.Identity.Data;
using Blog.Models;

namespace Blog.ViewModels;

public class BlogIndexView
{
    public BlogUser Owner { get; set; } = null!;
    public bool IsOwner { get; set; }
    public string BlogAddress { get; set; } = null!;
    public int VisitorCounter { get; set; }
    public string BlogTitle { get; set; } = null!;
    public PaginatedList<Article>? Articles { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = null!;
}