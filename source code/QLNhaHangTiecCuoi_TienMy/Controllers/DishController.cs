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
    public class DishController : ControllerBase
    {

        ApplicationDbContext da = new ApplicationDbContext();

        [HttpGet]
        public IActionResult GetAll([FromQuery] String keyword, [FromQuery] String isActive, [FromQuery] decimal FromPrice=-1, [FromQuery] decimal ToPrice=0
            , [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10 , [FromQuery] bool descending = false)
        {
            IEnumerable<Dish> dishes = (FromPrice < 0 && ToPrice == 0) ? da.Dishes : da.Dishes.FromSqlRaw($"EXEC dbo.GetDishInPriceRange {FromPrice}, {ToPrice}");
            if (keyword != null && keyword != "")
            {
                dishes = dishes.Where(d => StringUtils.convertToUnSign(d.Name).Contains(StringUtils.convertToUnSign(keyword)));
            }
            if (isActive != null && isActive != "")
            {
                if (isActive.ToString().Equals("true"))
                    dishes = dishes.Where(d => d.IsActive == true);
                else if (isActive.ToString().Equals("false"))
                    dishes = dishes.Where(d => d.IsActive == false);
            }
            if (descending)
            {
                dishes = dishes.OrderByDescending(d=>d.Name);
            }
            int totalPages = (dishes.Count() % pageSize) > 0 ? (dishes.Count() / pageSize) + 1 : (dishes.Count() / pageSize);

            return Ok(new { 
                Dish = dishes.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList(),
                Page = pageIndex,
                Size = pageSize,
                Total = totalPages
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            Dish dish = da.Dishes.FirstOrDefault(d => d.Id == id);
            if (dish != null) return Ok(dish);
            return NotFound(new { Message = "Dish with id: " + id + " isn't in the system." });
           
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public IActionResult AddNewDish([FromForm] DishData request)
        {
            try
            {
                if (request == null) return BadRequest(new { Message = "Your request is empty."});

                IDictionary<String, Object> errors = new Dictionary<string, object>();

                Dish dish = new Dish();
                if (request.Name == null || request.Name == "") errors.Add("Name", "Dish name cannot be null.");
                dish.Name = request.Name;
                dish.Description = request.Description;
                if (request.Price.Equals(null) || request.Price == 0) errors.Add("Price", "Price must have been present and greater than 0." );
                dish.Price = request.Price;
                dish.Discount = request.Discount;
                if (request.File == null) errors.Add("Image", "Cannot set without dish image. You can choose one.");
                else if (errors.Count == 0) 
                {
                    if (request.File.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            request.File.OpenReadStream().CopyTo(memoryStream);
                            dish.Image = memoryStream.ToArray();
                        }
                    }
                }
                if (!request.IsActive.Equals(null)) dish.IsActive = request.IsActive;

                if (errors.Count > 0)
                {
                    return BadRequest(new { Error = JsonConvert.SerializeObject(errors) });
                }

                da.Dishes.Add(dish);
                da.SaveChanges();

                return Created("api/Dish/" + dish.Id, new { Dish = dish });
            } catch
            {
                return BadRequest(new { Message = "System cannot handle your request." });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public IActionResult UpdateExistingDish(int id, [FromForm] DishData request)
        {
            try
            {
                Dish existing = da.Dishes.First(d => d.Id == id);

                if (request == null) return BadRequest(new { Message = "Your request is empty." });

                if (request.Name != null && request.Name.Length > 0) existing.Name = request.Name;
                if (request.Description != null && request.Description.Length > 0) existing.Description = request.Description;
                if (!request.Price.Equals(null) && request.Price > 0) existing.Price = request.Price;
                if (!request.Discount.Equals(null)) existing.Discount = request.Discount;
                if (request.File != null)
                {
                    if (request.File.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            request.File.OpenReadStream().CopyTo(memoryStream);
                            existing.Image = memoryStream.ToArray();
                        }
                    }
                }
                if (!request.IsActive.Equals(null)) existing.IsActive = request.IsActive;

                da.SaveChanges();

                return Accepted();
            } catch (ArgumentNullException e)
            {
                Console.WriteLine("Error: " + e.Message);
                return NotFound(new { Message = "Dish with id: " + id + " isn't in the system." });
            } catch
            {
                return BadRequest(new { Message = "System cannot handle your request." });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                Dish existing = da.Dishes.First(d => d.Id == id);
                da.Dishes.Remove(existing);
                da.SaveChanges();

                return NoContent();
            }
            catch (Exception e)
            {
                if (e is DbUpdateConcurrencyException || e is DbUpdateException)
                {
                    return BadRequest(new { Message = "Dish with id: " + id + " cannot remove because of references to it." });
                }

                return NotFound(new { Message = "Dish with id: " + id + " isn't existing in the system." });
            }
        }
    }
}
