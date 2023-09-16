using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace QLNhaHangTiecCuoi_TienMy.Models
{
    public partial class Wedding
    {
        public Wedding()
        {
            DishInWeddings = new HashSet<DishInWedding>();
        }

        [Key]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? StaffId { get; set; }
        [Required]
        public string Description { get; set; }
        public int TableNumber { get; set; }
        public int GuestNumber { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal? Deposit { get; set; }
        [StringLength(100)]
        public string DepositVia { get; set; }
        [StringLength(255)]
        public string DepositReceiptNo { get; set; }
        [Column(TypeName = "date")]
        public DateTime? PaidDate { get; set; }
        [StringLength(255)]
        public string ReceiptNo { get; set; }
        [StringLength(100)]
        public string PaidVia { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? CelebrityDate { get; set; }
        public int HallId { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal HallPrice { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal HallDiscount { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }

        [ForeignKey(nameof(CustomerId))]
        [InverseProperty(nameof(User.WeddingCustomers))]
        public virtual User Customer { get; set; }
        [ForeignKey(nameof(HallId))]
        [InverseProperty("Weddings")]
        public virtual Hall Hall { get; set; }
        [ForeignKey(nameof(StaffId))]
        [InverseProperty(nameof(User.WeddingStaffs))]
        public virtual User Staff { get; set; }
        [InverseProperty(nameof(DishInWedding.Wedding))]
        public virtual ICollection<DishInWedding> DishInWeddings { get; set; }
    }
}
