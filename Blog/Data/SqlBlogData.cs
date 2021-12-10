using Blog.Areas.Identity.Data;

namespace Blog.Data;

public class SqlBlogData : ISqlBlogData
{
    private readonly BlogIdentityDbContext _db;

    public SqlBlogData(BlogIdentityDbContext db)
    {
        _db = db;
    }

    public int Commit()
    {
        return _db.SaveChanges();
    }
}