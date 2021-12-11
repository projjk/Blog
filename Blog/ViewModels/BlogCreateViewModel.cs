using System.ComponentModel.DataAnnotations;
using Blog.Areas.Identity.Data;

namespace Blog.ViewModels;

public class BlogCreateViewModel
{
    [Required(ErrorMessage = "Please enter your blog's title")]
    [Display(Name = "Blog Title")]
    [StringLength(50, MinimumLength = 4)]
    public string BlogTitle { get; set; } = null!;

    [Required(ErrorMessage = "Please enter your blog's address")]
    [Display(Name = "Blog Address")]
    [StringLength(16, MinimumLength = 4)]
    public string BlogAddress { get; set; } = null!;
}