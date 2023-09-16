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
    [Route("api/[controller]")]
    [ApiController]
    public class HallController : ControllerBase
    {
        ApplicationDbContext da = new ApplicationDbContext();

        [HttpGet]
        public IActionResult GetAll([FromQuery] String keyword, [FromQuery] String isActive, [FromQuery] bool descending = false)
        {
            IEnumerable<Hall> halls = da.Halls;
            if (keyword != null && keyword != "")
            {
                halls = halls.Where(h => h.Id.ToString().Equals(keyword) || StringUtils.convertToUnSign(h.Name).Contains(StringUtils.convertToUnSign(keyword)));
            }
            if (isActive != null && isActive != "")
            {
                if (isActive.Equals("true"))
                    halls = halls.Where(h => h.IsActive == true);
                else if (isActive.Equals("false"))
                    halls = halls.Where(h => h.IsActive == false);
            }
            if (descending)
            {
                halls = halls.OrderByDescending(h=>h.Name);
            }

            return Ok(new {
                Hall = halls
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            Hall hall = da.Halls.FirstOrDefault(h => h.Id == id);
            if (hall != null)
            {
                return Ok(hall);
            }
            return NotFound(new { Message = "Hall with id: " + id + " isn't in the system." });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public IActionResult AddNewHall(HallData request)
        {
            try
            {
                if (request == null) return BadRequest(new { Message = "Your request is empty." });

                IDictionary<String, Object> errors = new Dictionary<String, Object>();

                Hall hall = new Hall();
                if (request.Name == null || request.Name == "") errors.Add("Name", "Hall name cannot be empty.");
                else hall.Name = request.Name;
                if (request.Description == null || request.Description == "") errors.Add("Description", "Description cannot be empty.");
                else hall.Description = request.Description;
                if (request.TableNumber.Equals(null) || request.TableNumber == 0) errors.Add("TableNumber", "Table number must be present and greater than 0.");
                else hall.TableNumber = request.TableNumber;
                if (request.GuestUpTo.Equals(null) || request.GuestUpTo == 0) errors.Add("GuestUpTo", "Guest up to must be present and greater than 0.");
                else  hall.GuestUpTo = request.GuestUpTo;
                if (request.Price.Equals(null) || request.Price == 0) errors.Add("Price", "Price must be present and greater than 0.");
                hall.Price = request.Price;
                hall.Discount = request.Discount;
                hall.IsActive = request.IsActive;

                if (errors.Count > 0)
                {
                    return BadRequest(new { Error = JsonConvert.SerializeObject(errors) });
                }

                da.Halls.Add(hall);
                da.SaveChanges();

                return Created("api/Hall/" + hall.Id, new { Hall = hall });
            } catch
            {
                return BadRequest(new { Message = "System cannot handle your request." });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public IActionResult UpdateExistingHall(int id, [FromBody] HallData request)
        {
            try
            {
                Hall existing = da.Halls.First(h => h.Id == id);

                IDictionary<String, String> errors = new Dictionary<String, String>();

                if (request == null) return BadRequest(new { Message = "Your request is empty." });

                if (request.Name != null && request.Name.Length > 0) existing.Name = request.Name;
                if (request.Description != null && request.Description.Length > 0) existing.Description = request.Description;
                if (!request.TableNumber.Equals(null))
                {
                    if (request.TableNumber > 0) existing.TableNumber = request.TableNumber;
                    else
                    {
                        errors.Add("TableNumber", "Table number must have been greater than 0.");
                    }
                }
                if (!request.GuestUpTo.Equals(null))
                {
                    if (request.GuestUpTo > 0) existing.GuestUpTo = request.GuestUpTo;
                    else
                    {
                        errors.Add("GuestUpTo", "Guest up to must have been greater than 0.");
                    }
                }
                if (!request.Price.Equals(null))
                {
                    if (request.Price > 0) existing.Price = request.Price;
                    else
                    {
                        errors.Add("Price", "Price must have been greater than 0.");
                    }
                }
                if (!request.Discount.Equals(null)) existing.Discount = request.Discount;
                if (request.IsActive.HasValue)
                {
                    existing.IsActive = request.IsActive.GetValueOrDefault(true);
                }

                if (errors.Count > 0)
                {
                    return BadRequest(new { Error = JsonConvert.SerializeObject(errors) });
                }

                da.SaveChanges();

                return Accepted();
            } catch 
            {
                return NotFound(new { Message = "Hall with id: " + id + " isn't in the system." });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                Hall exising = da.Halls.First(h => h.Id == id);
                da.Halls.Remove(exising);
                da.SaveChanges();

                return NoContent();
            } catch (Exception e)
            {
                if (e is DbUpdateConcurrencyException || e is DbUpdateException)
                {
                    return BadRequest(new { Message = "Hall with id: " + id + " cannot remove because of references to it." });
                }
                return NotFound(new { Message = "Hall with id: " + id + " isn't existing in the system." });
            }
        }
    }
}
