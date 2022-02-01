using System.ComponentModel.DataAnnotations;
using Blog.Models;

namespace Blog.ViewModels;

public class BlogManage
{
    public bool IsHidden { get; set; }
    [Required(ErrorMessage = "Please enter your blog's title")]
    [Display(Name = "Blog Title")]
    [StringLength(50, MinimumLength = 4)]
    public string BlogTitle { get; set; } = null!;
    
    [Required(ErrorMessage = "Please enter your blog's address")]
    [Display(Name = "Blog Address")]
    [StringLength(16, MinimumLength = 4)]
    [RegularExpression(@"^[\w-]+$", ErrorMessage = "The blog address contains an unsupported letter.")]
    public string BlogAddress { get; set; } = null!;

    [Required]
    public string DefaultCategory { get; set; } = null!;
    public IList<BlogManageCategory> Categories { get; set; } = null!;

    public BlogManageCategory AddCategory { get; set; } = null!;
}