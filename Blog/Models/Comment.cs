using System.ComponentModel.DataAnnotations;
using Blog.Areas.Identity.Data;

namespace Blog.Models;

public class Comment
{
    public int Id { get; set; }
    [StringLength(1500)]
    public string Body { get; set; } = null!;
    public DateTime PostDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public BlogUser? BlogUser { get; set; }
    [StringLength(20)]
    public string Author { get; set; } = null!;
    
    [StringLength(69)]
    public string Password { get; set; } = null!;
}