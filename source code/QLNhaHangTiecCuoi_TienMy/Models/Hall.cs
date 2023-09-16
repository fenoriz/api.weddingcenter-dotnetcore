using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace QLNhaHangTiecCuoi_TienMy.Models
{
    [Index(nameof(Name), Name = "UQ__Halls__737584F6158E7804", IsUnique = true)]
    public partial class Hall
    {
        public Hall()
        {
            Weddings = new HashSet<Wedding>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(70)]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public int TableNumber { get; set; }
        public int GuestUpTo { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal Price { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal? Discount { get; set; }
        public bool? IsActive { get; set; }

        [InverseProperty(nameof(Wedding.Hall))]
        public virtual ICollection<Wedding> Weddings { get; set; }
    }
}
