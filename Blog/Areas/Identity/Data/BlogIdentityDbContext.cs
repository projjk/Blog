using Blog.Areas.Identity.Data;
using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Blog = Blog.Models.Blog;

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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // seed user
        var user = new BlogUser
        {
            Id = "7cafdc8c-dbb4-42d7-877d-534bb57998c6",
            FullName = "Jake Jeon",
            DisplayName = "Jakal",
            UserName = "test@local.com",
            NormalizedUserName = "TEST@LOCAL.COM",
            Email = "test@local.com",
            NormalizedEmail = "TEST@LOCAL.COM",
            EmailConfirmed = true,
            PasswordHash = "AQAAAAEAACcQAAAAEOkocHMDnr1m69CBryi0B9SRqdaELZn9MSI1rZ4QtNruKGUrBxf/oM7vLibo1CmpcQ==",
            SecurityStamp = "UDAQGSO7AXESNGWWZM5CJYTVR47QG6LP",
            ConcurrencyStamp = "ae0fe4bd-cd57-450c-abfd-3426bf1102aa",
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0
        };
    
        builder.Entity<BlogUser>().HasData(user);

        builder.Entity<Models.Blog>().HasData(new Models.Blog
        {
            Id = 1,
            OwnerForeignKey = "7cafdc8c-dbb4-42d7-877d-534bb57998c6",
            IsHidden = false,
            VisitorCounter = 0,
            BlogTitle = "In the Matrix",
            BlogAddress = "jake"
        });

        builder.Entity<Category>().HasData(new 
        {
            Id = 1,
            Name = "General",
            Count = 0,
            OwnerId = "7cafdc8c-dbb4-42d7-877d-534bb57998c6",
            BlogId = 1
        });
    }
}
