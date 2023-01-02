using JwtApiPracitice.Auth;
using JwtApiPracitice.Models;
using JwtApiPracitice.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtApiPracitice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<StudentUser> _userManager;
        private readonly SignInManager<StudentUser> _signInManager;
        private readonly RoleManager<StudentRole> _roleManager;
        private readonly JwtSettings _jwtsettings;
        private const string ROLE_NAME = "JOESTAR";

        public AccountController(UserManager<StudentUser> userManager, SignInManager<StudentUser> signinManager, RoleManager<StudentRole> roleManager, IOptionsSnapshot<JwtSettings> setting)
        {
            _userManager = userManager;
            _signInManager = signinManager;
            _roleManager = roleManager;
            _jwtsettings = setting.Value;
        }

        [HttpPost("signin")]
        public async Task<IActionResult> Signin(UserData userdata)
        {
            var existingUser = await _userManager.FindByNameAsync(userdata.Username);
            if (existingUser is not null) 
            {
                //var signinResult = await _signInManager.PasswordSignInAsync(existingUser, userdata.Password, false, false);
                //if (signinResult.Succeeded)
                //{
                //    return NoContent();
                //}

                //jwt
                var passwordCorrect = await _userManager.CheckPasswordAsync(existingUser, userdata.Password);
                if (passwordCorrect)
                {
                    var roles = await _userManager.GetRolesAsync(existingUser);
                    return Ok(new { key= GenerateJwt(existingUser, roles) });
                }

                return BadRequest($"Incorrect password for user: {userdata.Username}");
            }
            return BadRequest($"User with name: {userdata.Username} doesn't exist in system");
        }
        [HttpPost("signout")]
        public async Task<IActionResult> Signout()
        {
            await _signInManager.SignOutAsync();
            return NoContent();
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserData userdata)
        {
            var existingUser = await _userManager.FindByNameAsync(userdata.Username);
            if (existingUser is null)
            {
                var user = new StudentUser { UserName = userdata.Username };
                var createResult = await _userManager.CreateAsync(user, userdata.Password);
                if (createResult.Succeeded)
                {
                    if (userdata.Username.Contains("joestar"))
                    {
                        await AddRole();
                        await _userManager.AddToRoleAsync(user, ROLE_NAME);
                    }
                    return Ok(user);
                }

                return Problem("Unable to create user!");
            }

            return BadRequest($"User with name: {userdata.Username} have already existed in system");
        }

        private async Task AddRole()
        {
            var role = await _roleManager.FindByNameAsync(ROLE_NAME);
            if (role is null)
            {
                var newRole = new StudentRole() { Name = ROLE_NAME};
                var createRole = await _roleManager.CreateAsync(newRole);
            }
        }

        private string GenerateJwt(StudentUser user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>() {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName)
            };

            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x));
            claims.AddRange(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtsettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToInt64(_jwtsettings.ExpirationInDays));

            var token = new JwtSecurityToken(_jwtsettings.Issuer, _jwtsettings.Issuer, claims, null, expires, creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
