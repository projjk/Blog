using Microsoft.Build.Framework;

namespace Blog.ViewModels;

public class BlogDeleteCategory
{
    [Required]
    public int CategoryId { get; set; }
}