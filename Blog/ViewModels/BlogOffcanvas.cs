using Blog.Areas.Identity.Data;
using Blog.Models;

namespace Blog.ViewModels;

public class BlogOffcanvas
{
    public string OwnerDisplayName { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = null!;
    public string BlogAddress { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public string BlogTitle { get; set; } = null!;
    public int VisitorCounter { get; set; }
    public bool IsOwner { get; set; }
}