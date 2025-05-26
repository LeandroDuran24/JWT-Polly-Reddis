using Domain.Models;
using Domain.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IJWTRepository jWTRepository, IConfiguration configuration) : ControllerBase
    {
        private readonly IJWTRepository jWTRepository  = jWTRepository;
        private readonly IConfiguration configuration = configuration;

        [HttpPost("Auth")]
        [AllowAnonymous]
        public async Task<IActionResult> Auth([FromBody] Users users)
        {
            try
            {
                var token  = jWTRepository.Auth(users);

                if(token == null)
                {
                    return Unauthorized();
                }

                return Ok(token);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }


        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh(Tokens tokens)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(tokens.Token);
                var username = principal.Identity.Name;

                //Esto seria para cuando el refresh se guarde en la DB

                //var savedRefreshToken = 
                //if (savedRefreshToken != tokens.RefreshToken)
                //    return Unauthorized();

                var newJwtToken = jWTRepository.GenerateToken(username);
                var newRefreshToken = jWTRepository.GenerateRefreshToken();

                return Ok(new Tokens { Token = newJwtToken, RefreshToken = newRefreshToken });

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {

                var tokenValidation = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]))
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidation, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");

                return principal;
            }
            catch (Exception e)
            {
                throw e ;
            }
        }
    }
}
