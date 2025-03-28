using CoolWear.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace CoolWear.Data;

public partial class PostgresContext : DbContext
{
    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customer_pkey");

            entity.ToTable("customer", tb => tb.HasComment("Bảng lưu trữ thông tin khách hàng"));

            entity.Property(e => e.CustomerId)
                .HasComment("Mã khách hàng (khóa chính)")
                .HasColumnName("customer_id");
            entity.Property(e => e.Address)
                .HasComment("Địa chỉ khách hàng")
                .HasColumnType("character varying")
                .HasColumnName("address");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("now()")
                .HasComment("Ngày tạo tài khoản")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_date");
            entity.Property(e => e.CustomerName)
                .HasComment("Tên khách hàng")
                .HasColumnType("character varying")
                .HasColumnName("customer_name");
            entity.Property(e => e.Email)
                .HasComment("Email khách hàng")
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.Phone)
                .HasComment("Số điện thoại khách hàng")
                .HasColumnType("character varying")
                .HasColumnName("phone");
            entity.Property(e => e.Points)
                .HasDefaultValue(0)
                .HasComment("Điểm tích lũy")
                .HasColumnName("points");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("order_pkey");

            entity.ToTable("order", tb => tb.HasComment("Bảng lưu trữ thông tin đơn hàng"));

            entity.Property(e => e.OrderId)
                .HasComment("Mã đơn hàng (khóa chính)")
                .HasColumnName("order_id");
            entity.Property(e => e.CustomerId)
                .HasComment("Mã khách hàng (khóa ngoại)")
                .HasColumnName("customer_id");
            entity.Property(e => e.IsRefunded)
                .HasDefaultValue(false)
                .HasComment("Trạng thái hoàn tiền")
                .HasColumnName("is_refunded");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("now()")
                .HasComment("Ngày đặt hàng")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("order_date");
            entity.Property(e => e.PaymentMethodId)
                .HasComment("Mã phương thức thanh toán (khóa ngoại)")
                .HasColumnName("payment_method_id");
            entity.Property(e => e.PointUsed)
                .HasDefaultValue(0)
                .HasComment("Số điểm sử dụng để thanh toán (Tích điểm sẽ tính trước khi dùng điểm thưởng để thanh toán")
                .HasColumnName("point_used");
            entity.Property(e => e.TotalAmount)
                .HasComment("Tổng số tiền")
                .HasColumnName("total_amount");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_customer_id_fkey");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_payment_method_id_fkey");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("payment_method_pkey");

            entity.ToTable("payment_method", tb => tb.HasComment("Bảng lưu trữ thông tin phương thức thanh toán"));

            entity.HasIndex(e => e.PaymentMethodName, "payment_method_payment_method_name_key").IsUnique();

            entity.Property(e => e.PaymentMethodId)
                .HasComment("Mã phương thức thanh toán (khóa chính)")
                .HasColumnName("payment_method_id");
            entity.Property(e => e.PaymentMethodName)
                .HasComment("Tên phương thức thanh toán")
                .HasColumnType("character varying")
                .HasColumnName("payment_method_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
