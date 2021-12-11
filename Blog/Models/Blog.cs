using System.ComponentModel.DataAnnotations.Schema;
using Blog.Areas.Identity.Data;

namespace Blog.Models;

public class Blog
{
    public int Id { get; set; }
    
    public string OwnerForeignKey { get; set; } = null!;

    [ForeignKey("OwnerForeignKey")]
    public BlogUser Owner { get; set; } = null!;

    public bool IsHidden { get; set; }
    public int VisitorCounter { get; set; }
    public string BlogTitle { get; set; } = null!;
    public string BlogAddress { get; set; } = null!;
    public IEnumerable<Article>? Articles { get; set; }
    public IEnumerable<Category> Categories { get; set; } = null!;
    public IEnumerable<Tag>? Tags { get; set; }
}