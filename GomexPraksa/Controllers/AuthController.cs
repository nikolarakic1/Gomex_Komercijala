using GomexPraksa.ApplicationUserSecurity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.AuthenticationDtos;

namespace GomexPraksa.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    public AuthController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    [HttpPost("Register")]
    public async Task<IActionResult> CreateAccountAsync(RegisterDTO dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await _userManager.CreateAsync(
            user,
            dto.Password
        );

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(
            user,
            dto.Role
        );

        return Ok("Korisnik uspešno kreiran.");
    }
    [HttpPut("Login")]
    public async Task<IActionResult> LogInAsync(LogInDto dto) 
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return Unauthorized("Greska u Signupu");
        }
        var validPassword = await _userManager.CheckPasswordAsync(user,dto.Passsword);
        if (!validPassword)
        {
            return Unauthorized("Greska u loginu");
        }
        return Ok("Login Uspesan");

    }

}

