using Microsoft.Build.Framework;

namespace Blog.ViewModels;

public class BlogDeleteArticle
{
    [Required]
    public string BlogAddress { get; set; } = null!;

    [Required]
    public int ArticleId { get; set; }
}