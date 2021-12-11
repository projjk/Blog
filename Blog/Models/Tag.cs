using System.ComponentModel.DataAnnotations;

namespace Blog.Models;

public class Tag
{
    public int Id { get; set; }
    
    [StringLength(20)]
    public string Name { get; set; } = null!;
    public int Count { get; set; }
    public ICollection<Article> Articles { get; set; } = null!;
    public ICollection<Blog> Blogs { get; set; } = null!;
}