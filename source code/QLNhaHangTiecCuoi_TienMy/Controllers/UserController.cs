using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QLNhaHangTiecCuoi_TienMy.Models;
using QLNhaHangTiecCuoi_TienMy.Models.Data;
using QLNhaHangTiecCuoi_TienMy.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace QLNhaHangTiecCuoi_TienMy.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {

        ApplicationDbContext da = new ApplicationDbContext();

        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [HttpGet]
        public IActionResult GetAll([FromQuery] String keyword = null, [FromQuery] bool descending = false
                , [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            IEnumerable <User> users = da.Users;
            if (keyword != null && keyword != "")
            {
                keyword = StringUtils.convertToUnSign(keyword);
                users = da.Users.Where(u => u.Id.ToString().Equals(keyword)
                    || u.FirstName.Contains(keyword) || u.LastName.Contains(keyword));
            }
            if (descending)
            {
                users = users.OrderByDescending(u => u.Id);
            }

            int totalPages = users.Count() % pageSize > 0 ? (users.Count() / pageSize) + 1 : (users.Count() / pageSize);

            foreach (User user in users)
            {
                user.Password = null;
            }

            return Ok(new {
                User = users.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList(),
                Page = pageIndex,
                Size = pageSize,
                Total = totalPages
            });
        }

        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            User user = da.Users.Include(u => u.Employees).FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.Password = null;
                return Ok(user);
            }
            return NotFound(new { Message = "User with ID: " + id + " isn't existing in the system." });
        }

        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [HttpPost]
        public IActionResult AddNewUser([FromForm] UserData request)
        {
            try
            {
                if (request == null) return BadRequest(new { Message = "Your request is empty." });

                IDictionary<String, String> errors = new Dictionary<String, String>();

                User user = new User();
                if (request.Username != null && request.Username != "") user.Username = request.Username;
                else errors.Add("Username", "Username cannot be null.");
                if (request.Password != null && request.Password != "") user.Password = UserPasswordUtils.HashPassword(request.Password);
                else errors.Add("Password", "Password cannot be null.");
                if (request.Phone != null && request.Phone != "") user.Phone = request.Phone;
                else errors.Add("Phone", "Phone cannot be null.");
                if (request.FirstName != null && request.LastName != "") user.FirstName = request.FirstName;
                else errors.Add("FirsNtname", "Firstname cannot be null.");
                if (request.LastName != null && request.LastName != null) user.LastName = request.LastName;
                user.IdentityNumber = request.IdentityNumber;
                if (request.Role == null || request.Role == "") request.Role = "USER";
                user.Role = request.Role;
                user.CreatedDate = DateTime.Now;

                if (errors.Count > 0)
                {
                    return BadRequest(new { Error = JsonConvert.SerializeObject(errors) });
                }

                da.Users.Add(user);
                da.SaveChanges();
                user.Password = null;

                return Created("api/user/" + user.Id, new { User = user });
            } catch
            {
                return BadRequest(new { Message = "System cannot handle your request." });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public IActionResult UpdateExistingUser(int id, [FromForm] UserData request)
        {
            try
            {
                User existing = da.Users.First(u => u.Id == id);

                if (request == null) return BadRequest(new { Message = "Your request is empty." });

                IDictionary<String, String> errors = new Dictionary<String, String>();

                if (request.FirstName != "") existing.FirstName = request.FirstName;
                else errors.Add("FirstName", "Your first name is not valid. Try another.");
                if (request.LastName != "") existing.LastName = request.LastName;
                else errors.Add("LastName", "Your last name is not valid. Try another.");
                if (request.IdentityNumber != "") existing.IdentityNumber = request.IdentityNumber;
                else errors.Add("IdentityNumber", "Your Identity is not valid. Try another is valid.");
                if (request.Phone != "") existing.Phone = request.Phone;
                else errors.Add("Phone", "Your phone number is not valid. Try again.");
                if (request.Role != null && request.Role != "") existing.Role = request.Role;

                if (errors.Count > 0)
                {
                    return BadRequest(new { Error = JsonConvert.SerializeObject(errors) });
                }

                da.SaveChanges();

                return Accepted();
            } catch
            {
                return NotFound(new { Message = "User with ID: " + id + " isn't existing in the system." });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                User existing = da.Users.Include(u => u.Employees).First(u => u.Id == id);
                da.Users.Remove(existing);
                da.SaveChanges();

                return NoContent();
            } catch (Exception e)
            {
                if (e is DbUpdateConcurrencyException || e is DbUpdateException)
                {
                    return BadRequest(new { Message = "User with id: " + id + " cannot remove because of weddings references to it." });
                }
                return NotFound(new { Message = "User with ID: " + id + " isn't existing in the system." });
            }
        }


        [Authorize(Roles = "ADMIN")]
        [HttpGet("roles")]
        public IActionResult GetRole()
        {
            return Ok(da.Roles.FromSqlRaw("EXEC dbo.GetRoles"));
        }


        [Authorize(Roles = "ADMIN")]
        [HttpGet("users-by-role")]
        public IActionResult GetUsersByRole([FromQuery] String role)
        {
            try
            {
                return Ok(da.Users.FromSqlRaw($"EXEC dbo.GetUserByRole {role}"));
            } catch
            {
                return BadRequest(new { Message = "Cannot get users from that role. Error!!!" });
            }
        }
    }
}
