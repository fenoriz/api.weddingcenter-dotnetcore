using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLNhaHangTiecCuoi_TienMy.Models.Data
{
    public class WeddingData
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int StaffId { get; set; }
        public string Description { get; set; }
        public int TableNumber { get; set; }
        public int GuestNumber { get; set; }
        public decimal Deposit { get; set; }
        public string DepositVia { get; set; }
        public string DepositReceiptNo { get; set; }
        public DateTime PaidDate { get; set; }
        public string ReceiptNo { get; set; }
        public string PaidVia { get; set; }
        public DateTime? CelebrityDate { get; set; }
        public int HallId { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<DishInWeddingData> dishOrders;
    }
}
