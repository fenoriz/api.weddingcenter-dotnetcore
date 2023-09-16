using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QLNhaHangTiecCuoi_TienMy.Models.Sp_Fn_DataSet;

#nullable disable

namespace QLNhaHangTiecCuoi_TienMy.Models
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Dish> Dishes { get; set; }
        public virtual DbSet<DishInWedding> DishInWeddings { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Hall> Halls { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Wedding> Weddings { get; set; }

        //CUSTOM DATASET
        public DbSet<RevenueStatsData> RevenueStats { get; set; }
        public DbSet<MonthRevenueStatsData> MonthRevenueStats { get; set; }
        public DbSet<RoleData> Roles { get; set; }

        public decimal CalculateWeddingOrder(int weddingId) => throw new NotSupportedException();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=NITRO-5-ACER\\DBTEST;Database=qltc;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Dish>(entity =>
            {
                entity.Property(e => e.Discount).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<DishInWedding>(entity =>
            {
                entity.Property(e => e.Quantity).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Dish)
                    .WithMany(p => p.DishInWeddings)
                    .HasForeignKey(d => d.DishId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Dish");

                entity.HasOne(d => d.Wedding)
                    .WithMany(p => p.DishInWeddings)
                    .HasForeignKey(d => d.WeddingId)
                    .OnDelete(DeleteBehavior.ClientCascade)
                    .HasConstraintName("Fk_Wedding");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientCascade)
                    .HasConstraintName("FK_User");
            });

            modelBuilder.Entity<Hall>(entity =>
            {
                entity.Property(e => e.Discount).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IdentityNumber).IsUnicode(false);

                entity.Property(e => e.Password).IsUnicode(false);

                entity.Property(e => e.Phone).IsUnicode(false);

                entity.Property(e => e.Role).IsUnicode(false);

                entity.Property(e => e.Username).IsUnicode(false);
            });

            modelBuilder.Entity<Wedding>(entity =>
            {
                entity.Property(e => e.Deposit).HasDefaultValueSql("((0))");

                entity.Property(e => e.DepositReceiptNo).IsUnicode(false);

                entity.Property(e => e.PaidVia).IsUnicode(false);

                entity.Property(e => e.ReceiptNo).IsUnicode(false);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.WeddingCustomers)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Fk_CustomerId");

                entity.HasOne(d => d.Hall)
                    .WithMany(p => p.Weddings)
                    .HasForeignKey(d => d.HallId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Fk_HallId");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.WeddingStaffs)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Fk_StaffId");
            });

            modelBuilder.Entity<RevenueStatsData>().HasNoKey();
            modelBuilder.Entity<MonthRevenueStatsData>().HasNoKey();
            modelBuilder.Entity<RoleData>().HasNoKey();

            modelBuilder.HasDbFunction(typeof(ApplicationDbContext).GetMethod(nameof(CalculateWeddingOrder), new[] { typeof(int) }))
                .HasName("CalculateTotalOfWedding");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
