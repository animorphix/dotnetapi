using Microsoft.AspNetCore.Mvc;
using Api.Services;
using Api.Models;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;


    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<TokenModel> Token(TokenRequestModel model) 
        => await _userService.GetToken(model.Login, model.Password);

}
