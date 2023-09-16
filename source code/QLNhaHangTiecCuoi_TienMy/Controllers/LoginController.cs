using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using QLNhaHangTiecCuoi_TienMy.Models;
using QLNhaHangTiecCuoi_TienMy.Models.Data;
using QLNhaHangTiecCuoi_TienMy.Utils;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace QLNhaHangTiecCuoi_TienMy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        ApplicationDbContext da = new ApplicationDbContext();

        private IConfiguration _conf;

        public LoginController(IConfiguration conf)
        {
            this._conf = conf;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromForm] String username, [FromForm] String password)
        {
            IActionResult response = Unauthorized();
            LoginData login = new LoginData { Username = username, Password = password };

            var user = AuthenticateUser(login);

            if (user != null)
            {
                String tokenString = GenerateJSONWebToken(user);
                response = Ok(new { Token = tokenString });
            }

            return response;
        }

        private User AuthenticateUser(LoginData login)
        {
            User user = da.Users.FirstOrDefault(u => u.Username.Equals(login.Username));
            if (user != null && user.Password.Equals(UserPasswordUtils.HashPassword(login.Password)))
            {
                return new User
                {
                    Username = user.Username,
                    Phone = user.Phone,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                };
            }
            return null;
        }

        private String GenerateJSONWebToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conf["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("Username", userInfo.Username),
                new Claim(JwtRegisteredClaimNames.FamilyName, userInfo.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, userInfo.Role)
            };

            var tokenString = new JwtSecurityToken(
                    issuer: _conf["JwtSettings:Issuer"],
                    audience: _conf["JwtSettings:Audience"],
                    claims,
                    expires: DateTime.Now.AddMinutes(240),
                    signingCredentials: credentials);

            var encodedTokenString = new JwtSecurityTokenHandler().WriteToken(tokenString);

            return encodedTokenString;
        }
    }
}
