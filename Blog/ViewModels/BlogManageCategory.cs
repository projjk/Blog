using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Blog.Areas.Identity.Data;
using Blog.Models;
using Microsoft.AspNetCore.Mvc;

namespace Blog.ViewModels;

public class BlogManageCategory
{
    [Required] [HiddenInput] public int Id { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 1)]
    [Display(Name = "Category Name")]

    public string Name { get; set; } = null!;

    [ReadOnly(true)] public int Count { get; set; }

    [Required] public bool Display { get; set; }
    [Required] 
    [Display(Name = "Category Type")]
    public CategoryTypeEnum CategoryType { get; set; }
    [Required] [Range(1, 10)]
    [Display(Name = "Items Per Page")]
    public int ItemsPerPage { get; set; }
}