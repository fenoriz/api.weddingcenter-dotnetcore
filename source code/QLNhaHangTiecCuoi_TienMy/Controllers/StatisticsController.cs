using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QLNhaHangTiecCuoi_TienMy.Models;
using Microsoft.EntityFrameworkCore;
using QLNhaHangTiecCuoi_TienMy.Models.Sp_Fn_DataSet;
using QLNhaHangTiecCuoi_TienMy.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace QLNhaHangTiecCuoi_TienMy.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        ApplicationDbContext da = new ApplicationDbContext();

        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [HttpGet("wedding-stats")]
        public IActionResult WeddingStatsPerMonthYear([FromQuery] int year, [FromQuery] int month = 0)
        {
            try
            {
                if (month == 0)
                {
                    return Ok(da.RevenueStats.FromSqlRaw("SELECT * FROM dbo.WeddingRevenueStatistics({0}, NULL)", year).ToList());
                }
                return Ok(da.MonthRevenueStats.FromSqlRaw($"SELECT * FROM dbo.GetWeddingRevenueStatsOfMonth({month}, {year})").ToList());
            } catch
            {
                return NotFound(new { Message = "Cannot stats at the present." });
            }
        }

        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [HttpGet("top5-employees")]
        public IActionResult Top5Employee([FromQuery] int year , [FromQuery] int month)
        {
            try
            {
                return Ok(da.Users.FromSqlRaw($"EXEC dbo.Top5EmployeesOrder {month}, {year}"));
            } catch
            {
                return NotFound(new { Message = "Cannot stats at the present." });
            }
        }


    }
}
