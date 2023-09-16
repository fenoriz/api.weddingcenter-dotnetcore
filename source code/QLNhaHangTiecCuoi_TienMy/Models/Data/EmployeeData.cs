using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QLNhaHangTiecCuoi_TienMy.Models.Data
{
    public class EmployeeData
    {
        public int Id { get; set; }
        public DateTime DateOfBirth { get; set; }
        public byte[] Picture { get; set; }
        public IFormFile File { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserId { get; set; }
    }
}
