using Blog.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data;

public class SqlBlogData : ISqlBlogData
{
    private readonly BlogIdentityDbContext _db;

    public SqlBlogData(BlogIdentityDbContext db)
    {
        _db = db;
    }

    public void CreateBlog(Models.Blog blog)
    {
        _db.Blogs.Add(blog);
        Commit();
    }

    public int Commit()
    {
        return _db.SaveChanges();
    }
}