using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QLNhaHangTiecCuoi_TienMy.Models.Data
{
    public class DishData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public float Discount { get; set; }
        public byte[] Image { get; set; }
        public IFormFile File { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
