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
    "is_deleted" boolean NOT NULL DEFAULT false
);

COMMENT ON TABLE "product_color" IS 'Bảng màu sắc sản phẩm';
COMMENT ON COLUMN "product_color"."color_id" IS 'Mã màu sắc, khóa chính, tự động tăng';
COMMENT ON COLUMN "product_color"."color_name" IS 'Tên màu sắc, duy nhất';
COMMENT ON COLUMN "product_color"."is_deleted" IS 'Trạng thái xóa màu sắc, mặc định là chưa (false)';

-- Bảng kích thước sản phẩm
CREATE TABLE "product_size" (
    "size_id" serial PRIMARY KEY,
    "size_name" varchar(10) NOT NULL UNIQUE,
    "is_deleted" boolean NOT NULL DEFAULT false
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

-- === Bảng product_category (Quần áo Nam) ===
INSERT INTO "product_category" ("category_name", "product_type") VALUES
('Áo Thun Cổ Tròn', 'Áo'),        -- ID: 1
('Áo Thun Cổ Tim', 'Áo'),         -- ID: 2
('Áo Thun Tay Dài', 'Áo'),        -- ID: 3
('Áo Sơ Mi Công Sở', 'Áo'),      -- ID: 4
('Áo Sơ Mi Caro', 'Áo'),          -- ID: 5
('Áo Sơ Mi Flannel', 'Áo'),       -- ID: 6
('Áo Polo Trơn', 'Áo'),           -- ID: 7
('Áo Polo Họa Tiết', 'Áo'),       -- ID: 8
('Áo Khoác Dù', 'Áo'),            -- ID: 9
('Áo Khoác Bomber', 'Áo'),        -- ID: 10
('Áo Khoác Jean', 'Áo'),          -- ID: 11
('Áo Len Cổ Lọ', 'Áo'),          -- ID: 12
('Quần Jeans Slimfit', 'Quần'),   -- ID: 13
('Quần Jeans Straight', 'Quần'),  -- ID: 14
('Quần Kaki Chinos', 'Quần'),     -- ID: 15
('Quần Kaki Túi Hộp', 'Quần'),    -- ID: 16
('Quần Tây Regular Fit', 'Quần'), -- ID: 17
('Quần Short Kaki Nam', 'Quần');  -- ID: 18
-- ('Quần Jogger Nỉ', 'Quần'),     -- ID: 19 (Nếu cần thêm)
-- ('Quần Jogger Thun', 'Quần');    -- ID: 20 (Nếu cần thêm)

-- === Bảng product_color ===
INSERT INTO "product_color" ("color_name", "is_deleted") VALUES
('Đen', false),                   -- ID: 1
('Trắng', false),                 -- ID: 2
('Xám Đậm', false),               -- ID: 3
('Xám Nhạt', false),              -- ID: 4
('Xanh Navy', false),             -- ID: 5
('Be (Kem)', false),              -- ID: 6
('Nâu Đất', false),               -- ID: 7
('Xanh Rêu', false),              -- ID: 8
('Đỏ Đô', false),      -- ID: 9
('Xanh Biển Nhạt', false),        -- ID: 10
('Xanh Olive', false),            -- ID: 11
('Nâu Bò', false),                -- ID: 12
('Than Chì', false),   -- ID: 13
('Trắng Ngà', false),     -- ID: 14
('Xanh Cổ Vịt', false),    -- ID: 15
('Xám Tro', false),    -- ID: 16
('Đen Nhám', false),-- ID: 17
('Vàng Cát', false),       -- ID: 18
('Bạc', true);                    -- ID: 19 (Đã xóa)

-- === Bảng product_size (Size Nam) ===
INSERT INTO "product_size" ("size_name", "is_deleted") VALUES
('S', false),                     -- ID: 1
('M', false),                     -- ID: 2
('L', false),                     -- ID: 3
('XL', false),                    -- ID: 4
('XXL', false),                   -- ID: 5
('Freesize', false),              -- ID: 6
('W28', false),                   -- ID: 7 (Quần)
('W29', false),                   -- ID: 8
('W30', false),                   -- ID: 9
('W31', false),                   -- ID: 10
('W32', false),                   -- ID: 11
('W33', false),                   -- ID: 12
('W34', false),                   -- ID: 13
('W36', false),                   -- ID: 14
('W28/L30', false),               -- ID: 15
('W30/L30', false),               -- ID: 16
('W32/L32', false),               -- ID: 17
('W34/L32', false),               -- ID: 18
('XS', true);                     -- ID: 19 (Đã xóa)

-- === Bảng payment_method ===
INSERT INTO "payment_method" ("payment_method_name") VALUES
('Tiền mặt'),                    -- ID: 1
('Chuyển khoản');                -- ID: 2

---------------------------------
-- === BẢNG PHỤ THUỘC (1) === --
---------------------------------

-- === Bảng customer ===
INSERT INTO "customer" ("customer_name", "email", "phone", "address", "create_date", "points", "is_deleted") VALUES
('Trần Minh Quân', 'quan.tran@email.com', '0911111111', '1 Paster, Quận 1, TP.HCM', NOW() - interval '25 day', 22, false),
('Lê Hoàng Phúc', 'phuc.le@mail.net', '0922222222', '2 Hai Bà Trưng, Hoàn Kiếm, Hà Nội', NOW() - interval '55 day', 8, false),
('Phạm Gia Huy', NULL, '0933333333', '3 Trần Phú, Hải Châu, Đà Nẵng', NOW() - interval '8 day', 35, false),
('Vũ Đức Anh', 'anh.vu@domain.org', '0944444444', '4 Lê Lợi, Quận 3, TP.HCM', NOW() - interval '80 day', 2, false),
('Nguyễn Bảo Long', 'long.nguyen@sample.co', '0955555555', '5 Quang Trung, Hà Đông, Hà Nội', NOW() - interval '12 day', 48, false),
('Hoàng Minh Đức', NULL, '0966666666', '6 Bạch Đằng, Sơn Trà, Đà Nẵng', NOW() - interval '110 day', 11, false),
('Đặng Khôi Nguyên', 'nguyen.dk@provider.vn', '0977777777', '7 Cách Mạng Tháng 8, Quận 10, TP.HCM', NOW() - interval '18 day', 19, false),
('Bùi Tuấn Kiệt', 'kiet.bui@email.com', '0988888888', '8 Lý Thường Kiệt, Cầu Giấy, Hà Nội', NOW() - interval '150 day', 5, true), -- Khách đã xóa
('Doãn Chí Bình', 'binh.doan@mail.org', '0999999999', '9 Nguyễn Văn Linh, Thanh Khê, Đà Nẵng', NOW() - interval '22 day', 28, false),
('Triệu Nhật Minh', NULL, '0912121212', '10 Nguyễn Huệ, Quận 1, TP.HCM', NOW() - interval '210 day', 3, false),
('Đinh Tiến Dũng', 'dung.dinh@tech.co', '0923232323', '11 Trần Hưng Đạo, Hoàn Kiếm, Hà Nội', NOW() - interval '40 day', 16, false),
('Tạ Quang Huy', 'huy.ta@service.net', '0934343434', '12 Võ Nguyên Giáp, Ngũ Hành Sơn, Đà Nẵng', NOW() - interval '9 day', 21, false),
('Lương Mạnh Hải', NULL, '0945454545', '13 Nam Kỳ Khởi Nghĩa, Quận 3, TP.HCM', NOW() - interval '280 day', 7, false),
('Phan Anh Tuấn', 'tuan.phan@web.com', '0956565656', '14 Phố Huế, Hai Bà Trưng, Hà Nội', NOW() - interval '4 day', 42, false),
('Hà Huy Tập', 'tap.ha@company.vn', '0967676767', '15 Lê Duẩn, Hải Châu, Đà Nẵng', NOW() - interval '310 day', 10, false),
('Cao Bá Quát', 'quat.cao@history.edu', '0978787878', '16 Đồng Khởi, Quận 1, TP.HCM', NOW() - interval '2 day', 95, false),
('Lý Thường Kiệt', NULL, '0989898989', '17 Hoàng Diệu, Ba Đình, Hà Nội', NOW() - interval '400 day', 4, false),
('Quang Trung', 'trung.quang@king.vn', '0901020304', '18 Tây Sơn, Đống Đa, Hà Nội', NOW() - interval '60 day', 1, false);

-- === Bảng store_owner ===
INSERT INTO "store_owner" ("owner_name", "email", "phone", "address", "username", "password", "entropy") VALUES
('Chủ Shop CoolWear', 'admin@coolwear.com', '0901234567', '123 Đường Thời Trang, Q. Fashion, TP. Style', 'admin', '123', ''); -- Pass: 123

---------------------------------
-- === BẢNG PHỤ THUỘC (2) === --
---------------------------------

-- === Bảng product ===
-- Giả định category_id từ 1 đến 18
INSERT INTO "product" ("product_name", "import_price", "price", "category_id", "public_id", "is_deleted") VALUES
('Áo Thun Nam Basic Cổ Tròn Cotton', 75000, 149000, 1, '/Assets/1.webp', false),         -- ID: 1
('Áo Thun Nam Cổ Tim Slimfit', 85000, 169000, 2, '/Assets/2.webp', false),             -- ID: 2
('Áo Sơ Mi Nam Oxford Trắng Công Sở', 190000, 379000, 4, '/Assets/3.webp', false), -- ID: 3
('Quần Jeans Nam Slimfit Xanh Đen', 260000, 499000, 13, '/Assets/13.webp', false),  -- ID: 4
('Áo Polo Nam Pique Basic', 150000, 299000, 7, '/Assets/4.webp', false),                -- ID: 5
('Quần Kaki Nam Chinos Co Giãn', 210000, 399000, 15, '/Assets/14.webp', false),           -- ID: 6
('Áo Khoác Dù Nam 2 Lớp Chống Nước', 280000, 549000, 9, '/Assets/5.webp', false),          -- ID: 7
('Quần Tây Nam Regular Fit Xám', 270000, 489000, 17, '/Assets/16.webp', false),           -- ID: 8
('Áo Sơ Mi Nam Caro Nhỏ Dài Tay', 200000, 389000, 5, '/Assets/6.webp', false),            -- ID: 9
('Quần Short Kaki Nam Basic', 130000, 259000, 18, '/Assets/17.webp', false),            -- ID: 10
('Áo Khoác Bomber Nam Kaki Lót Gió', 320000, 599000, 10, '/Assets/7.webp', false), -- ID: 11
('Quần Jeans Nam Straight Wash Nhẹ', 270000, 519000, 14, '/Assets/18.webp', false),   -- ID: 12
('Áo Thun Nam Tay Dài Raglan', 110000, 219000, 3, '/Assets/8.webp', false),          -- ID: 13
('Áo Len Nam Cổ Lọ Merino', 240000, 459000, 12, '/Assets/9.webp', false),             -- ID: 14
('Sơ Mi Flannel Kẻ Ô Nam', 220000, 419000, 6, '/Assets/10.webp', true),            -- ID: 15 (Đã xóa)
('Áo Polo Nam Họa Tiết Lá', 170000, 329000, 8, '/Assets/11.webp', false),             -- ID: 16
('Quần Kaki Nam Túi Hộp Cargo', 240000, 469000, 16, '/Assets/19.webp', false),         -- ID: 17
('Áo Khoác Jean Nam Denim Bụi', 300000, 579000, 11, '/Assets/12.webp', false);          -- ID: 18

---------------------------------
-- === BẢNG PHỤ THUỘC (3) === --
---------------------------------

-- === Bảng product_variant ===
-- Giả định product_id: 1-14, 16-18 (15 đã xóa)
-- Giả định color_id: 1-16, 18 (17, 19 đã xóa)
-- Giả định size_id: 1-16, 18 (17, 19 đã xóa)

-- Product 1: Áo Thun Nam Basic Cổ Tròn Cotton (ID: 1)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(1, 1, 1, 100, false), -- Đen, S
(1, 1, 2, 150, false), -- Đen, M
(1, 2, 2, 120, false), -- Trắng, M
(1, 4, 3, 80, false);  -- Xám Nhạt, L

-- Product 2: Áo Thun Nam Cổ Tim Slimfit (ID: 2)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(2, 3, 1, 50, false),  -- Xám Đậm, S
(2, 3, 2, 70, false),  -- Xám Đậm, M
(2, 5, 2, 60, false);  -- Xanh Navy, M

-- Product 3: Áo Sơ Mi Nam Oxford Trắng Công Sở (ID: 3)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(3, 2, 2, 80, false),  -- Trắng, M
(3, 2, 3, 75, false),  -- Trắng, L
(3, 10, 3, 50, false); -- Xanh Biển Nhạt, L

-- Product 4: Quần Jeans Nam Slimfit Xanh Đen (ID: 4)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(4, 5, 8, 60, false),  -- Xanh Navy (giả lập xanh đen), W29
(4, 5, 9, 90, false),  -- Xanh Navy, W30
(4, 5, 10, 70, false), -- Xanh Navy, W31
(4, 1, 9, 40, false);  -- Đen, W30 (Thêm variant)

-- Product 5: Áo Polo Nam Pique Basic (ID: 5)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(5, 1, 2, 110, false), -- Đen, M
(5, 2, 2, 130, false), -- Trắng, M
(5, 5, 3, 90, false);  -- Xanh Navy, L

-- Product 6: Quần Kaki Nam Chinos Co Giãn (ID: 6)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(6, 6, 9, 70, false),  -- Be, W30
(6, 6, 10, 80, false), -- Be, W31
(6, 7, 9, 65, false);  -- Nâu Đất, W30

-- Product 7: Áo Khoác Dù Nam 2 Lớp Chống Nước (ID: 7)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(7, 1, 3, 40, false),  -- Đen, L
(7, 1, 4, 35, false),  -- Đen, XL
(7, 8, 3, 30, false);  -- Xanh Rêu, L

-- Product 8: Quần Tây Nam Regular Fit Xám (ID: 8)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(8, 3, 10, 55, false), -- Xám Đậm, W31
(8, 3, 11, 60, false), -- Xám Đậm, W32
(8, 13, 11, 45, false);-- Than Chì, W32

-- Product 9: Áo Sơ Mi Nam Caro Nhỏ Dài Tay (ID: 9)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(9, 5, 2, 40, false),  -- Xanh Navy caro, M
(9, 9, 2, 35, false),  -- Đỏ Đô caro, M
(9, 5, 3, 30, false);  -- Xanh Navy caro, L

-- Product 10: Quần Short Kaki Nam Basic (ID: 10)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(10, 6, 8, 90, false), -- Be, W29
(10, 6, 9, 100, false),-- Be, W30
(10, 5, 9, 80, false); -- Xanh Navy, W30

-- Product 11: Áo Khoác Bomber Nam Kaki Lót Gió (ID: 11)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(11, 6, 3, 25, false), -- Be, L
(11, 11, 3, 20, false),-- Xanh Olive, L
(11, 11, 4, 15, false);-- Xanh Olive, XL

-- Product 12: Quần Jeans Nam Straight Wash Nhẹ (ID: 12)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(12, 10, 9, 50, false), -- Xanh Biển Nhạt, W30
(12, 10, 10, 60, false),-- Xanh Biển Nhạt, W31
(12, 10, 11, 40, false);-- Xanh Biển Nhạt, W32

-- Product 13: Áo Thun Nam Tay Dài Raglan (ID: 13)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(13, 1, 2, 70, false),  -- Thân đen/Tay trắng, M
(13, 4, 2, 60, false),  -- Thân xám/Tay đen, M
(13, 5, 3, 50, false);  -- Thân navy/Tay trắng, L

-- Product 14: Áo Len Nam Cổ Lọ Merino (ID: 14)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(14, 1, 2, 30, false),  -- Đen, M
(14, 7, 2, 25, false),  -- Nâu Đất, M
(14, 13, 3, 20, false); -- Than Chì, L

-- Product 16: Áo Polo Nam Họa Tiết Lá (ID: 16)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(16, 2, 1, 45, false),  -- Nền trắng, S
(16, 2, 2, 55, false);  -- Nền trắng, M

-- Product 17: Quần Kaki Nam Túi Hộp Cargo (ID: 17)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(17, 8, 9, 40, false),  -- Xanh Rêu, W30
(17, 8, 10, 50, false), -- Xanh Rêu, W31
(17, 1, 10, 30, false); -- Đen, W31

-- Product 18: Áo Khoác Jean Nam Denim Bụi (ID: 18)
INSERT INTO "product_variant" ("product_id", "color_id", "size_id", "stock_quantity", "is_deleted") VALUES
(18, 10, 2, 20, false), -- Xanh wash, M
(18, 10, 3, 25, false), -- Xanh wash, L
(18, 1, 3, 15, false);  -- Đen wash, L

---------------------------------
-- === BẢNG PHỤ THUỘC (4) === --
---------------------------------

-- === Bảng order ===
-- Giả định customer_id: 1-7, 9-18 (8 đã xóa)
-- Giả định payment_method_id: 1, 2
INSERT INTO "order" ("order_date", "customer_id", "subtotal", "net_total", "payment_method_id", "point_used", "status") VALUES
(NOW() - interval '1 day', 1, 528000, 528000, 1, 0, 'Hoàn thành'),             -- Order ID: 1
(NOW() - interval '2 day', 3, 998000, 993000, 2, 5, 'Hoàn thành'),              -- Order ID: 2 (Dùng 5 điểm = 5k)
(NOW() - interval '3 day', NULL, 618000, 618000, 1, 0, 'Đang xử lý'),           -- Order ID: 3
(NOW() - interval '4 day', 5, 299000, 299000, 1, 0, 'Hoàn thành'),             -- Order ID: 4
(NOW() - interval '5 day', 7, 1098000, 1098000, 2, 0, 'Hoàn thành'),            -- Order ID: 5
(NOW() - interval '6 day', 9, 878000, 868000, 1, 10, 'Hoàn thành'),             -- Order ID: 6 (Dùng 10 điểm = 10k)
(NOW() - interval '7 day', 11, 0, 0, 1, 0, 'Đã hủy'),                       -- Order ID: 7 (Subtotal = 0)
(NOW() - interval '8 day', 2, 428000, 428000, 2, 0, 'Hoàn thành'),             -- Order ID: 8
(NOW() - interval '9 day', 13, 599000, 599000, 1, 0, 'Hoàn thành'),            -- Order ID: 9
(NOW() - interval '10 day', NULL, 0, 0, 1, 0, 'Đã hoàn trả'),                -- Order ID: 10 (Subtotal = 0)
(NOW() - interval '11 day', 4, 519000, 519000, 2, 0, 'Hoàn thành'),            -- Order ID: 11
(NOW() - interval '12 day', 15, 438000, 438000, 1, 0, 'Hoàn thành'),           -- Order ID: 12
(NOW() - interval '13 day', 6, 459000, 459000, 1, 0, 'Đang xử lý'),           -- Order ID: 13
(NOW() - interval '14 day', 17, 658000, 638000, 2, 20, 'Hoàn thành'),           -- Order ID: 14 (Dùng 20 điểm = 20k)
(NOW() - interval '15 day', 10, 469000, 469000, 1, 0, 'Hoàn thành'),           -- Order ID: 15
(NOW() - interval '16 day', 12, 579000, 579000, 2, 0, 'Hoàn thành'),           -- Order ID: 16
(NOW() - interval '17 day', 14, 448000, 448000, 1, 0, 'Đang xử lý'),           -- Order ID: 17
(NOW() - interval '18 day', 16, 489000, 489000, 2, 0, 'Hoàn thành'),           -- Order ID: 18

---------------------------------
-- === BẢNG PHỤ THUỘC (5) === --
---------------------------------

-- === Bảng order_item ===
-- Chỉ thêm item cho các đơn hàng có status 'Hoàn thành' hoặc 'Đang xử lý'
-- Giả định variant_id được tạo tuần tự
-- Giá (unit_price) lấy từ bảng product tương ứng

-- Order 1 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(1, 1, 1, 149000),  -- Áo thun đen S
(1, 7, 1, 379000);  -- Sơ mi trắng L

-- Order 2 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(2, 10, 2, 499000); -- Jean xanh navy W30 (Mua 2 cái)

-- Order 3 (Đang xử lý)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(3, 13, 1, 280000), -- Chân váy Be M (Ví dụ, dù là đồ nam, để test)
(3, 16, 1, 399000); -- Kaki Chinos Be W31

-- Order 4 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(4, 17, 1, 299000); -- Polo Pique đen M

-- Order 5 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(5, 21, 1, 549000), -- Khoác dù đen L
(5, 22, 1, 549000); -- Khoác dù đen XL

-- Order 6 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(6, 26, 1, 489000), -- Quần tây xám W32
(6, 29, 1, 389000); -- Sơ mi caro navy M

-- Order 8 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(8, 5, 1, 169000),  -- Áo thun cổ tim xám đậm S
(8, 30, 1, 259000); -- Short kaki Be W29

-- Order 9 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(9, 34, 1, 599000); -- Khoác bomber Be L

-- Order 11 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(11, 38, 1, 519000);-- Jean straight wash W31

-- Order 12 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(12, 40, 1, 219000),-- Thun raglan thân xám M
(12, 41, 1, 219000);-- Thun raglan thân navy L

-- Order 13 (Đang xử lý)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(13, 43, 1, 459000);-- Len cổ lọ nâu đất M

-- Order 14 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(14, 46, 2, 329000); -- Polo họa tiết lá trắng M (Mua 2)

-- Order 15 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(15, 48, 1, 469000); -- Kaki túi hộp xanh rêu W31

-- Order 16 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(16, 51, 1, 579000); -- Khoác jean đen wash L

-- Order 17 (Đang xử lý)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(17, 2, 1, 149000),  -- Áo thun đen M
(17, 18, 1, 299000); -- Polo Pique trắng M

-- Order 18 (Hoàn thành)
INSERT INTO "order_item" ("order_id", "variant_id", "quantity", "unit_price") VALUES
(18, 24, 1, 489000); -- Quần tây than chì W32