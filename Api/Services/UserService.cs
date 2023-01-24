using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Configs;
using Api.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services;

public class UserService: IDisposable
{
    private readonly IMapper _mapper;
    private readonly DAL.DataContext _context;
    private readonly AuthConfig _config;

    public UserService (IMapper mapper, IOptions<AuthConfig> config, DataContext context)
    {
        _mapper = mapper;
        _context = context;
        _config = config.Value;
    }


    //Manipulations with users 
    public async Task<Guid> CreateUser(CreateUserModel model)
    {
        var dbUser = _mapper.Map<DAL.Entities.User>(model);
        var t = await _context.Users.AddAsync(dbUser);
        await _context.SaveChangesAsync();
        return t.Entity.Id;
    }

    public async Task<bool> CheckUserExists(string email)
    {
        return await _context.Users.AnyAsync(x=>x.Email.ToLower() == email.ToLower());
    }

    public async Task Delete (Guid id)
    {
        var dbUser = await GetUserById(id);

        if (dbUser != null)
        {
            _context.Users.Remove(dbUser);
            await _context.SaveChangesAsync();
        }
    }


    //User Profile Image manipulation
    public async Task AddAvatarToUser(Guid userId, MetaDataModel meta, string filePath)
    {
        var user = await _context.Users.Include(x=>x.Avatar).FirstOrDefaultAsync(x=>x.Id==userId);
        if (user != null)
        {
            var avatar = new Avatar
            { 
                Author = user, 
                MimeType = meta.MimeType, 
                FilePath = filePath, 
                Name = meta.Name, 
                Size = meta.Size,
            };
            
            user.Avatar = avatar;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<AttachModel> GetUserAvatar(Guid userId)
    {
        var user = await GetUserById(userId);
        var attach = _mapper.Map<AttachModel>(user.Avatar);
        return attach;
    }


    //Getting users
    public async Task<List<UserModel>> GetUsers()
    {
        return await _context.Users.AsNoTracking().ProjectTo<UserModel>(_mapper.ConfigurationProvider).ToListAsync();
    }
    
    public async Task<UserModel> GetUser(Guid id)
    {
        var user = await GetUserById(id);
        return _mapper.Map<UserModel>(user);
    }

    private async Task<DAL.Entities.User> GetUserById(Guid id)
    {
        var user = await _context.Users.Include(x=>x.Avatar).FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            throw new Exception("User Not Found");
        }

        return user;
    }

    private async Task<DAL.Entities.User> GetUserByCredentials(string login, string password) 
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

    
    //Token generation and auth
    private TokenModel GenerateTokens(DAL.Entities.UserSession session)
    {
        if (session.User == null)
        {
            throw new Exception("session.user==null");
        }
        var jwt = new JwtSecurityToken(
            issuer: _config.Issuer,
            audience: _config.Audience,
            notBefore: DateTime.Now,
            claims: new Claim[] 
            {
            new Claim(ClaimsIdentity.DefaultNameClaimType, session.User.Name),
            new Claim("userId", session.User.Id.ToString()),
            new Claim("SessionId", session.Id.ToString())
            },
            expires: DateTime.Now.AddMinutes(_config.LifeTime),
            signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
        );
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        var refresh = new JwtSecurityToken(
            notBefore: DateTime.Now,
            claims: new Claim[] 
            {
            new Claim("refreshToken", session.RefreshToken.ToString()),
            },
            expires: DateTime.Now.AddHours(_config.LifeTime),
            signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
        );
        var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refresh);

        return new TokenModel(encodedJwt, encodedRefresh);
    }
    
    public async Task<TokenModel> GetToken (string login, string password)
    {
        var user = await GetUserByCredentials(login,password);
        var session = await  _context.UserSessions.AddAsync(new DAL.Entities.UserSession 
        {
            User = user, 
            RefreshToken = Guid.NewGuid(), 
            Created = DateTime.UtcNow, 
            Id = Guid.NewGuid(),
        });
        await _context.SaveChangesAsync();
        return GenerateTokens(session.Entity);
    }
    
    public async Task<TokenModel> GetTokenByRefreshToken(string refreshToken)
    {
        var validParams = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = _config.SymmetricSecurityKey()
        };

        //?????????? What is principal
        var principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validParams, out var securityToken);
         
        ///????????
        if (securityToken is not JwtSecurityToken jwtToken 
        || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid Token");
        }

        if (principal.Claims.FirstOrDefault(x=>x.Type=="refreshToken")?.Value is String refreshIdString
            && Guid.TryParse(refreshIdString, out var refreshId))
        {
            var session = await GetSessionByrefreshToken(refreshId);
            if(!session.IsActive)
            {
                throw new Exception("Session not active");
            }
            var user = session.User;
            session.RefreshToken = Guid.NewGuid();
            await _context.SaveChangesAsync();
            return GenerateTokens(session);
        }

        else
        {
            throw new SecurityTokenException("Invalid Token");
        }
    }

   
   //Sessions management
    public async Task<UserSession> GetSessionById (Guid userId)
    {
        var session = await _context.UserSessions.FirstOrDefaultAsync(x => x.Id == userId);
        
        if(session == null)
        {
            throw new Exception("Session not found");
        }
        else
        {
            return session;
        }
        
    }

    private async Task<UserSession> GetSessionByrefreshToken (Guid id)
    {
        var session = await _context.UserSessions.Include(x=>x.User).FirstOrDefaultAsync(x => x.RefreshToken == id);
        
        if(session == null)
        {
            throw new Exception("Session not found");
        }
        else
        {
            return session;
        }
        
    }



    public void Dispose()
    {
        _context.Dispose();
    }
}

