using System.ComponentModel.DataAnnotations;
using Blog.Areas.Identity.Data;

namespace Blog.Models;

public class Article
{
    public int Id { get; set; }

    [StringLength(50)]
    public string Url { get; set; } = null!;
    
    [StringLength(50)]
    public string Title { get; set; } = null!;
    public DateTime PostDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public BlogUser Author { get; set; } = null!;
    
    [DataType(DataType.Text)]
    public string Body { get; set; } = null!;
    public int ViewCount { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<Tag>? Tags { get; set; }
    public ICollection<Comment>? Comments { get; set; }
}