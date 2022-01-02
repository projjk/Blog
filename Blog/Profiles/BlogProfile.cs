using AutoMapper;
using Blog.Models;
using Blog.ViewModels;

namespace Blog.Profiles;

public class BlogProfile : Profile
{
    public BlogProfile()
    {
        CreateMap<Models.Blog, BlogCreateViewModel>()
            .ReverseMap();
        CreateMap<Article, BlogWriteViewModel>()
            .ReverseMap().ForMember(a => a.Tags, opt => opt.Ignore());
        CreateMap<Models.Blog, BlogIndexIndex>()
            .ForMember(a => a.Articles, opt => opt.Ignore())
            .ReverseMap();
    }
}