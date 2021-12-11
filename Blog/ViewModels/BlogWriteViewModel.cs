using System.ComponentModel.DataAnnotations;
using Blog.Models;

namespace Blog.ViewModels;

public class BlogWriteViewModel
{
    [Required(ErrorMessage = "Please enter your article's address")]
    [Display(Name = "URL")]
    [StringLength(50, MinimumLength = 6)]
    [RegularExpression(@"^[\w-]+$", ErrorMessage = "URL contains an unsupported letter.")]
    public string Url { get; set; } = null!;
    
    [Required(ErrorMessage = "Please enter the title")]
    [StringLength(50, MinimumLength = 2)]
    public string Title { get; set; } = null!;
    
    [Required(ErrorMessage = "Please enter the body")]
    [DataType(DataType.Text)]
    public string Body { get; set; } = null!;
    
    [Required(ErrorMessage = "Please choose a category")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }
    
    [StringLength(50, MinimumLength = 2)]
    public string? Tags { get; set; }
    public List<Category>? Categories { get; set; }
}