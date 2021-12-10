using Blog.Areas.Identity.Data;

namespace Blog.Models;

public class Article
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public string Title { get; set; } = null!;
    public DateTime PostDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public BlogUser Author { get; set; } = null!;
    public string Body { get; set; } = null!;
    public int ViewCount { get; set; }
    public Category? Category { get; set; }
    public IEnumerable<Tag>? Tags { get; set; }
    public IEnumerable<Comment>? Comments { get; set; }
}