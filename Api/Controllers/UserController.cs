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
    public async Task CreateUser(CreateUserModel model) => await _userService.CreateUser(model);
//test

    [HttpGet]
    [Authorize]
    public async Task<List<UserModel>> GetUsers() => await _userService.GetUsers();
}
