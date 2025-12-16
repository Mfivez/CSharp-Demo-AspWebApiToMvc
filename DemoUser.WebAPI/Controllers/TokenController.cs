using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DemoUser.BLL.Services.Interfaces;
using DemoUser.WebAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _config;

    public TokenController(IUserService userService, IConfiguration config)
    {
        _userService = userService;
        _config = config;
    }


    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login(LoginRequest request)
    {
        var user = _userService.Authenticate(request.Username, request.Password);
        if (user is null)
            return Unauthorized();

        var key = _config["jwt:key"];

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["jwt:issuer"],
            audience: _config["jwt:audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}