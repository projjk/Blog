using System.ComponentModel.DataAnnotations;
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

    [StringLength(50)]
    public string BlogTitle { get; set; } = null!;
    
    [StringLength(16)]
    public string BlogAddress { get; set; } = null!;

    public Category DefaultCategory { get; set; } = null!;
    public ICollection<Article>? Articles { get; set; }
    public ICollection<Category> Categories { get; set; } = null!;
    public ICollection<Tag>? Tags { get; set; }
}