using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Configs;
using Api.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services;

public class UserService
{
    private readonly IMapper _mapper;
    private readonly DAL.DataContext _context;
    private readonly AuthConfig _config;

    public UserService (IMapper mapper, DataContext context, IOptions<AuthConfig> config)
    {
        _mapper = mapper;
        _context = context;
        _config = config.Value;
    }

    public async Task CreateUser(CreateUserModel model)
    {
        var dbUser = _mapper.Map<DAL.Entities.User>(model);
        await _context.Users.AddAsync(dbUser);
        await _context.SaveChangesAsync();
    }

    public async Task<List<UserModel>> GetUsers()
    {
        return await _context.Users.AsNoTracking().ProjectTo<UserModel>(_mapper.ConfigurationProvider).ToListAsync();
    }

    private async Task<DAL.Entities.User> GetUserById(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            throw new Exception("User Not Found");
        }

        return user;
    }

    public async Task<UserModel> GetUser(Guid id)
    {
        var user = await GetUserById(id);
        return _mapper.Map<UserModel>(user);
    }

    private async Task<DAL.Entities.User> GetUserByCredentials(string login, string password) //GetUserByCredention in tutorial????
    {
        var user = await _context.Users.FirstOrDefaultAsync(x=>x.Email.ToLower() == login.ToLower());

        if (user==null)
        {
            throw new Exception("User Not Found");
        }
        
        if(!HashHelper.Verify(password,user.PasswordHash))
        {
            throw new Exception("Password incorrect");
        }

        return user;
    }

    public async Task<TokenModel> GetToken (string login, string password)
    {
        var user = await GetUserByCredentials(login,password);

        var claims = new Claim[] 
        {
            //new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
            new Claim("displayName", user.Name),
            new Claim("id", user.Id.ToString()),
        };

        var jwt = new JwtSecurityToken(
            issuer: _config.Issuer,
            audience: _config.Audience,
            notBefore: DateTime.Now,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_config.LifeTime),
            signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
        );

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new TokenModel(encodedJwt);
    }
}

