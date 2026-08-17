using GomexPraksa.ApplicationUserSecurity;
using GomexPraksa.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
//using Models.AuthenticationDtos;

namespace GomexPraksa.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _service;
    /* public AuthController(UserManager<ApplicationUser> userManager, IJwtService service)
     {
         _userManager = userManager;
         _service = service;
     }
     [HttpPost("Register")]
     public async Task<IActionResult> CreateAccountAsync(RegisterDTO dto)
     {
         var existingUser =
             await _userManager.FindByEmailAsync(dto.Email);

         if (existingUser is not null)
         {
             return Conflict(new
             {
                 message = "Korisnik sa ovim emailom već postoji."
             });
         }

         var user = new ApplicationUser
         {
             UserName = dto.Email,
             Email = dto.Email
         };

         var createResult = await _userManager.CreateAsync(
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

         // Mora biti ista rola kao u RoleSeeder-u
         const string defaultRole = "Komercijalista";

         var roleResult = await _userManager.AddToRoleAsync(
             user,
             defaultRole
         );

         if (!roleResult.Succeeded)
         {
             // Da ne ostane kreiran korisnik bez role
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




     */
}