using EcommerceApi.Api.Data;
using EcommerceApi.Api.Dtos;
using EcommerceApi.Api.Models;
using EcommerceApi.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ECommerceDbContext _db;

    public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService, ECommerceDbContext db)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _db = db;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if(!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, "Customer");

        _db.Customers.Add(new Customer
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserId = user.Id
        });
        await _db.SaveChangesAsync();

        var token = await _tokenService.GenerateTokenAsync(user);
        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            Roles = new List<string> { "Customer" }
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { error = "Invalid email or password."});

        var roles = await _userManager.GetRolesAsync(user);
        var token = await _tokenService.GenerateTokenAsync(user);

        return Ok( new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles.ToList()
        });
    }
}