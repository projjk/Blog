using System.ComponentModel.DataAnnotations;
using Blog.Areas.Identity.Data;
using Blog.Models;

namespace Blog.ViewModels;

public class BlogView
{
    // Blog Properties
    public BlogUser Owner { get; set; } = null!;
    public string BlogAddress { get; set; } = null!;
    public int VisitorCounter { get; set; }
    public string BlogTitle { get; set; } = null!;
    
    // Article Properties
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public string Title { get; set; } = null!;
    public DateTime PostDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public Category Category { get; set; } = null!;
    public string Body { get; set; } = null!;
    public int ViewCount { get; set; }
    public ICollection<Tag>? Tags { get; set; }
    public ICollection<Comment>? Comments { get; set; }
}