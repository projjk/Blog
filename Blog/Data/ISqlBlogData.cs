namespace Blog.Data;

public interface ISqlBlogData
{
    void CreateBlog(Models.Blog blog);
    int Commit();
}