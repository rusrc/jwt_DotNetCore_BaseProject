using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using static WebApplication2.Startup;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Security.Claims;
using WebApplication2.Models;
using Microsoft.AspNetCore.Identity;
using WebApplication2.ModelViews;

namespace WebApplication2.Controllers
{
    [Produces("application/json")]
    [Route("api/Account")]
    public class AccountController : Controller
    {
        CoreDbContext _context;
        UserManager<User> _userManager;

        public AccountController(CoreDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("token")]
        public async Task Token([FromBody]JwtLoginModelView model)
        {
            var identity = await GetIdentity(model.Username, model.Password);

            if (identity == null)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                await Response.WriteAsync("Invalid username or password.");
                return;
            }

            var now = DateTime.UtcNow;
            // create JWT-token
            var jwt = new JwtSecurityToken(
                    issuer: JWT_ISSUER,
                    audience: JWT_AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(JWT_LIFETIME)),
                    signingCredentials: new SigningCredentials(JwtSymmetricSecurityKey, SecurityAlgorithms.HmacSha256));

            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };

            // Response
            // Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        [HttpPost("token2")]
        public async Task<IActionResult> Token2([FromBody]JwtLoginModelView model)
        {
            var identity = await GetIdentity(model.Username, model.Password);

            if (identity == null)
            {
                return BadRequest(new { Error = "Invalid username or password." });
            }

            var now = DateTime.UtcNow;
            // create JWT-token
            var jwt = new JwtSecurityToken(
                    issuer: JWT_ISSUER,
                    audience: JWT_AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(JWT_LIFETIME)),
                    signingCredentials: new SigningCredentials(JwtSymmetricSecurityKey, SecurityAlgorithms.HmacSha256));

            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };

            return Ok(response);

        }

        private async Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            User user = await _userManager.FindByNameAsync(username);

            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, "SimpleUser")
                };

                var claimsIdentity = new ClaimsIdentity
                (
                    claims,
                    "Token",
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType
                );

                return claimsIdentity;
            }

            return null; // No user
        }
    }
}