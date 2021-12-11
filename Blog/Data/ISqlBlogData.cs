using Blog.Models;

namespace Blog.Data;

public interface ISqlBlogData
{
    void CreateBlog(Models.Blog blog);
    int Commit();
    void CreateArticle(Article article);
}