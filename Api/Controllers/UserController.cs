using Api.Models;
using Api.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DAL;
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

    [HttpPost]
    public async Task CreateUser(CreateUserModel model) 
    {
        if (await _userService.CheckUserExists(model.Email))
        {
            throw new Exception ("User already exists");
        }
        await _userService.CreateUser(model);
    }

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
                System.IO.File.Copy(tempFi.FullName, path, true);
                await _userService.AddAvatarToUser(userId,model,path);
            }
        }

        else throw new Exception("You are not authorized");
    }


    [HttpGet]
    [Authorize]
    public async Task<List<UserModel>> GetUsers() => await _userService.GetUsers();

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
