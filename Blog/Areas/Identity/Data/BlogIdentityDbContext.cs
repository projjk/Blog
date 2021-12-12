using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
        const string adminUserId = "7cafdc8c-dbb4-42d7-877d-534bb57998c6";
        const string adminRoleId = "53a23392-63e5-49aa-a04d-6e73726d5e11";
        const string basicRoleId = "aed07505-c967-4c47-b403-1d62e19ea7d0";
        const string bloggerRoleId = "0b19edc3-5c37-4192-a01a-b7b4309fb321";

        base.OnModelCreating(builder);
        
        // seed roles
        builder.Entity<IdentityRole>().HasData(new IdentityRole
        {
            Id = adminRoleId,
            Name = "Admin",
            NormalizedName = "ADMIN",
            ConcurrencyStamp = "71922fd6-6f2b-4f20-8040-22d71b6e984f"
        });
        builder.Entity<IdentityRole>().HasData(new IdentityRole
        {
            Id = basicRoleId,
            Name = "Basic",
            NormalizedName = "BASIC",
            ConcurrencyStamp = "336671d9-503c-4522-baed-dad8a8bab86a"
        });
        builder.Entity<IdentityRole>().HasData(new IdentityRole
        {
            Id = bloggerRoleId,
            Name = "Blogger",
            NormalizedName = "BLOGGER",
            ConcurrencyStamp = "1bc3a18c-32c1-494d-b69d-9a74f4795ac0"
        });

        // seed user
        var user = new BlogUser
        {
            Id = adminUserId,
            FullName = "Jake Jeon",
            DisplayName = "Jake",
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

        // add roles to the user
        builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        {
            RoleId = adminRoleId,
            UserId = adminUserId
        });
        builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        {
            RoleId = basicRoleId,
            UserId = adminUserId
        });
        builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        {
            RoleId = bloggerRoleId,
            UserId = adminUserId
        });

        // seed a default blog
        builder.Entity<Models.Blog>().HasData(new Models.Blog
        {
            Id = 1,
            OwnerForeignKey = adminUserId,
            IsHidden = false,
            VisitorCounter = 0,
            BlogTitle = "In the Matrix",
            BlogAddress = "jake"
        });

        // seed a default category
        builder.Entity<Category>().HasData(new 
        {
            Id = 1,
            Name = "General",
            Count = 0,
            OwnerId = adminUserId,
            BlogId = 1,
            IsHidden = false,
            CategoryType = CategoryTypeEnum.View
        });
    }
}
