using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using QLNhaHangTiecCuoi_TienMy.Models;
using QLNhaHangTiecCuoi_TienMy.Models.Data;
using Microsoft.AspNetCore.Authorization;
using QLNhaHangTiecCuoi_TienMy.Utils;
using Microsoft.EntityFrameworkCore;

namespace QLNhaHangTiecCuoi_TienMy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeddingController : ControllerBase
    {
        ApplicationDbContext da = new ApplicationDbContext();

        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [HttpGet]
        public IActionResult GetAll([FromQuery] bool? pending, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate
            , [FromQuery] int? customerId, [FromQuery] bool? completed, [FromQuery] bool? isOver, [FromQuery] bool? paid
            , [FromQuery] int? dayLeft, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {

            IEnumerable<Wedding> weddings = (dayLeft.Equals(null) || dayLeft == 0) ? da.Weddings 
                : da.Weddings.FromSqlRaw($"EXEC dbo.FindPendingWeddingInDays {dayLeft}");
            if (pending.HasValue && pending == true) weddings = weddings.Where(w => w.CreatedDate.Equals(null));
            if (fromDate.HasValue)
            {
                if (!fromDate.GetValueOrDefault(DateTime.Now.AddDays(1)).Date.Equals(DateTime.Now.AddDays(1).Date)) {
                    weddings = weddings.Where(w => w.CreatedDate.HasValue
                        && w.CreatedDate.GetValueOrDefault().Date >= fromDate.GetValueOrDefault().Date);
                }
            }
            weddings = weddings.Where(w => w.CreatedDate.HasValue
                        && w.CreatedDate.GetValueOrDefault().Date <= toDate.GetValueOrDefault(DateTime.Now) || w.CreatedDate == null);
            if (customerId.HasValue && customerId > 0)
            {
                weddings = weddings.Where(w => w.CustomerId == customerId);
            }
            if (completed.HasValue)
            {
                weddings = weddings.Where(w => w.PaidDate.HasValue
                    && w.CelebrityDate.HasValue && w.CelebrityDate.GetValueOrDefault().Date > DateTime.Now.Date);
            } else if (paid.HasValue)
            {
                weddings = weddings.Where(w => w.PaidDate.HasValue);
            } else if (isOver.HasValue)
            {
                weddings = weddings.Where(w => !w.PaidDate.HasValue
                    && w.CelebrityDate.HasValue && w.CelebrityDate.GetValueOrDefault().Date > DateTime.Now.Date);
            }

            weddings = weddings.OrderByDescending(w => w.Id);

            int totalPages = weddings.Count() % pageSize > 0 ? (weddings.Count() / pageSize) + 1 : (weddings.Count() / pageSize);

            return Ok(new { 
                Wedding = weddings.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList(),
                Page = pageIndex,
                Size = pageSize,
                Total = totalPages
            });
        }

        [Authorize(Roles = "ADMIN,EMPLOYEE,USER")]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var wedding = da.Weddings.Where(w => w.Id == id).Include(w => w.DishInWeddings).Include(w => w.Hall).Include(w => w.Customer)
                .Include(w=>w.Staff).Select(wedding => new { wedding, Total = da.CalculateWeddingOrder(wedding.Id) }).FirstOrDefault();
            if (wedding != null)
            {
                return Ok(wedding);
            }
            return NotFound(new { Message = "Wedding with ID:" + id + " isn't existing in the system." });
        }

        [Authorize(Roles = "ADMIN,EMPLOYEE,USER")]
        [HttpPost]
        public IActionResult AddNewWedding([FromBody] WeddingData request, [FromHeader] String Authorization)
        {
            string tokenString = Authorization.Replace("Bearer ", "");
            IDbContextTransaction tx = da.Database.BeginTransaction();
            try
            {
                if (request == null) return BadRequest(new { Message = "Request is empty." });

                Hall hall;
                if (!request.HallId.Equals(null))
                {
                    hall = da.Halls.First(h => h.Id == request.HallId);
                }
                else return BadRequest(new { Error = new { HallId = "That hall doesn't exist." } });

                IDictionary<String, String> errors = new Dictionary<String, String>();

                Wedding wedding = new Wedding();
                if (request.Description == null || request.Description == "") errors.Add("Description", "Wedding must have at least some note from customer.");
                else wedding.Description = request.Description;
                if (request.TableNumber.Equals(null) || request.TableNumber == 0) errors.Add("TableNumber", "Table Number cannot be null or 0.");
                else wedding.TableNumber = request.TableNumber;
                if (request.GuestNumber.Equals(null) || request.GuestNumber == 0) errors.Add("GuestNumber", "Guest Number cannot be null or 0." );
                else wedding.GuestNumber = request.GuestNumber;
                wedding.Deposit = request.Deposit;
                wedding.DepositVia = request.DepositVia;
                wedding.DepositReceiptNo = request.DepositReceiptNo;
                if (request.CelebrityDate == null) errors.Add("CelebrityDate", "Wedding must have celebrity date.");
                else if (!(request.CelebrityDate.HasValue && request.CelebrityDate.Value.Date >= DateTime.Now.AddDays(3).Date))
                    errors.Add("CelebrityDate", "Wedding must have at least 3 days from now.");
                else wedding.CelebrityDate = request.CelebrityDate;
                if (request.HallId.Equals(null)) errors.Add("HallId", "Hall must have been chosen.");
                else { wedding.HallId = request.HallId;
                    wedding.HallPrice = hall.Price;
                    wedding.HallDiscount = hall.Discount.GetValueOrDefault(0);
                }
                if (request.CustomerId.Equals(null)) errors.Add("CustomerId", "Customer cannot be null.");

                if (errors.Count == 0)
                {
                    User user = UserPasswordUtils.GetCurrentUser(tokenString);
                    if (user != null)
                    {
                        if (user.Employees.Count > 0) //employee creating
                        {
                            if (wedding.Deposit.HasValue && wedding.Deposit > 0
                                && (wedding.DepositVia != null && wedding.DepositVia != "")) {
                                if (wedding.DepositReceiptNo != null && wedding.DepositReceiptNo != ""
                                    || (wedding.DepositVia.Equals("Cash"))) { //accept all
                                    User customer = da.Users.FirstOrDefault(u => u.Id == request.CustomerId);
                                    if (customer == null) return BadRequest(new { Error = new { CustomerId = "You must choose customer who owns this wedding order." } });
                                    wedding.CustomerId = request.CustomerId;
                                    wedding.CreatedDate = DateTime.Now;
                                    wedding.StaffId = user.Id;

                                } else
                                {
                                    return BadRequest(new { Error = new { DepositReceiptNo = "You have Receipt Number if not pay over cash." } });
                                }
                            } else
                            {
                                return BadRequest(new { Error = new { Deposit = "You must take deposit from customer to add new *." } });
                            }

                        }
                        else //user creating
                        {
                            wedding.CustomerId = user.Id;
                            wedding.Deposit = null;
                            wedding.DepositReceiptNo = null;
                            wedding.DepositVia = null;
                        }
                    }
                } else
                {
                    return BadRequest(new { Error = JsonConvert.SerializeObject(errors) });
                }

                da.Weddings.Add(wedding);

                if (request.dishOrders != null && request.dishOrders.Count > 0)
                {
                    request.dishOrders.ForEach((dishOrder) => {
                        Dish dish = da.Dishes.FirstOrDefault(d=>d.Id == dishOrder.DishId);
                        if (dish != null)
                        {
                            DishInWedding dishInWedding = new DishInWedding();
                            dishInWedding.DishId = dishOrder.DishId;
                            dishInWedding.DishPrice = dish.Price;
                            dishInWedding.DishDiscount = dish.Discount.GetValueOrDefault(0);
                            if (dishOrder.Quantity.Equals(null) || dishOrder.Quantity == 0) dishOrder.Quantity = 1;
                            dishInWedding.Quantity = dishOrder.Quantity;
                            dishInWedding.Wedding = wedding;

                            da.DishInWeddings.Add(dishInWedding);
                        }
                    });
                }

                da.SaveChanges();
                tx.Commit();

                return Created("api/Wedding/" + wedding.Id, new { Wedding = wedding });
            } catch
            {
                tx.Rollback();
                return BadRequest();
            }
        }

        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [HttpPost("{weddingId}/add-dish")]
        public IActionResult AddDishToExistingWedding(int weddingId, [FromBody] IEnumerable<DishInWeddingData> request)
        {
            IDbContextTransaction tx = da.Database.BeginTransaction();
            if (request == null) return BadRequest(new { Message = "Your request is empty." });
            try
            {
                Wedding existing = da.Weddings.First(w => w.Id == weddingId);
                if (existing.PaidDate != null)
                {
                    return BadRequest(new { Message = "You cannot change any things from All Paid Wedding."});
                }

                LinkedList<int> dishErrors = new LinkedList<int>();

                foreach (DishInWeddingData data in request) {
                    if (data.DishId.Equals(null))
                    {
                        continue;
                    }
                    Dish dish = da.Dishes.FirstOrDefault(d => d.Id == data.DishId);
                    if (dish == null)
                    {
                        dishErrors.AddLast(data.DishId);
                        continue;
                    }
                    if (!data.Id.Equals(null)) //existing record
                    {
                        DishInWedding existingDishInWedding = da.DishInWeddings.FirstOrDefault(d=>d.Id == data.Id || (d.WeddingId == data.WeddingId && d.DishId == data.DishId));

                        //when dishInWedding not existing \\ current dishObject is not the same as the old one -> create new
                        if (existingDishInWedding == null || (!existingDishInWedding.DishId.Equals(data.DishId)
                            || !existingDishInWedding.DishPrice.Equals(dish.Price) || !existingDishInWedding.DishDiscount.Equals(dish.Discount)))
                        {
                            DishInWedding newOne = AddDishToExistingWedding(dish, existing, data.Quantity);
                            da.DishInWeddings.Add(newOne);
                        }
                        else
                        {
                            if (data.Quantity.Equals(null)) existingDishInWedding.Quantity += 1;
                            else existingDishInWedding.Quantity = data.Quantity;
                        }
                    } else //create new record
                    {
                        DishInWedding newOne = AddDishToExistingWedding(dish, existing, data.Quantity);
                        da.DishInWeddings.Add(newOne);
                    }
                }

                if (dishErrors.Count > 0)
                {
                    return BadRequest(new { Message = "Some dishes isn't existing in the system.",
                                            DishIds = dishErrors });
                }

                da.SaveChanges();
                tx.Commit();

                return Accepted();
                 
            } catch
            {
                tx.Rollback();
                return NotFound(new { Message = "Wedding Order ID: " + weddingId + " isn't existing in the system." });
            }

        }

        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [HttpPost("{weddingId}/remove-dish")]
        public IActionResult RemoveDishFromExistingWedding(int weddingId, [FromBody] IEnumerable<int> request) //that's is list of DishInWedding ids
        {
            if (request == null) return BadRequest(new { Message = "Your request is empty." });
            try
            {
                Wedding existing = da.Weddings.Include(w => w.DishInWeddings).First(w => w.Id == weddingId);

                if (existing.PaidDate != null)
                {
                    return BadRequest(new { Message = "You cannot change any things from All Paid Wedding." });
                }

                foreach (int dishInWeddingId in request)
                {
                    DishInWedding existingDishInWedding = existing.DishInWeddings.FirstOrDefault(diw => diw.Id == dishInWeddingId);
                    if (existingDishInWedding != null) da.DishInWeddings.Remove(existingDishInWedding);
                }

                da.SaveChanges();

                return Accepted();
            }
            catch
            {
                return NotFound(new { Message = "Wedding Order ID: " + weddingId + " isn't existing in the system." });
            }

        }


        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public IActionResult UpdateExistingWedding(int id, [FromBody] WeddingData request)
        {
            IDbContextTransaction tx = da.Database.BeginTransaction();
            try
            {
                if (request == null) return BadRequest();

                Wedding existing = da.Weddings.First(w => w.Id == id);
                Hall hall = null;
                IDictionary<String, String> errors = new Dictionary<String, String>();
                if (!request.HallId.Equals(existing.HallId) && !request.HallId.Equals(null))
                {
                    hall = da.Halls.FirstOrDefault(h => h.Id == request.HallId);
                    if (hall == null)
                    {
                        errors.Add("HallId", "Chosen hall is not valid in system.");
                    }
                }

                if (request.Description != null && request.Description.Length > 0) existing.Description = request.Description;
                if (request.TableNumber == 0)
                    //errors.Add("TableNumber", "Table number shouldn't have been equals to 0.");
                    existing.TableNumber = request.TableNumber;
                if (request.GuestNumber > 0) 
                    //errors.Add("GuestNumber", "Guest number shouldn't have been equals to 0.");
                    existing.GuestNumber = request.GuestNumber;
                if (existing.Deposit.HasValue && existing.CreatedDate != null && existing.PaidDate != null) //has all paid
                { //can change receiptNo and PaidVia
                    if (request.ReceiptNo != null && request.ReceiptNo != "") existing.ReceiptNo = request.ReceiptNo;
                    if (request.PaidVia != null && request.PaidVia != "") existing.PaidVia = request.PaidVia;
                    if (request.DepositReceiptNo != null && request.DepositReceiptNo != "") existing.DepositReceiptNo = request.DepositReceiptNo;
                    if (request.DepositVia != null && request.DepositVia != null) existing.DepositVia = request.DepositVia;
                }
                if (request.CelebrityDate.HasValue)
                {
                    if (request.CelebrityDate.Value.Date >= DateTime.Now.AddDays(3).Date)
                    {
                        existing.CelebrityDate = request.CelebrityDate;
                    }
                    else
                    {
                        errors.Add("CelebrityDate", "Celerity date change at least more 3 days from now.");
                    }
                }
                if (!request.HallId.Equals(null) && hall != null) {
                    existing.HallId = hall.Id;
                    existing.HallPrice = hall.Price;
                    existing.HallDiscount = hall.Discount.GetValueOrDefault(0);
                }

                //You can update dishInWeddings (dishList in this wedding) by add/remove dish api
                //This api just can update wedding information and hall info, except dish orders

                if (errors.Count > 0)
                {
                    return BadRequest(new { Error = JsonConvert.SerializeObject(errors) });
                }

                da.SaveChanges();
                tx.Commit();

                return Accepted();
            } catch
            {
                tx.Rollback();
                return BadRequest(new { Message = "System cannot handle your request." });
            }
        }

        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [HttpPost("{weddingId}/deposit")]
        public IActionResult Deposit(int weddingId, [FromForm] String depositReceiptNo
                , [FromForm] String depositVia, [FromHeader] String Authorization, [FromForm] decimal deposit = 0)
        {
            try
            {
                Wedding existing = da.Weddings.First(w => w.Id == weddingId);
                if (existing.Deposit > 0 && existing.CreatedDate != null && existing.DepositVia != null)
                {
                    return BadRequest(new { Error = "This wedding has been deposit." });
                }

                String tokenString = Authorization.Replace("Bearer ", "");
                User user = UserPasswordUtils.GetCurrentUser(tokenString);

                IDictionary<String, String> errors = new Dictionary<String, String>();

                if (deposit > 0)
                {
                    existing.Deposit = deposit;
                }
                else errors.Add("deposit", "Deposit cannot be set by value 0.");
                if (depositReceiptNo != null && depositReceiptNo != "") existing.DepositReceiptNo = depositReceiptNo;
                else errors.Add("DepositReceiptNo", "Deposit receipt No cannot be empty.");
                if (depositVia != null && depositVia != "") existing.DepositVia = depositVia;
                else errors.Add("DepositVia", "Deposit via cannot be emty.");

                if (errors.Count == 0)
                {
                    existing.CreatedDate = DateTime.Now;
                    existing.StaffId = user.Id;
                } else
                {
                    return BadRequest(new { Error = JsonConvert.SerializeObject(errors) });
                }

                da.SaveChanges();

                return Accepted();
            } catch
            {
                return NotFound(new { Message = "Wedding Order ID: " + weddingId + " isn't existing in the system." });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("{weddingId}/pay")]
        public IActionResult PayTotalLeft(int weddingId, [FromForm] String receiptNo, [FromForm] String paidVia, [FromForm] decimal totalLeft = 0)
        {
            try
            {
                var wedding = da.Weddings.Where(w => w.Id == weddingId).Include(w => w.DishInWeddings).Include(w => w.Hall)
                   .Select(wedding => new { wedding, Total = da.CalculateWeddingOrder(wedding.Id) }).FirstOrDefault();
                Wedding existing = wedding.wedding;

                if (existing == null) throw new ArgumentNullException();
                if (existing.Deposit == 0) return BadRequest(new { Error = "You must deposit for this wedding before to pay it." });
                if (existing.PaidDate != null) return BadRequest(new { Error = "This wedding has been paid before. You don't need to check out it again." });

                bool totalFlag = true;
                IDictionary<String, String> errors = new Dictionary<String, String>();

                decimal calculatedTotalLeft = wedding.Total - existing.Deposit.Value;
                if (!(totalLeft > 0 && totalLeft.Equals(calculatedTotalLeft)))
                {
                    totalFlag = false;
                    errors.Add("TotalLeft", "Total left must have been paid is not matched with system. Try again.");
                }
                if (totalFlag)
                {
                    if (receiptNo != null && receiptNo != "") existing.ReceiptNo = receiptNo;
                    else errors.Add("ReceiptNo", "Receipt No cannot be empty.");
                    if (paidVia != null && paidVia != "") existing.PaidVia = paidVia;
                    else errors.Add("PaidVia", "Paid via cannot be empty.");
                }

                if (totalFlag && errors.Count == 0)
                {
                    existing.PaidDate = DateTime.Now;
                } else
                {
                    return BadRequest(new { Error = JsonConvert.SerializeObject(errors) });
                }

                da.SaveChanges();

                return Accepted();
            }
            catch
            {
                return NotFound(new { Message = "Wedding Order ID: " + weddingId + " isn't existing in the system." });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                Wedding existing = da.Weddings.First(w => w.Id == id);

                IEnumerable<DishInWedding> dishInWeddings = existing.DishInWeddings;
                da.DishInWeddings.RemoveRange(dishInWeddings);

                da.Weddings.Remove(existing);
                da.SaveChanges();

                return NoContent();
            } catch
            {
                return NotFound(new { Message = "Wedding Order ID: " + id + " isn't existing in the system."});
            }
        }

        private DishInWedding AddDishToExistingWedding(Dish dish, Wedding wedding, int quantity)
        {
            if (quantity.Equals(null)) quantity = 1;
            DishInWedding newOne = new DishInWedding();
            newOne.DishId = dish.Id;
            newOne.DishPrice = dish.Price;
            newOne.DishDiscount = dish.Discount.GetValueOrDefault(0);
            newOne.Quantity = quantity;
            newOne.WeddingId = wedding.Id;

            return newOne;
        }
    }
}
