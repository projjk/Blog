using System.ComponentModel.DataAnnotations;
using Blog.Areas.Identity.Data;

namespace Blog.Models;

public class Comment
{
    public int Id { get; set; }
    public string Body { get; set; } = null!;
    public DateTime PostDate { get; set; }
    public BlogUser? BlogUser { get; set; }
    public string Author { get; set; } = null!;
    
    [StringLength(20)]
    public string? Password { get; set; }
}