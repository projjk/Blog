using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels;

public class BlogWriteComment
{
    [Required]
    public int ArticleId { get; set; }
    [Required]
    public string ArticleUrl { get; set; } = null!;
    [Required]
    public string BlogAddress { get; set; } = null!;
    [Required(ErrorMessage = "Please enter your name.")]
    [StringLength(20)]
    public string CommentAuthor { get; set; } = null!;

    [Required(ErrorMessage = "Please enter the password for the comment.")]
    [StringLength(50)]
    public string CommentPassword { get; set; } = null!;

    [Required(ErrorMessage = "Please enter the comment body.")]
    [StringLength(1500)]
    public string CommentBody { get; set; } = null!;
}