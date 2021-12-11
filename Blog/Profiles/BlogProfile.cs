using AutoMapper;
using Blog.ViewModels;

namespace Blog.Profiles;

public class BlogProfile : Profile
{
    public BlogProfile()
    {
        CreateMap<Models.Blog, BlogCreateViewModel>()
            .ReverseMap();
    }
}