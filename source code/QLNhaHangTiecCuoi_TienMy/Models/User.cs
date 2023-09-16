using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace QLNhaHangTiecCuoi_TienMy.Models
{
    [Index(nameof(Username), Name = "UQ__Users__536C85E441E8D03D", IsUnique = true)]
    [Index(nameof(Phone), Name = "UQ__Users__5C7E359EB29607C5", IsUnique = true)]
    public partial class User
    {
        public User()
        {
            Employees = new HashSet<Employee>();
            WeddingCustomers = new HashSet<Wedding>();
            WeddingStaffs = new HashSet<Wedding>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(25)]
        public string Username { get; set; }
        [Required]
        [StringLength(255)]
        public string Password { get; set; }
        [Required]
        [StringLength(15)]
        public string Phone { get; set; }
        [Required]
        [StringLength(25)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [StringLength(20)]
        public string IdentityNumber { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? CreatedDate { get; set; }
        [StringLength(10)]
        public string Role { get; set; }

        [InverseProperty(nameof(Employee.User))]
        public virtual ICollection<Employee> Employees { get; set; }
        [InverseProperty(nameof(Wedding.Customer))]
        public virtual ICollection<Wedding> WeddingCustomers { get; set; }
        [InverseProperty(nameof(Wedding.Staff))]
        public virtual ICollection<Wedding> WeddingStaffs { get; set; }
    }
}
