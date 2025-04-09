using CoolWear.Models;
using Microsoft.EntityFrameworkCore;

namespace CoolWear.Data;

public partial class PostgresContext : DbContext
{
    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductColor> ProductColors { get; set; }

    public virtual DbSet<ProductSize> ProductSizes { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<StoreOwner> StoreOwners { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customer_pkey");

            entity.ToTable("customer", tb => tb.HasComment("Bảng khách hàng"));

            entity.Property(e => e.CustomerId)
                .HasComment("Mã khách hàng, khóa chính, tự động tăng")
                .HasColumnName("customer_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsRequired()
                .HasComment("Địa chỉ khách hàng")
                .HasColumnName("address");
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("now()")
                .HasComment("Ngày tạo tài khoản, mặc định là thời điểm hiện tại")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_date");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(100)
                .HasComment("Tên khách hàng")
                .HasColumnName("customer_name");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasComment("Email khách hàng")
                .HasColumnName("email");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasComment("Trạng thái xóa khách hàng, mặc định là chưa (false)")
                .HasColumnName("is_deleted");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsRequired()
                .HasComment("Số điện thoại khách hàng")
                .HasColumnName("phone");
            entity.Property(e => e.Points)
                .HasDefaultValue(0)
                .HasComment("Điểm tích lũy của khách hàng, mặc định là 0")
                .HasColumnName("points");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("order_pkey");

            entity.ToTable("order", tb => tb.HasComment("Bảng đơn hàng"));

            entity.Property(e => e.OrderId)
                .HasComment("Mã đơn hàng, khóa chính, tự động tăng")
                .HasColumnName("order_id");
            entity.Property(e => e.CustomerId)
                .HasComment("Mã khách hàng, khóa ngoại (có thể null nếu khách hàng không đăng nhập)")
                .HasColumnName("customer_id");
            entity.Property(e => e.NetTotal)
                .HasComment("Tổng tiền đơn hàng sau khi giảm giá")
                .HasColumnName("net_total");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("now()")
                .HasComment("Ngày đặt hàng, mặc định là thời điểm hiện tại")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("order_date");
            entity.Property(e => e.PaymentMethodId)
                .HasComment("Mã phương thức thanh toán, khóa ngoại")
                .HasColumnName("payment_method_id");
            entity.Property(e => e.PointUsed)
                .HasDefaultValue(0)
                .HasComment("Số điểm sử dụng, mặc định là 0")
                .HasColumnName("point_used");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Đang xử lý'::character varying")
                .HasComment("Trạng thái đơn hàng, mặc định là \"Đang xử lý\"")
                .HasColumnName("status");
            entity.Property(e => e.Subtotal)
                .HasComment("Tổng tiền đơn hàng trước khi giảm giá")
                .HasColumnName("subtotal");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("order_customer_id_fkey");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_payment_method_id_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("order_item_pkey");

            entity.ToTable("order_item", tb => tb.HasComment("Bảng chi tiết đơn hàng"));

            entity.Property(e => e.OrderItemId)
                .HasComment("Mã chi tiết đơn hàng, khóa chính, tự động tăng")
                .HasColumnName("order_item_id");
            entity.Property(e => e.OrderId)
                .HasComment("Mã đơn hàng, khóa ngoại")
                .HasColumnName("order_id");
            entity.Property(e => e.Quantity)
                .HasComment("Số lượng sản phẩm")
                .HasColumnName("quantity");
            entity.Property(e => e.UnitPrice)
                .HasComment("Đơn giá")
                .HasColumnName("unit_price");
            entity.Property(e => e.VariantId)
                .HasComment("Mã biến thể sản phẩm, khóa ngoại")
                .HasColumnName("variant_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_item_order_id_fkey");

            entity.HasOne(d => d.Variant).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_item_variant_id_fkey");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("payment_method_pkey");

            entity.ToTable("payment_method", tb => tb.HasComment("Bảng phương thức thanh toán"));

            entity.HasIndex(e => e.PaymentMethodName, "payment_method_payment_method_name_key").IsUnique();

            entity.Property(e => e.PaymentMethodId)
                .HasComment("Mã phương thức thanh toán, khóa chính, tự động tăng")
                .HasColumnName("payment_method_id");
            entity.Property(e => e.PaymentMethodName)
                .HasMaxLength(30)
                .HasComment("Tên phương thức thanh toán, duy nhất")
                .HasColumnName("payment_method_name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("product_pkey");

            entity.ToTable("product", tb => tb.HasComment("Bảng sản phẩm"));

            entity.HasIndex(e => e.ProductName, "product_product_name_key").IsUnique();

            entity.Property(e => e.ProductId)
                .HasComment("Mã sản phẩm, khóa chính, tự động tăng")
                .HasColumnName("product_id");
            entity.Property(e => e.CategoryId)
                .HasComment("Mã danh mục sản phẩm, khóa ngoại")
                .HasColumnName("category_id");
            entity.Property(e => e.ImportPrice)
                .HasComment("Giá nhập sản phẩm")
                .HasColumnName("import_price");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasComment("Trạng thái xóa sản phẩm, mặc định là chưa (false)")
                .HasColumnName("is_deleted");
            entity.Property(e => e.Price)
                .HasComment("Giá bán sản phẩm")
                .HasColumnName("price");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasComment("Tên sản phẩm, duy nhất")
                .HasColumnName("product_name");
            entity.Property(e => e.PublicId)
                .HasMaxLength(255)
                .HasComment("Đường dẫn hình ảnh công khai")
                .HasColumnName("public_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull) // Để không xóa sản phẩm khi xóa danh mục
                .HasConstraintName("product_category_id_fkey");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("product_category_pkey");

            entity.ToTable("product_category", tb => tb.HasComment("Bảng danh mục sản phẩm"));

            entity.HasIndex(e => e.CategoryName, "product_category_category_name_key").IsUnique();

            entity.Property(e => e.CategoryId)
                .HasComment("Mã danh mục sản phẩm, khóa chính")
                .HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasComment("Tên danh mục sản phẩm, duy nhất")
                .HasColumnName("category_name");
            entity.Property(e => e.ProductType)
                .HasMaxLength(20)
                .HasComment("Loại sản phẩm (áo, quần,...)")
                .HasColumnName("product_type");
        });

        modelBuilder.Entity<ProductColor>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("product_color_pkey");

            entity.ToTable("product_color", tb => tb.HasComment("Bảng màu sắc sản phẩm"));

            entity.HasIndex(e => e.ColorName, "product_color_color_name_key").IsUnique();

            entity.Property(e => e.ColorId)
                .HasComment("Mã màu sắc, khóa chính, tự động tăng")
                .HasColumnName("color_id");
            entity.Property(e => e.ColorName)
                .HasMaxLength(20)
                .HasComment("Tên màu sắc, duy nhất")
                .HasColumnName("color_name");
        });

        modelBuilder.Entity<ProductSize>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("product_size_pkey");

            entity.ToTable("product_size", tb => tb.HasComment("Bảng kích thước sản phẩm"));

            entity.HasIndex(e => e.SizeName, "product_size_size_name_key").IsUnique();

            entity.Property(e => e.SizeId)
                .HasComment("Mã kích thước, khóa chính, tự động tăng")
                .HasColumnName("size_id");
            entity.Property(e => e.SizeName)
                .HasMaxLength(10)
                .HasComment("Tên kích thước, duy nhất")
                .HasColumnName("size_name");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("product_variant_pkey");

            entity.ToTable("product_variant", tb => tb.HasComment("Bảng biến thể sản phẩm"));

            entity.HasIndex(e => new { e.ProductId, e.ColorId, e.SizeId }, "product_variant_product_id_color_id_size_id_key").IsUnique();

            entity.Property(e => e.VariantId)
                .HasComment("Mã biến thể sản phẩm, khóa chính, tự động tăng")
                .HasColumnName("variant_id");
            entity.Property(e => e.ColorId)
                .HasComment("Mã màu sắc, khóa ngoại")
                .HasColumnName("color_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasComment("Trạng thái xóa biến thể sản phẩm, mặc định là chưa (false)")
                .HasColumnName("is_deleted");
            entity.Property(e => e.ProductId)
                .HasComment("Mã sản phẩm, khóa ngoại")
                .HasColumnName("product_id");
            entity.Property(e => e.SizeId)
                .HasComment("Mã kích thước, khóa ngoại")
                .HasColumnName("size_id");
            entity.Property(e => e.StockQuantity)
                .HasComment("Số lượng tồn kho")
                .HasColumnName("stock_quantity");

            entity.HasOne(d => d.Color).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ColorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("product_variant_color_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("product_variant_product_id_fkey");

            entity.HasOne(d => d.Size).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("product_variant_size_id_fkey");
        });

        modelBuilder.Entity<StoreOwner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("store_owner_pkey");

            entity.ToTable("store_owner", tb => tb.HasComment("Bảng chủ cửa hàng"));

            entity.Property(e => e.OwnerId)
                .HasComment("Mã chủ cửa hàng, khóa chính, tự động tăng")
                .HasColumnName("owner_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasComment("Địa chỉ chủ cửa hàng")
                .HasColumnName("address");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasComment("Email chủ cửa hàng")
                .HasColumnName("email");
            entity.Property(e => e.Entropy)
                .HasMaxLength(100)
                .HasDefaultValueSql("'0'::character varying")
                .HasComment("Mã hóa mật khẩu, mặc định là 0")
                .HasColumnName("entropy");
            entity.Property(e => e.OwnerName)
                .HasMaxLength(100)
                .HasComment("Tên chủ cửa hàng")
                .HasColumnName("owner_name");
            entity.Property(e => e.Password)
                .HasMaxLength(350)
                .HasComment("Mật khẩu")
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasComment("Số điện thoại chủ cửa hàng")
                .HasColumnName("phone");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasComment("Tên đăng nhập")
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
