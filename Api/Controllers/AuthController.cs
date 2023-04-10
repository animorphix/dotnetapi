using Microsoft.AspNetCore.Mvc;
using Api.Services;
using Api.Models;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AuthService _authService;


    public AuthController(UserService userService, AuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    [HttpPost]
    public async Task RegisterUser(CreateUserModel model) 
    {
        if (await _userService.CheckUserExists(model.Email))
        {
            throw new Exception ("User already exists");
        }
        await _userService.CreateUser(model);
    }

    [HttpPost]
    public async Task<TokenModel> Token(TokenRequestModel model) 
        => await _authService.GetToken(model.Login, model.Password);

    [HttpPost]
    public async Task<TokenModel> RefreshToken(RefreshTokenRequestModel model) 
        => await _authService.GetTokenByRefreshToken(model.RefreshToken);

}
