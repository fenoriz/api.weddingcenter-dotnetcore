using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLNhaHangTiecCuoi_TienMy.Models.Data
{
    public class HallData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TableNumber { get; set; }
        public int GuestUpTo { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public bool? IsActive { get; set; }
    }
}
