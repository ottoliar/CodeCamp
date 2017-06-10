using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using ServiceLayer.Filters;
using ServiceLayer.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Controllers
{
    public class AuthController : Controller
    {
        private CampContext _context;
        private SignInManager<CampUser> _signInMgr;
        private ILogger _logger;
        private UserManager<CampUser> _userMgr;
        private IPasswordHasher<CampUser> _hasher;
        private IConfigurationRoot _config;

        public AuthController(CampContext context,
            SignInManager<CampUser> signInMgr,
            UserManager<CampUser> userMgr,
            IPasswordHasher<CampUser> hasher,
            ILogger logger,
            IConfigurationRoot config)
        {
            _signInMgr = signInMgr;
            _userMgr = userMgr;
            _hasher = hasher;
            _context = context;
            _logger = logger;
            _config = config;
        }

        [HttpPost("api/auth/login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody]CredentialModel model)
        {
            try
            {
                var result = await _signInMgr.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Exception thrown while logging in: {ex}");
            }

            return BadRequest("Failed to login");
        }

        [ValidateModel]
        [HttpPost("api/auth/token")]
        public async Task<IActionResult> CreateToken([FromBody]CredentialModel model)
        {
            try
            {
                var user = await _userMgr.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    if (_hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Success)
                    {
                        var userClaims = await _userMgr.GetClaimsAsync(user);

                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                            new Claim(JwtRegisteredClaimNames.Email, user.Email)
                        }.Union(userClaims);

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            issuer: _config["Tokens:Issuer"],
                            audience: _config["Tokens:Audience"],
                            claims: claims,
                            expires: DateTime.UtcNow.AddMinutes(15),
                            signingCredentials: creds
                            );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Exception thrown while creating JWT: {ex}");
            }

            return BadRequest("Failed to generate token");
        }
    }
}
