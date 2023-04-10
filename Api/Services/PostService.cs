using Api.Models.Post;
using AutoMapper;
using DAL;
using DAL.Entities;

namespace Api.Services;

public class PostService
{
    private readonly IMapper _mapper;
    private readonly DAL.DataContext _context;

    public PostService(IMapper mapper, DataContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task CreatePost(CreatePostModel createPostModel)
    {
        var dbModel = _mapper.Map<Post>(createPostModel);
    }
}
