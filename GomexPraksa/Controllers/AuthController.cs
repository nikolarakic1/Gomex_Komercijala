using GomexPraksa.ApplicationUserSecurity;
using GomexPraksa.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.AuthenticationDtos;
using System.Diagnostics;
using System.Security.Claims;

namespace GomexPraksa.Controllers;

[Authorize(Roles = "Menadzer,SefMenadzera")]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    public async Task<IActionResult> CreateAccountAsync(
        RegisterDTO dto)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(dto.Email);

        if (existingUser is not null)
        {
            return Conflict(new
            {
                message =
                    "Korisnik sa ovim emailom već postoji."
            });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var createResult =
            await _userManager.CreateAsync(
                user,
                dto.Password
            );

        if (!createResult.Succeeded)
        {
            return BadRequest(new
            {
                errors = createResult.Errors
                    .Select(error => error.Description)
            });
        }

        const string defaultRole = "Menadzer";

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                defaultRole
            );

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return BadRequest(new
            {
                errors = roleResult.Errors
                    .Select(error => error.Description)
            });
        }

        return Ok(new
        {
            message = "Korisnik uspešno kreiran."
        });
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> LogInAsync(
        LogInDto dto)
    {
        var total = Stopwatch.StartNew();

        var sw = Stopwatch.StartNew();

        var user =
            await _userManager.FindByEmailAsync(
                dto.Email
            );

        Console.WriteLine(
            $"FindByEmailAsync: {sw.ElapsedMilliseconds} ms"
        );

        if (user is null)
        {
            return Unauthorized(
                "Pogrešan email ili lozinka."
            );
        }

        sw.Restart();

        var validPassword =
            await _userManager.CheckPasswordAsync(
                user,
                dto.Passsword
            );

        Console.WriteLine(
            $"CheckPasswordAsync: {sw.ElapsedMilliseconds} ms"
        );

        if (!validPassword)
        {
            return Unauthorized(
                "Pogrešan email ili lozinka."
            );
        }

        sw.Restart();

        var roles =
            await _userManager.GetRolesAsync(user);

        Console.WriteLine(
            $"GetRolesAsync: {sw.ElapsedMilliseconds} ms"
        );

        sw.Restart();

        var token =
            _jwtService.GenerateToken(
                user,
                roles
            );

        Console.WriteLine(
            $"GenerateToken: {sw.ElapsedMilliseconds} ms"
        );

        Console.WriteLine(
            $"UKUPAN LOGIN: {total.ElapsedMilliseconds} ms"
        );

        return Ok(new
        {
            token
        });
    }

    [HttpGet("UserCredentials")]
    public IActionResult GetCredentials()
    {
        var ime =
            User.FindFirstValue(ClaimTypes.Name);

        var role =
            User.FindFirstValue(ClaimTypes.Role);

        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        var email =
            User.FindFirstValue(ClaimTypes.Email);

        return Ok(new
        {
            ime,
            role,
            userId,
            email
        });
    }
}