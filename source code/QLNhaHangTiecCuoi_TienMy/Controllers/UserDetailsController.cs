using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QLNhaHangTiecCuoi_TienMy.Models;
using QLNhaHangTiecCuoi_TienMy.Utils;
using Microsoft.AspNetCore.Authorization;

namespace QLNhaHangTiecCuoi_TienMy.Controllers
{
    [Route("api/")]
    [ApiController]
    public class UserDetailsController : ControllerBase
    {
        ApplicationDbContext da = new ApplicationDbContext();

        [Route("register")]
        [HttpPost]
        public IActionResult Register([FromForm] User request)
        {
            try
            {
                IDictionary<String, String> errors = new Dictionary<String, String>();
                if (request.Username == null || request.Username == "") errors.Add("Username", "Username cannot be null.");
                if (request.FirstName == null || request.FirstName == "") errors.Add("FirstName", "FirstName cannot be null.");
                if (request.LastName == null || request.Username == "") errors.Add("Lastname", "Lastname cannot be null.");
                if (request.Password == null || request.Password == "") errors.Add("Password", "Password is required.");
                if (request.Phone == null || request.Phone == "") errors.Add("Phone", "Phone is required to indentify you and other.");
                if (request.IdentityNumber == "") errors.Add("IdentityNumber", "Identity number is not valid.");
                request.Role = "USER";

                if (errors.Count > 0)
                {
                    return BadRequest(new { Error = JsonConvert.SerializeObject(errors) });
                }

                request.Password = UserPasswordUtils.HashPassword(request.Password);
                da.Users.Add(request);
                da.SaveChanges();

                return Ok(new { Message = "You have registered in the system. Now you can log in." });
            } catch
            {
                return BadRequest(new { Message = "Cannot handle your register request." });
            }
        }

        [Authorize]
        [Route("change-password")]
        [HttpPost]
        public IActionResult ChangePassword([FromForm] String oldPassword, [FromForm] String newPassword, [FromHeader] String Authorization)
        {
            try
            {
                String tokenString = Authorization.Replace("Bearer ", "");
                String currentUserName = UserPasswordUtils.GetUserNameInCurrent(tokenString);
                User currentUser = da.Users.FirstOrDefault(u => u.Username.Equals(currentUserName));
                String has = UserPasswordUtils.HashPassword(oldPassword);
                if (currentUser == null || !UserPasswordUtils.HashPassword(oldPassword).Equals(currentUser.Password))
                    throw new ArgumentNullException();
                currentUser.Password = UserPasswordUtils.HashPassword(newPassword);
                da.SaveChanges();

                return Ok(new { Message = "You have changed password successfully." });
            } catch
            {
                return BadRequest(new { Message = "You must have true password before." });
            }
        }

        [Route("forgot-password")]
        [HttpPost]
        public IActionResult ForgotPassword([FromForm] String password, [FromForm] String phone, [FromForm] String username)
        {
            try
            {
                User user = da.Users.First(u => u.Username.Equals(username));
                if (!user.Phone.Equals(phone)) return BadRequest(new { Phone = "Phone number you have typed is not matched in system." });
                user.Password = UserPasswordUtils.HashPassword(password);
                da.SaveChanges();
                return Ok(new { Message = "You have reset your password successfully." });
            } catch
            {
                return NotFound(new { Message = "Your account isn't existing or system cannot handle you request." });
            }
        }

        [Authorize]
        [Route("current-user")]
        [HttpGet]
        public IActionResult GetCurrentUser([FromHeader] String Authorization)
        {
            String tokenString = Authorization.Replace("Bearer ", "");
            User currentUser = UserPasswordUtils.GetCurrentUser(tokenString);
            if (currentUser != null)
            {
                currentUser.Password = null;
            }
            return Ok(new { User = currentUser });
        }
    }
}
