using Blog.Areas.Identity.Data;
using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blog.Areas.Identity.Data;

public class BlogIdentityDbContext : IdentityDbContext<BlogUser>
{
    public DbSet<Models.Blog> Blogs { get; set; } = null!;
    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;

    public BlogIdentityDbContext(DbContextOptions<BlogIdentityDbContext> options)
        : base(options)
    {
    }
}
