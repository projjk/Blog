using Blog.Areas.Identity.Data;

namespace Blog.Models;

public class Blog
{
    public int Id { get; set; }
    public BlogUser Owner { get; set; } = null!;
    public bool IsHidden { get; set; }
    public int VisitorCounter { get; set; }
    public IEnumerable<Article>? Articles { get; set; }
    public IEnumerable<Category> Categories { get; set; } = null!;
    public IEnumerable<Tag>? Tags { get; set; }
}