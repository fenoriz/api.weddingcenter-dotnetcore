using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace QLNhaHangTiecCuoi_TienMy.Models
{
    [Table("_DishInWedding")]
    public partial class DishInWedding
    {
        [Key]
        public int Id { get; set; }
        public int WeddingId { get; set; }
        public int DishId { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal DishPrice { get; set; }
        public double DishDiscount { get; set; }
        public int? Quantity { get; set; }

        [ForeignKey(nameof(DishId))]
        [InverseProperty("DishInWeddings")]
        public virtual Dish Dish { get; set; }
        [ForeignKey(nameof(WeddingId))]
        [InverseProperty("DishInWeddings")]
        public virtual Wedding Wedding { get; set; }
    }
}
