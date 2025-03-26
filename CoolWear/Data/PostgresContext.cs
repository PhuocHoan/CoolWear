using CoolWear.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace CoolWear.Data;

public partial class PostgresContext(DbContextOptions<PostgresContext> options) : DbContext(options)
{
    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductColor> ProductColors { get; set; }

    public virtual DbSet<ProductColorLink> ProductColorLinks { get; set; }

    public virtual DbSet<ProductSize> ProductSizes { get; set; }

    public virtual DbSet<ProductSizeLink> ProductSizeLinks { get; set; }

    public virtual DbSet<StoreOwner> StoreOwners { get; set; }

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

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("order_item_pkey");

            entity.ToTable("order_item", tb => tb.HasComment("Bảng lưu trữ thông tin chi tiết đơn hàng"));

            entity.Property(e => e.OrderItemId)
                .HasComment("Mã chi tiết đơn hàng (khóa chính)")
                .HasColumnName("order_item_id");
            entity.Property(e => e.OrderId)
                .HasComment("Mã đơn hàng (khóa ngoại)")
                .HasColumnName("order_id");
            entity.Property(e => e.ProductId)
                .HasComment("Mã sản phẩm (khóa ngoại)")
                .HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasComment("Số lượng")
                .HasColumnName("quantity");
            entity.Property(e => e.UnitPrice)
                .HasComment("Đơn giá")
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_item_order_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_item_product_id_fkey");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("payment_method_pkey");

            entity.ToTable("payment_method", tb => tb.HasComment("Bảng lưu trữ thông tin phương thức thanh toán"));

            entity.HasIndex(e => e.PaymentMethodName, "unique_payment_method_name").IsUnique();

            entity.Property(e => e.PaymentMethodId)
                .HasComment("Mã phương thức thanh toán (khóa chính)")
                .HasColumnName("payment_method_id");
            entity.Property(e => e.PaymentMethodName)
                .HasComment("Tên phương thức thanh toán")
                .HasColumnType("character varying")
                .HasColumnName("payment_method_name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("product_pkey");

            entity.ToTable("product", tb => tb.HasComment("Bảng lưu trữ thông tin sản phẩm"));

            entity.HasIndex(e => e.ProductName, "unique_product_name").IsUnique();

            entity.Property(e => e.ProductId)
                .HasComment("Mã sản phẩm (khóa chính)")
                .HasColumnName("product_id");
            entity.Property(e => e.CategoryId)
                .HasComment("Mã danh mục (khóa ngoại)")
                .HasColumnName("category_id");
            entity.Property(e => e.ImportPrice)
                .HasComment("Giá nhập")
                .HasColumnName("import_price");
            entity.Property(e => e.Price)
                .HasComment("Giá bán")
                .HasColumnName("price");
            entity.Property(e => e.ProductName)
                .HasComment("Tên sản phẩm")
                .HasColumnType("character varying")
                .HasColumnName("product_name");
            entity.Property(e => e.PublicId)
                .HasComment("Đường dẫn tới ảnh sản phẩm")
                .HasColumnType("character varying")
                .HasColumnName("public_id");
            entity.Property(e => e.StockQuantity)
                .HasComment("Số lượng tồn kho")
                .HasColumnName("stock_quantity");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_category_id_fkey");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("product_category_pkey");

            entity.ToTable("product_category", tb => tb.HasComment("Bảng lưu trữ danh mục sản phẩm (loại áo/quần)"));

            entity.HasIndex(e => e.CategoryName, "unique_category_name").IsUnique();

            entity.Property(e => e.CategoryId)
                .HasComment("Mã danh mục (khóa chính)")
                .HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasComment("Tên danh mục")
                .HasColumnType("character varying")
                .HasColumnName("category_name");
            entity.Property(e => e.ProductType)
                .HasComment("Loại sản phẩm")
                .HasColumnType("character varying")
                .HasColumnName("product_type");
        });

        modelBuilder.Entity<ProductColor>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("product_color_pkey");

            entity.ToTable("product_color", tb => tb.HasComment("Bảng lưu trữ thông tin màu sắc sản phẩm"));

            entity.HasIndex(e => e.ColorName, "unique_color_name").IsUnique();

            entity.Property(e => e.ColorId)
                .HasComment("Mã màu (khóa chính)")
                .HasColumnName("color_id");
            entity.Property(e => e.ColorName)
                .HasComment("Tên màu")
                .HasColumnType("character varying")
                .HasColumnName("color_name");
        });

        modelBuilder.Entity<ProductColorLink>(entity =>
        {
            entity.HasKey(e => e.ProductColorId).HasName("product_color_link_pkey");

            entity.ToTable("product_color_link", tb => tb.HasComment("Bảng lưu trữ liên kết giữa sản phẩm và màu sắc"));

            entity.Property(e => e.ProductColorId)
                .HasComment("Mã liên kết (khóa chính)")
                .HasColumnName("product_color_id");
            entity.Property(e => e.ColorId)
                .HasComment("Mã màu (khóa ngoại)")
                .HasColumnName("color_id");
            entity.Property(e => e.ProductId)
                .HasComment("Mã sản phẩm (khóa ngoại)")
                .HasColumnName("product_id");

            entity.HasOne(d => d.Color).WithMany(p => p.ProductColorLinks)
                .HasForeignKey(d => d.ColorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_color_link_color_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductColorLinks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_color_link_product_id_fkey");
        });

        modelBuilder.Entity<ProductSize>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("product_size_pkey");

            entity.ToTable("product_size", tb => tb.HasComment("Bảng lưu trữ thông tin kích thước sản phẩm"));

            entity.HasIndex(e => e.SizeName, "unique_size_name").IsUnique();

            entity.Property(e => e.SizeId)
                .HasComment("Mã kích thước (khóa chính)")
                .HasColumnName("size_id");
            entity.Property(e => e.SizeName)
                .HasComment("Tên kích thước")
                .HasColumnType("character varying")
                .HasColumnName("size_name");
        });

        modelBuilder.Entity<ProductSizeLink>(entity =>
        {
            entity.HasKey(e => e.ProductSizeId).HasName("product_size_link_pkey");

            entity.ToTable("product_size_link", tb => tb.HasComment("Bảng lưu trữ liên kết giữa sản phẩm và kích thước"));

            entity.Property(e => e.ProductSizeId)
                .HasComment("Mã liên kết (khóa chính)")
                .HasColumnName("product_size_id");
            entity.Property(e => e.ProductId)
                .HasComment("Mã sản phẩm (khóa ngoại)")
                .HasColumnName("product_id");
            entity.Property(e => e.SizeId)
                .HasComment("Mã kích thước (khóa ngoại)")
                .HasColumnName("size_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductSizeLinks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_size_link_product_id_fkey");

            entity.HasOne(d => d.Size).WithMany(p => p.ProductSizeLinks)
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_size_link_size_id_fkey");
        });

        modelBuilder.Entity<StoreOwner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("store_owner_pkey");

            entity.ToTable("store_owner", tb => tb.HasComment("Bảng lưu trữ thông tin chủ cửa hàng"));

            entity.Property(e => e.OwnerId)
                .HasComment("Mã chủ cửa hàng (khóa chính)")
                .HasColumnName("owner_id");
            entity.Property(e => e.Address)
                .HasComment("Địa chỉ chủ cửa hàng")
                .HasColumnType("character varying")
                .HasColumnName("address");
            entity.Property(e => e.Email)
                .HasComment("Email chủ cửa hàng")
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.OwnerName)
                .HasComment("Tên chủ cửa hàng")
                .HasColumnType("character varying")
                .HasColumnName("owner_name");
            entity.Property(e => e.Password)
                .HasComment("Mật khẩu đã mã hóa")
                .HasColumnType("character varying")
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasComment("Số điện thoại chủ cửa hàng")
                .HasColumnType("character varying")
                .HasColumnName("phone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
