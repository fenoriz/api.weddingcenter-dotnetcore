using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLNhaHangTiecCuoi_TienMy.Models.Data
{
    public class DishInWeddingData
    {
        public int Id { get; set; }
        public int WeddingId { get; set; }
        public int DishId { get; set; }
        public int Quantity { get; set; }
    }
}
