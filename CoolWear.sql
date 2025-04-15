-- Bảng danh mục sản phẩm
CREATE TABLE "product_category" (
    "category_id" serial PRIMARY KEY,
    "category_name" varchar(50) NOT NULL UNIQUE,
    "product_type" varchar(20) NOT NULL
);

COMMENT ON TABLE "product_category" IS 'Bảng danh mục sản phẩm';
COMMENT ON COLUMN "product_category"."category_id" IS 'Mã danh mục sản phẩm, khóa chính, tự động tăng';
COMMENT ON COLUMN "product_category"."category_name" IS 'Tên danh mục sản phẩm, duy nhất';
COMMENT ON COLUMN "product_category"."product_type" IS 'Loại sản phẩm (áo, quần)';

-- Bảng màu sắc sản phẩm
CREATE TABLE "product_color" (
    "color_id" serial PRIMARY KEY,
    "color_name" varchar(20) NOT NULL UNIQUE,
    "is_deleted" boolean NOT NULL DEFAULT false,
);

COMMENT ON TABLE "product_color" IS 'Bảng màu sắc sản phẩm';
COMMENT ON COLUMN "product_color"."color_id" IS 'Mã màu sắc, khóa chính, tự động tăng';
COMMENT ON COLUMN "product_color"."color_name" IS 'Tên màu sắc, duy nhất';
COMMENT ON COLUMN "product_color"."is_deleted" IS 'Trạng thái xóa màu sắc, mặc định là chưa (false)';

-- Bảng kích thước sản phẩm
CREATE TABLE "product_size" (
    "size_id" serial PRIMARY KEY,
    "size_name" varchar(10) NOT NULL UNIQUE,
    "is_deleted" boolean NOT NULL DEFAULT false,
);

COMMENT ON TABLE "product_size" IS 'Bảng kích thước sản phẩm';
COMMENT ON COLUMN "product_size"."size_id" IS 'Mã kích thước, khóa chính, tự động tăng';
COMMENT ON COLUMN "product_size"."size_name" IS 'Tên kích thước, duy nhất';
COMMENT ON COLUMN "product_size"."is_deleted" IS 'Trạng thái xóa kích thước, mặc định là chưa (false)';

-- Bảng phương thức thanh toán
CREATE TABLE "payment_method" (
    "payment_method_id" serial PRIMARY KEY,
    "payment_method_name" varchar(30) NOT NULL UNIQUE
);

COMMENT ON TABLE "payment_method" IS 'Bảng phương thức thanh toán';
COMMENT ON COLUMN "payment_method"."payment_method_id" IS 'Mã phương thức thanh toán, khóa chính, tự động tăng';
COMMENT ON COLUMN "payment_method"."payment_method_name" IS 'Tên phương thức thanh toán, duy nhất';

-- Bảng khách hàng
CREATE TABLE "customer" (
    "customer_id" serial PRIMARY KEY,
    "customer_name" varchar(100) NOT NULL, 
    "email" varchar(100) UNIQUE, 
    "phone" varchar(20) NOT NULL UNIQUE, 
    "address" varchar(255) NOT NULL,
    "create_date" TIMESTAMP NOT NULL DEFAULT now(),
    "points" integer NOT NULL DEFAULT 0,
    "is_deleted" boolean NOT NULL DEFAULT false
);

COMMENT ON TABLE "customer" IS 'Bảng khách hàng';
COMMENT ON COLUMN "customer"."customer_id" IS 'Mã khách hàng, khóa chính, tự động tăng';
COMMENT ON COLUMN "customer"."customer_name" IS 'Tên khách hàng';
COMMENT ON COLUMN "customer"."email" IS 'Email khách hàng';
COMMENT ON COLUMN "customer"."phone" IS 'Số điện thoại khách hàng';
COMMENT ON COLUMN "customer"."address" IS 'Địa chỉ khách hàng';
COMMENT ON COLUMN "customer"."create_date" IS 'Ngày tạo tài khoản, mặc định là thời điểm hiện tại';
COMMENT ON COLUMN "customer"."points" IS 'Điểm tích lũy của khách hàng, mặc định là 0';
COMMENT ON COLUMN "customer"."is_deleted" IS 'Trạng thái xóa khách hàng, mặc định là chưa (false)';

-- Bảng chủ cửa hàng
CREATE TABLE "store_owner" (
    "owner_id" serial PRIMARY KEY,
    "owner_name" varchar(100) NOT NULL, 
    "email" varchar(100) NOT NULL, 
    "phone" varchar(20) NOT NULL, 
    "address" varchar(255) NOT NULL, 
    "username" varchar(50) NOT NULL, 
    "password" varchar(350) NOT NULL,
    "entropy" varchar(100) NOT NULL DEFAULT '0' -- Mã hóa mật khẩu
);

COMMENT ON TABLE "store_owner" IS 'Bảng chủ cửa hàng';
COMMENT ON COLUMN "store_owner"."owner_id" IS 'Mã chủ cửa hàng, khóa chính, tự động tăng';
COMMENT ON COLUMN "store_owner"."owner_name" IS 'Tên chủ cửa hàng';
COMMENT ON COLUMN "store_owner"."email" IS 'Email chủ cửa hàng';
COMMENT ON COLUMN "store_owner"."phone" IS 'Số điện thoại chủ cửa hàng';
COMMENT ON COLUMN "store_owner"."address" IS 'Địa chỉ chủ cửa hàng';
COMMENT ON COLUMN "store_owner"."username" IS 'Tên đăng nhập';
COMMENT ON COLUMN "store_owner"."password" IS 'Mật khẩu';
COMMENT ON COLUMN "store_owner"."entropy" IS 'Mã hóa mật khẩu, mặc định là 0';

-- Bảng sản phẩm
CREATE TABLE "product" (
    "product_id" serial PRIMARY KEY,
    "product_name" varchar(100) NOT NULL UNIQUE,
    "import_price" integer NOT NULL,
    "price" integer NOT NULL,
    "category_id" integer,
    "public_id" varchar(255) NOT NULL,
    "is_deleted" boolean NOT NULL DEFAULT false,
    FOREIGN KEY ("category_id") REFERENCES "product_category" ("category_id")
);

COMMENT ON TABLE "product" IS 'Bảng sản phẩm';
COMMENT ON COLUMN "product"."product_id" IS 'Mã sản phẩm, khóa chính, tự động tăng';
COMMENT ON COLUMN "product"."product_name" IS 'Tên sản phẩm, duy nhất';
COMMENT ON COLUMN "product"."import_price" IS 'Giá nhập sản phẩm';
COMMENT ON COLUMN "product"."price" IS 'Giá bán sản phẩm';
COMMENT ON COLUMN "product"."category_id" IS 'Mã danh mục sản phẩm, khóa ngoại';
COMMENT ON COLUMN "product"."public_id" IS 'Đường dẫn hình ảnh công khai';
COMMENT ON COLUMN "product"."is_deleted" IS 'Trạng thái xóa sản phẩm, mặc định là chưa (false)';

-- Bảng biến thể sản phẩm
CREATE TABLE "product_variant" (
    "variant_id" serial PRIMARY KEY,
    "product_id" integer NOT NULL,
    "color_id" integer,
    "size_id" integer,
    "stock_quantity" integer NOT NULL,
    "is_deleted" boolean NOT NULL DEFAULT false,
    UNIQUE ("product_id", "color_id", "size_id"),
    FOREIGN KEY ("product_id") REFERENCES "product" ("product_id"),
    FOREIGN KEY ("color_id") REFERENCES "product_color" ("color_id"),
    FOREIGN KEY ("size_id") REFERENCES "product_size" ("size_id")
);

COMMENT ON TABLE "product_variant" IS 'Bảng biến thể sản phẩm';
COMMENT ON COLUMN "product_variant"."variant_id" IS 'Mã biến thể sản phẩm, khóa chính, tự động tăng';
COMMENT ON COLUMN "product_variant"."product_id" IS 'Mã sản phẩm, khóa ngoại';
COMMENT ON COLUMN "product_variant"."color_id" IS 'Mã màu sắc, khóa ngoại';
COMMENT ON COLUMN "product_variant"."size_id" IS 'Mã kích thước, khóa ngoại';
COMMENT ON COLUMN "product_variant"."stock_quantity" IS 'Số lượng tồn kho';
COMMENT ON COLUMN "product_variant"."is_deleted" IS 'Trạng thái xóa biến thể sản phẩm, mặc định là chưa (false)';

-- Bảng đơn hàng
CREATE TABLE "order" (
    "order_id" serial PRIMARY KEY,
    "order_date" timestamp NOT NULL DEFAULT now(),
    "customer_id" integer,
    "subtotal" integer NOT NULL, -- Tổng tiền trước giảm giá
    "net_total" integer NOT NULL, -- Tổng tiền sau giảm giá
    "payment_method_id" integer NOT NULL,
    "point_used" integer NOT NULL DEFAULT 0,
    "status" varchar(20) NOT NULL DEFAULT 'Đang xử lý', -- Có 4 trạng thái là: 'Đang xử lý' (dùng khi giao hàng), 'Hoàn thành', 'Đã hủy', 'Đã hoàn trả'
    FOREIGN KEY ("customer_id") REFERENCES "customer" ("customer_id"),
    FOREIGN KEY ("payment_method_id") REFERENCES "payment_method" ("payment_method_id")
);
COMMENT ON TABLE "order" IS 'Bảng đơn hàng';
COMMENT ON COLUMN "order"."order_id" IS 'Mã đơn hàng, khóa chính, tự động tăng';
COMMENT ON COLUMN "order"."order_date" IS 'Ngày đặt hàng, mặc định là thời điểm hiện tại';
COMMENT ON COLUMN "order"."customer_id" IS 'Mã khách hàng, khóa ngoại (có thể null nếu khách hàng không đăng nhập)';
COMMENT ON COLUMN "order"."subtotal" IS 'Tổng tiền đơn hàng trước khi giảm giá';
COMMENT ON COLUMN "order"."net_total" IS 'Tổng tiền đơn hàng sau khi giảm giá';
COMMENT ON COLUMN "order"."payment_method_id" IS 'Mã phương thức thanh toán, khóa ngoại';
COMMENT ON COLUMN "order"."point_used" IS 'Số điểm sử dụng, mặc định là 0';
COMMENT ON COLUMN "order"."status" IS 'Trạng thái đơn hàng, mặc định là "Đang xử lý"';

-- Bảng chi tiết đơn hàng
CREATE TABLE "order_item" (
    "order_item_id" serial PRIMARY KEY,
    "order_id" integer NOT NULL,
    "variant_id" integer NOT NULL,
    "quantity" integer NOT NULL,
    "unit_price" integer NOT NULL,
    FOREIGN KEY ("order_id") REFERENCES "order" ("order_id"),
    FOREIGN KEY ("variant_id") REFERENCES "product_variant" ("variant_id")
);

COMMENT ON TABLE "order_item" IS 'Bảng chi tiết đơn hàng';
COMMENT ON COLUMN "order_item"."order_item_id" IS 'Mã chi tiết đơn hàng, khóa chính, tự động tăng';
COMMENT ON COLUMN "order_item"."order_id" IS 'Mã đơn hàng, khóa ngoại';
COMMENT ON COLUMN "order_item"."variant_id" IS 'Mã biến thể sản phẩm, khóa ngoại';
COMMENT ON COLUMN "order_item"."quantity" IS 'Số lượng sản phẩm';
COMMENT ON COLUMN "order_item"."unit_price" IS 'Đơn giá';

-- Insert sample data into product_category
INSERT INTO "product_category" ("category_name", "product_type") VALUES
('Áo Thun', 'áo'),
('Áo dài tay', 'áo'),
('Áo Polo', 'áo'),
('Quần Jean', 'quần'),
('Quần Jogger', 'quần');

-- Insert sample data into product_color
INSERT INTO "product_color" ("color_name") VALUES
('Đen'),
('Trắng'),
('Xanh navy'),
('Đỏ'),
('Xanh lá');

-- Insert sample data into product_size
INSERT INTO "product_size" ("size_name") VALUES
('S'), ('M'), ('L'), ('XL'),
('28'), ('30'), ('32');

-- Insert sample data into payment_method
INSERT INTO "payment_method" ("payment_method_name") VALUES
('tiền mặt'),
('Momo');

-- Insert sample data into customer
INSERT INTO "customer" ("customer_name", "email", "phone", "address", "create_date", "points") VALUES
('Trần Văn A', 'tran.van.a@gmail.com', '0902345678', '123 Nguyễn Trãi, Quận 5, TP.HCM', '2023-01-01 10:00:00', 100),
('Nguyễn Thị B', 'nguyen.thi.b@gmail.com', '0912345679', '45 Lê Lợi, Quận 1, TP.HCM', '2023-02-01 12:00:00', 200);

-- Insert sample data into store_owner
INSERT INTO "store_owner" ("owner_name", "email", "phone", "address", "username", "password") VALUES
('Nguyễn Văn Chủ', 'chu@example.com', '0901234567', '123 Đường ABC, Quận 1, TP.HCM', 'admin', '123');

-- Insert sample data into product
-- Note: Assuming category_ids are 1, 3, 11 based on the order of insertion into product_category. Adjust if needed.
INSERT INTO "product" ("product_name", "import_price", "price", "category_id", "public_id") VALUES
('Áo Thun', 70000, 159000, 1, '/Assets/AT.220.NAU.1.webp'),
('Quần Jean', 350000, 439000, 4, '/Assets/quan-jeans-nam-dang-straight-sieu-nhe-xanh-wash-1.webp'), -- Assuming 'Quần Jean' is category_id 4
('Áo Polo', 200000, 279000, 3, '/Assets/ao-polo-the-thao-active-premium-thoang-khi-exdry-xam-1.jpg');

-- Insert sample data into product_variant
-- Note: Assuming product_ids are 1, 2, 3 and color/size ids match the insertion order. Adjust if needed.
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity") VALUES
(1, 1, 1, 50),  -- Áo Thun Đen S (product_id=1, color_id=1, size_id=1)
(1, 1, 2, 40),  -- Áo Thun Đen M (product_id=1, color_id=1, size_id=2)
(1, 2, 1, 30),  -- Áo Thun Trắng S (product_id=1, color_id=2, size_id=1)
(1, 2, 2, 20),  -- Áo Thun Trắng M (product_id=1, color_id=2, size_id=2)
(2, 3, 5, 25),  -- Quần Jean Xanh navy 28 (product_id=2, color_id=3, size_id=5)
(2, 3, 6, 35),  -- Quần Jean Xanh navy 30 (product_id=2, color_id=3, size_id=6)
(2, 1, 5, 15),  -- Quần Jean Đen 28 (product_id=2, color_id=1, size_id=5)
(2, 1, 6, 10),  -- Quần Jean Đen 30 (product_id=2, color_id=1, size_id=6)
(3, 4, 3, 20),  -- Áo Polo Đỏ L (product_id=3, color_id=4, size_id=3)
(3, 4, 4, 15),  -- Áo Polo Đỏ XL (product_id=3, color_id=4, size_id=4)
(3, 5, 3, 25),  -- Áo Polo Xanh lá L (product_id=3, color_id=5, size_id=3)
(3, 5, 4, 30);  -- Áo Polo Xanh lá XL (product_id=3, color_id=5, size_id=4)

-- Insert sample data into order (Initial incorrect values, will be updated)
-- Note: Assuming customer_ids are 1, 2 and payment_method_ids are 1, 2. Adjust if needed.
INSERT INTO "order" ("order_date", "customer_id", "subtotal", "net_total", "payment_method_id", "point_used", "status") VALUES
('2024-01-01 10:00:00', 1, 0, 0, 1, 0, 'Đang xử lý'), -- Placeholder totals for order 1
('2024-01-02 11:00:00', 2, 0, 0, 2, 0, 'Đang xử lý'); -- Placeholder totals for order 2

-- Insert sample data into order_item (Initial incorrect prices, will be updated)
-- Note: Assuming order_ids are 1, 2 and variant_ids match the insertion order above. Adjust if needed.
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(1, 1, 2, 0),  -- 2 Áo Thun Đen S (variant_id=1) - Placeholder price
(1, 6, 1, 0),  -- 1 Quần Jean Xanh navy 30 (variant_id=6) - Placeholder price
(2, 9, 1, 0),  -- 1 Áo Polo Đỏ L (variant_id=9) - Placeholder price
(2, 4, 1, 0);  -- 1 Áo Thun Trắng M (variant_id=4) - Placeholder price

-- Update sample data for order 1 and its items
-- Order 1: 2 Áo Thun Đen S (159000 * 2) + 1 Quần Jean Xanh navy 30 (439000 * 1)
-- Subtotal = 318000 + 439000 = 757000
-- Net Total = 757000 (0 points used)
UPDATE "order" SET "subtotal" = 757000, "net_total" = 757000 WHERE "order_id" = 1;
UPDATE "order_item" SET "unit_price" = 159000 WHERE "order_item_id" = 1; -- Áo Thun Đen S
UPDATE "order_item" SET "unit_price" = 439000 WHERE "order_item_id" = 2; -- Quần Jean Xanh navy 30

-- Update sample data for order 2 and its items
-- Order 2: 1 Áo Polo Đỏ L (279000 * 1) + 1 Áo Thun Trắng M (159000 * 1)
-- Subtotal = 279000 + 159000 = 438000
-- Net Total = 438000 (0 points used)
UPDATE "order" SET "subtotal" = 438000, "net_total" = 438000 WHERE "order_id" = 2;
UPDATE "order_item" SET "unit_price" = 279000 WHERE "order_item_id" = 3; -- Áo Polo Đỏ L
UPDATE "order_item" SET "unit_price" = 159000 WHERE "order_item_id" = 4; -- Áo Thun Trắng M

-- Thêm đơn hàng mới (Order 3)
-- Customer 1, 1 Áo Thun Đen M (159000), 1 Quần Jean Đen 28 (439000), Momo, 50 points used, Hoàn thành
-- Subtotal = 159000 + 439000 = 598000
-- Net Total = 598000 - (50 * 1000) = 548000 (Assuming 1 point = 1000đ)
INSERT INTO "order" ("order_date", "customer_id", "subtotal", "net_total", "payment_method_id", "point_used", "status") VALUES
('2024-03-10 14:30:00', 1, 598000, 548000, 2, 50, 'Hoàn thành');

-- Thêm đơn hàng mới (Order 4)
-- Guest, 2 Áo Polo Xanh lá XL (279000 * 2), Tiền mặt, 0 points used, Đã hủy
-- Subtotal = 279000 * 2 = 558000
-- Net Total = 558000
INSERT INTO "order" ("order_date", "customer_id", "subtotal", "net_total", "payment_method_id", "point_used", "status") VALUES
('2024-04-01 09:15:00', NULL, 558000, 558000, 1, 0, 'Đã hủy');

-- Thêm chi tiết cho đơn hàng mới (Order 3 Items)
-- Note: Assuming order_id is 3 and variant_ids are 2, 7. Adjust if needed.
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(3, 2, 1, 159000),  -- 1 Áo Thun Đen M (variant_id = 2)
(3, 7, 1, 439000);  -- 1 Quần Jean Đen 28 (variant_id = 7)

-- Thêm chi tiết cho đơn hàng mới (Order 4 Items)
-- Note: Assuming order_id is 4 and variant_id is 12. Adjust if needed.
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(4, 12, 2, 279000); -- 2 Áo Polo Xanh lá XL (variant_id = 12)