using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using QLNhaHangTiecCuoi_TienMy.Models;
using System.Text;
using Microsoft.Extensions.Configuration;
using QLNhaHangTiecCuoi_TienMy.Models.Data;
using QLNhaHangTiecCuoi_TienMy.Utils;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace QLNhaHangTiecCuoi_TienMy.Utils
{
    public class UserPasswordUtils
    {
        static ApplicationDbContext da = new ApplicationDbContext();
        public static String HashPassword(String originPassword)
        {
            byte[] data = (MD5.Create()).ComputeHash(Encoding.UTF8.GetBytes(originPassword));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                builder.Append(data[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public static User GetCurrentUser(String tokenString)
        {
            try
            {
                String username = GetUserNameInCurrent(tokenString);
                User currentUser = da.Users.Include(u => u.Employees).First(u => u.Username.Equals(username));
                return currentUser;
            } catch
            {
                return null;
            }
        }

        public static String GetUserNameInCurrent(String tokenString)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            var jwtSecuityToken = handler.ReadJwtToken(tokenString);
            return jwtSecuityToken.Claims.FirstOrDefault(c => c.Type == "Username")?.Value;
        }
    }
}
