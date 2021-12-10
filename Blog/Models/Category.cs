using Blog.Areas.Identity.Data;

namespace Blog.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Count { get; set; }
    public BlogUser Owner { get; set; } = null!;
}