using Api.Models;
using Api.Models.Post;
using Api.Services;
using Common.Consts;
using Common.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class PostController : ControllerBase
{
    private readonly PostService _postService;

    public PostController(PostService postService)
    {
        _postService = postService;
    }

    [HttpPost]
    public async Task CreatePost(CreatePostRequest createPostRequest)
    {
        var userId = User.GetClaimValue<Guid>(ClaimNames.UserId);
        if (userId == default)
        {
            throw new Exception("User is not authorised");
        }

        var model = new CreatePostModel
        {
            AuthorId = userId,
            Contents = createPostRequest.Contents.Select(x=> 
            new MetaWithPath(x, q=>
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "Attaches", 
                q.TempId.ToString()))).ToList()
        };
        
        model.Contents.ForEach(x => {
            var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), x.TempId.ToString()));
            if (tempFi.Exists)
            {
                var destFi = new FileInfo(x.FilePath);
            }
        });

        await _postService.CreatePost(model);
    }
}
