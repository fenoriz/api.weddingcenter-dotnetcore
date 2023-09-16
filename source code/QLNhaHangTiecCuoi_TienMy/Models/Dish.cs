using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace QLNhaHangTiecCuoi_TienMy.Models
{
    [Index(nameof(Name), Name = "UQ__Dishes__737584F65F4BEA6F", IsUnique = true)]
    public partial class Dish
    {
        public Dish()
        {
            DishInWeddings = new HashSet<DishInWedding>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(70)]
        public string Name { get; set; }
        [StringLength(255)]
        public string Description { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal Price { get; set; }
        public double? Discount { get; set; }
        [Column(TypeName = "image")]
        public byte[] Image { get; set; }
        public bool? IsActive { get; set; }

        [InverseProperty(nameof(DishInWedding.Dish))]
        public virtual ICollection<DishInWedding> DishInWeddings { get; set; }
    }
}
