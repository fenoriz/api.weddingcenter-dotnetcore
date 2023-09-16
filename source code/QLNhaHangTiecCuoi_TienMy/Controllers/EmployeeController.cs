using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using QLNhaHangTiecCuoi_TienMy.Models;
using QLNhaHangTiecCuoi_TienMy.Models.Data;
using QLNhaHangTiecCuoi_TienMy.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace QLNhaHangTiecCuoi_TienMy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        ApplicationDbContext da = new ApplicationDbContext();

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public IActionResult GetAll([FromQuery] String keyword = null, [FromQuery] bool descending = false
                , [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            IEnumerable<Employee> employees = da.Employees.Include(e => e.User);
            if (keyword != null && keyword != "")
            {
                employees = employees.Where(e => e.Id.ToString() == keyword);
            }

            if (descending)
            {
                employees = employees.OrderByDescending(e => e.Id);
            }

            int totalPages = employees.Count() % pageSize > 0 ? (employees.Count() / pageSize) + 1 : (employees.Count() / pageSize);

            return Ok(new {
                Employee = employees,
                Size = pageSize,
                Page = pageIndex,
                Total = totalPages });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            Employee employee = da.Employees.Include(e => e.User).FirstOrDefault(e => e.Id == id);
            if (employee != null) return Ok(employee);
            return NotFound(new { Message = "Employee with ID:" + id + " isn't existing in the system." });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public IActionResult AddNewEmployee([FromForm] EmployeeData request)
        {
            try
            {
                Employee employee = new Employee();

                IDictionary<String, String> errors = new Dictionary<String, String>();

                if (request.DateOfBirth != null && request.DateOfBirth.Year <= DateTime.Now.Year - 18) employee.DateOfBirth = request.DateOfBirth;
                else errors.Add("DateOfBirth", "Date of birth should have been over 18 years old.");
                if (request.File != null && request.File.Length > 0)
                {
                    using (var mermoryStream = new MemoryStream())
                    {
                        request.File.OpenReadStream().CopyTo(mermoryStream);
                        employee.Picture = mermoryStream.ToArray();
                    }
                }
                else errors.Add("Picture", "Picture cannot be absent in this field.");
                if (request.UserId.Equals(null)) errors.Add("UserId", "User must have been chosen in this field.");
                if (!request.UserId.Equals(null) && da.Users.FirstOrDefault(u => u.Id == request.UserId) == null)
                    errors.Add("UserId", "User isn't existing in the system.");
                else employee.UserId = request.UserId;

                if (errors.Count > 0)
                {
                    return BadRequest(new { Error = JsonConvert.SerializeObject(errors) });
                }

                da.Employees.Add(employee);
                da.SaveChanges();

                return Created("api/Employee/" + employee.Id, new { Employee = employee });
            } catch
            {
                return BadRequest(new { Message = "System cannot handle your request." });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public IActionResult UpdateExistingEmployee(int id, [FromForm] EmployeeData request)
        {
            try
            {
                Employee existing = da.Employees.First(e => e.Id == id);

                if (request == null) return BadRequest(new { Message = "Your request is null." });

                if (request.DateOfBirth != null && request.DateOfBirth.Year <= DateTime.Now.Year - 18) existing.DateOfBirth = request.DateOfBirth;
                else return BadRequest(new { DateOfBirth = "Date of birth should have been over 18 years old." });
                if (request.File != null && request.File.Length > 0)
                {
                    using (var mermoryStream = new MemoryStream())
                    {
                        request.File.OpenReadStream().CopyTo(mermoryStream);
                        existing.Picture = mermoryStream.ToArray();
                    }
                }

                da.SaveChanges();

                return Accepted();
            } catch
            {
                return NotFound(new { Message = "Employee with ID:" + id + " isn't existing in the system." });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                Employee existing = da.Employees.First(e => e.Id == id);
                Employee employeeUser = da.Employees.Include(e => e.User).First(e => e.Id == id);
                employeeUser.User.Role = "USER";
                da.Employees.Remove(existing);
                da.SaveChanges();

                return NoContent();
            } catch
            {
                return NotFound(new { Message = "Employee with ID:" + id + " isn't existing in the system." });
            }
        }
    }
}
