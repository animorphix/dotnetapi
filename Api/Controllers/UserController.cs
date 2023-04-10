using System.Web.Http.Cors;
using Api.Models;
using Api.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;


    public UserController(UserService userService)
    {
        _userService = userService;
    }

    // [HttpPost]
    // public async Task CreateUser(CreateUserModel model) 
    // {
    //     if (await _userService.CheckUserExists(model.Email))
    //     {
    //         throw new Exception ("User already exists");
    //     }
    //     await _userService.CreateUser(model);
    // }

    [HttpPost]
    [Authorize]
    public async Task AddAvatarToUser(MetaDataModel model)
    {
        var userIdString = User.Claims.FirstOrDefault(x=>x.Type =="userId")?.Value;

        if (Guid.TryParse(userIdString, out var userId))
        {
            var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(),model.TempId.ToString()));

            if (!tempFi.Exists)
            {
                throw new Exception("File not found");
            }

            else
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(),"Attaches",model.TempId.ToString());
                var destFi = new FileInfo(path);
                if(destFi.Directory !=null && !destFi.Directory.Exists)
                {
                    destFi.Directory.Create();
                }
                System.IO.File.Copy(tempFi.FullName, path, true);
                await _userService.AddAvatarToUser(userId,model,path);
            }
        }

        else throw new Exception("You are not authorized");
    }

    [HttpGet]
    public async Task<FileResult> GetAvatarById(Guid userId)
    {
        var attach = await _userService.GetUserAvatar(userId);

        return File(System.IO.File.ReadAllBytes(attach.FilePath), attach.MimeType);
    }

    [HttpGet]
    public async Task<FileResult> DownloadAvatarById(Guid userId)
    {
        var attach = await _userService.GetUserAvatar(userId);

        HttpContext.Response.ContentType = attach.MimeType;
        FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(attach.FilePath),attach.MimeType)
        {
            FileDownloadName = attach.Name
        };

        return result;
    } 

    [HttpGet]
    public async Task<IEnumerable<UserAvatarModel>> GetUsers() => await _userService.GetUsers();

    [HttpGet]
    [Authorize]
    public async Task<UserModel> GetCurrentUser() 
    {
        var userIdString = User.Claims.FirstOrDefault(x=>x.Type =="userId")?.Value;

        if (Guid.TryParse(userIdString, out var userId))
        {
            return await _userService.GetUser(userId);
        }
        else throw new Exception("You are not authorized");
    }

}
