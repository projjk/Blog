using Blog.Areas.Identity.Data;
using Blog.Models;

namespace Blog.ViewModels;

public class BlogIndexIndex
{
    public BlogUser Owner { get; set; } = null!;
    public string BlogAddress { get; set; } = null!;
    public int VisitorCounter { get; set; }
    public string BlogTitle { get; set; } = null!;
    public PaginatedList<Article>? Articles { get; set; }
    public Category Category { get; set; } = null!;
}