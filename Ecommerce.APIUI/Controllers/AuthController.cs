using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.APIUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.APIUI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JWTSettingsModel _jwtSettings;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<IdentityUser> userManager,
            IOptions<JWTSettingsModel> jwtSettings,
            ILogger<AuthController> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings.Value));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterModel userModel)
        {
            try
            {
                var user = new IdentityUser { UserName = userModel.Email, Email = userModel.Email };
                var result = await _userManager.CreateAsync(user, userModel.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"user with a name of {user.Email} was created !");
                    return Ok();
                }
                else
                {
                    _logger.LogInformation($"user tried to register with {user.Email} but failed");
                    return BadRequest(result.Errors);
                }
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken(UserLoginModel userModel)
        {
            try
            {
                if (userModel is null) return BadRequest("Invalid Sign in Credentials.");
                IdentityUser user = _userManager.Users.SingleOrDefault(u => u.Email == userModel.Email);
                if (user is null)
                {
                    _logger.LogWarning($"Someone tried to log in with a non Existing user with the name of {userModel.Email}");
                    return BadRequest( "Invalid Sign in Credentials." );
                    // return NotFound("User not found");
                }
                bool correctPassword = await _userManager.CheckPasswordAsync(user, userModel.Password);
                if (correctPassword)
                {
                    IEnumerable<string> roles = await _userManager.GetRolesAsync(user);
                    string token = GenerateJWTToken(user, roles);
                    _logger.LogInformation($"User {userModel.Email} Successfuly requested a token.");
                    TokenModel tokenModel= new TokenModel(){Token = token};
                    return Ok(tokenModel);
                }
                else
                {
                    _logger.LogWarning($"Someone tried to log in with a user {user.Email} but provided a wrong password.");
                    return BadRequest( "Invalid Sign in Credentials.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error" );
            }

        }

        private string GenerateJWTToken(IdentityUser user, IEnumerable<string> roles)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub , user.Id.ToString()),
                new Claim(ClaimTypes.Name , user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            };

            IEnumerable<Claim> roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));
            claims.AddRange(roleClaims);
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            DateTime expires = DateTime.Now.AddDays(Convert.ToDouble(_jwtSettings.ExpirationInDays));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Issuer,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}