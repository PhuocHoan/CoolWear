CREATE TABLE "product_category" (
  "category_id" integer PRIMARY KEY,
  "category_name" varchar not null unique,
  "product_type" varchar not null
);

CREATE TABLE "product" (
  "product_id" serial PRIMARY KEY,
  "product_name" varchar not null unique,
  "import_price" integer not null,
  "price" integer not null,
  "stock_quantity" integer not null,
  "category_id" integer not null,
  "public_id" varchar not null
);

CREATE TABLE "product_color" (
  "color_id" serial PRIMARY KEY,
  "color_name" varchar not null unique
);

CREATE TABLE "product_color_link" (
  "product_color_id" serial PRIMARY KEY,
  "product_id" integer not null,
  "color_id" integer not null
);

CREATE TABLE "product_size" (
  "size_id" serial PRIMARY KEY,
  "size_name" varchar not null unique
);

CREATE TABLE "product_size_link" (
  "product_size_id" serial PRIMARY KEY,
  "product_id" integer not null,
  "size_id" integer not null
);

CREATE TABLE "customer" (
  "customer_id" serial PRIMARY KEY,
  "customer_name" varchar not null,
  "email" varchar,
  "phone" varchar,
  "address" varchar,
  "create_date" timestamp not null DEFAULT now(),
  "points" integer not null DEFAULT 0
);

CREATE TABLE "store_owner" (
  "owner_id" serial PRIMARY KEY,
  "owner_name" varchar not null,
  "email" varchar not null,
  "phone" varchar not null,
  "address" varchar not null,
  "password" varchar not null
);

CREATE TABLE "payment_method" (
  "payment_method_id" serial PRIMARY KEY,
  "payment_method_name" varchar not null unique
);

CREATE TABLE "order" (
  "order_id" serial PRIMARY KEY,
  "order_date" timestamp not null DEFAULT now(),
  "customer_id" integer not null,
  "total_amount" integer not null,
  "point_used" integer not null DEFAULT 0,
  "payment_method_id" integer not null,
  "is_refunded" bool not null DEFAULT FALSE
);

CREATE TABLE "order_item" (
  "order_item_id" serial PRIMARY KEY,
  "order_id" integer not null,
  "product_id" integer not null,
  "quantity" integer not null,
  "unit_price" integer not null
);

ALTER TABLE "product" ADD FOREIGN KEY ("category_id") REFERENCES "product_category" ("category_id");

ALTER TABLE "product_color_link" ADD FOREIGN KEY ("product_id") REFERENCES "product" ("product_id");

ALTER TABLE "product_color_link" ADD FOREIGN KEY ("color_id") REFERENCES "product_color" ("color_id");

ALTER TABLE "product_size_link" ADD FOREIGN KEY ("product_id") REFERENCES "product" ("product_id");

ALTER TABLE "product_size_link" ADD FOREIGN KEY ("size_id") REFERENCES "product_size" ("size_id");

ALTER TABLE "order" ADD FOREIGN KEY ("customer_id") REFERENCES "customer" ("customer_id");

ALTER TABLE "order_item" ADD FOREIGN KEY ("order_id") REFERENCES "order" ("order_id");

ALTER TABLE "order_item" ADD FOREIGN KEY ("product_id") REFERENCES "product" ("product_id");

ALTER TABLE "order" ADD FOREIGN KEY ("payment_method_id") REFERENCES "payment_method" ("payment_method_id");

-- Bảng 
COMMENT ON TABLE product_category IS 'Bảng lưu trữ thông tin các danh mục sản phẩm';
COMMENT ON COLUMN product_category.category_id IS 'ID duy nhất của danh mục (khóa chính)';
COMMENT ON COLUMN product_category.category_name IS 'Tên danh mục sản phẩm';
COMMENT ON COLUMN product_category.product_type IS 'Loại sản phẩm (áo, quần) thuộc danh mục';

-- Bảng sản phẩm
COMMENT ON TABLE "product" IS 'Bảng lưu trữ thông tin sản phẩm';
COMMENT ON COLUMN "product"."product_id" IS 'Mã sản phẩm (khóa chính)';
COMMENT ON COLUMN "product"."product_name" IS 'Tên sản phẩm';
COMMENT ON COLUMN "product"."import_price" IS 'Giá nhập';
COMMENT ON COLUMN "product"."price" IS 'Giá bán';
COMMENT ON COLUMN "product"."stock_quantity" IS 'Số lượng tồn kho';
COMMENT ON COLUMN "product"."category_id" IS 'Mã danh mục (khóa ngoại)';
COMMENT ON COLUMN "product"."public_id" IS 'Đường dẫn tới ảnh sản phẩm';

-- Bảng màu sắc
COMMENT ON TABLE "product_color" IS 'Bảng lưu trữ thông tin màu sắc sản phẩm';
COMMENT ON COLUMN "product_color"."color_id" IS 'Mã màu (khóa chính)';
COMMENT ON COLUMN "product_color"."color_name" IS 'Tên màu';

-- Bảng liên kết sản phẩm và màu sắc
COMMENT ON TABLE "product_color_link" IS 'Bảng lưu trữ liên kết giữa sản phẩm và màu sắc';
COMMENT ON COLUMN "product_color_link"."product_color_id" IS 'Mã liên kết (khóa chính)';
COMMENT ON COLUMN "product_color_link"."product_id" IS 'Mã sản phẩm (khóa ngoại)';
COMMENT ON COLUMN "product_color_link"."color_id" IS 'Mã màu (khóa ngoại)';

-- Bảng kích thước
COMMENT ON TABLE "product_size" IS 'Bảng lưu trữ thông tin kích thước sản phẩm';
COMMENT ON COLUMN "product_size"."size_id" IS 'Mã kích thước (khóa chính)';
COMMENT ON COLUMN "product_size"."size_name" IS 'Tên kích thước';

-- Bảng liên kết sản phẩm và kích thước
COMMENT ON TABLE "product_size_link" IS 'Bảng lưu trữ liên kết giữa sản phẩm và kích thước';
COMMENT ON COLUMN "product_size_link"."product_size_id" IS 'Mã liên kết (khóa chính)';
COMMENT ON COLUMN "product_size_link"."product_id" IS 'Mã sản phẩm (khóa ngoại)';
COMMENT ON COLUMN "product_size_link"."size_id" IS 'Mã kích thước (khóa ngoại)';

-- Bảng khách hàng
COMMENT ON TABLE "customer" IS 'Bảng lưu trữ thông tin khách hàng';
COMMENT ON COLUMN "customer"."customer_id" IS 'Mã khách hàng (khóa chính)';
COMMENT ON COLUMN "customer"."customer_name" IS 'Tên khách hàng';
COMMENT ON COLUMN "customer"."email" IS 'Email khách hàng';
COMMENT ON COLUMN "customer"."phone" IS 'Số điện thoại khách hàng';
COMMENT ON COLUMN "customer"."address" IS 'Địa chỉ khách hàng';
COMMENT ON COLUMN "customer"."create_date" IS 'Ngày tạo tài khoản';
COMMENT ON COLUMN "customer"."points" IS 'Điểm tích lũy';

-- Bảng chủ cửa hàng
COMMENT ON TABLE "store_owner" IS 'Bảng lưu trữ thông tin chủ cửa hàng';
COMMENT ON COLUMN "store_owner"."owner_id" IS 'Mã chủ cửa hàng (khóa chính)';
COMMENT ON COLUMN "store_owner"."owner_name" IS 'Tên chủ cửa hàng';
COMMENT ON COLUMN "store_owner"."email" IS 'Email chủ cửa hàng';
COMMENT ON COLUMN "store_owner"."phone" IS 'Số điện thoại chủ cửa hàng';
COMMENT ON COLUMN "store_owner"."address" IS 'Địa chỉ chủ cửa hàng';
COMMENT ON COLUMN "store_owner"."password" IS 'Mật khẩu đã mã hóa';

-- Bảng phương thức thanh toán
COMMENT ON TABLE "payment_method" IS 'Bảng lưu trữ thông tin phương thức thanh toán';
COMMENT ON COLUMN "payment_method"."payment_method_id" IS 'Mã phương thức thanh toán (khóa chính)';
COMMENT ON COLUMN "payment_method"."payment_method_name" IS 'Tên phương thức thanh toán';

-- Bảng đơn hàng
COMMENT ON TABLE "order" IS 'Bảng lưu trữ thông tin đơn hàng';
COMMENT ON COLUMN "order"."order_id" IS 'Mã đơn hàng (khóa chính)';
COMMENT ON COLUMN "order"."order_date" IS 'Ngày đặt hàng';
COMMENT ON COLUMN "order"."customer_id" IS 'Mã khách hàng (khóa ngoại)';
COMMENT ON COLUMN "order"."total_amount" IS 'Tổng số tiền';
COMMENT ON COLUMN "order"."payment_method_id" IS 'Mã phương thức thanh toán (khóa ngoại)';
COMMENT ON COLUMN "order"."is_refunded" IS 'Trạng thái hoàn tiền';
COMMENT ON COLUMN "order"."point_used" IS 'Số điểm sử dụng để thanh toán (Tích điểm sẽ tính trước khi dùng điểm thưởng để thanh toán)';

-- Bảng chi tiết đơn hàng
COMMENT ON TABLE "order_item" IS 'Bảng lưu trữ thông tin chi tiết đơn hàng';
COMMENT ON COLUMN "order_item"."order_item_id" IS 'Mã chi tiết đơn hàng (khóa chính)';
COMMENT ON COLUMN "order_item"."order_id" IS 'Mã đơn hàng (khóa ngoại)';
COMMENT ON COLUMN "order_item"."product_id" IS 'Mã sản phẩm (khóa ngoại)';
COMMENT ON COLUMN "order_item"."quantity" IS 'Số lượng';
COMMENT ON COLUMN "order_item"."unit_price" IS 'Đơn giá';

-- Dữ liệu mẫu
INSERT INTO product_category (category_id, category_name, product_type) VALUES
(1, 'Áo Thun', 'áo'),
(2, 'Áo dài tay', 'áo'),
(3, 'Áo Polo', 'áo'),
(4, 'Áo khoác', 'áo'),
(5, 'Áo Sơ Mi', 'áo'),
(6, 'Áo Tanktop', 'áo'),
(7, 'Áo thể thao', 'áo'),
(8, 'Áo Vest', 'áo'),
(9, 'Áo Len', 'áo'),
(10, 'Áo Hoodie', 'áo'),
(11, 'Quần Jean', 'quần'),
(12, 'Quần Jogger', 'quần'),
(13, 'Quần Kaki', 'quần'),
(14, 'Quần Pants', 'quần'),
(15, 'Quần Shorts', 'quần'),
(16, 'Quần Lót', 'quần'),
(17, 'Quần Đùi', 'quần'),
(18, 'Quần bơi', 'quần'),
(19, 'Quần Legging', 'quần'),
(20, 'Quần Culottes', 'quần');

INSERT INTO product_color (color_name) VALUES
('Phối màu'),
('Đen'),
('Xám'),
('Trắng'),
('Be'),
('Xanh lam'),
('Xanh lá'),
('Xanh ngọc'),
('Đỏ'),
('Cam'),
('Vàng'),
('Tím'),
('Nâu'),
('Hồng'),
('Xanh sáng'),
('Xanh đậm'),
('Đen xám'),
('Xanh navy'),
('Đỏ đô'),
('Vàng chanh');

INSERT INTO product_size (size_name) VALUES
('40'),
('42');

INSERT INTO payment_method (payment_method_name) VALUES
('tiền mặt'),
('Momo');

INSERT INTO customer (customer_name, email, phone, address, create_date, points) VALUES
('Trần Văn A', 'tran.van.a@gmail.com', '0902345678', '123 Nguyễn Trãi, Quận 5, TP.HCM', '2023-01-01 10:00:00', 100),
('Nguyễn Thị B', 'nguyen.thi.b@gmail.com', '0912345679', '45 Lê Lợi, Quận 1, TP.HCM', '2023-02-01 12:00:00', 200),
('Lê Văn C', 'le.van.c@gmail.com', '0923456780', '78 Trần Phú, Quận 7, TP.HCM', '2023-03-01 14:00:00', 150),
('Phạm Thị D', 'pham.thi.d@gmail.com', '0934567891', '12 Nguyễn Huệ, Quận 3, TP.HCM', '2023-04-01 16:00:00', 300),
('Hoàng Văn E', 'hoang.van.e@gmail.com', '0945678902', '56 Lý Thường Kiệt, Quận 10, TP.HCM', '2023-05-01 18:00:00', 50),
('Vũ Thị F', 'vu.thi.f@gmail.com', '0956789013', '89 Phạm Văn Đồng, Gò Vấp, TP.HCM', '2023-06-01 20:00:00', 250),
('Đặng Văn G', 'dang.van.g@gmail.com', '0967890124', '34 Hùng Vương, Bình Thạnh, TP.HCM', '2023-07-01 22:00:00', 120),
('Bùi Thị H', 'bui.thi.h@gmail.com', '0978901235', '67 Nguyễn Văn Cừ, Thủ Đức, TP.HCM', '2023-08-01 09:00:00', 180),
('Đỗ Văn I', 'do.van.i@gmail.com', '0989012346', '90 Lê Đại Hành, Quận 11, TP.HCM', '2023-09-01 11:00:00', 90),
('Ngô Thị K', 'ngo.thi.k@gmail.com', '0990123457', '23 Nguyễn Thị Minh Khai, Quận 1, TP.HCM', '2023-10-01 13:00:00', 220),
('Dương Văn L', 'duong.van.l@gmail.com', '0901234569', '45 Trần Hưng Đạo, Quận 5, TP.HCM', '2023-11-01 15:00:00', 130),
('Lý Thị M', 'ly.thi.m@gmail.com', '0912345680', '78 Nguyễn Đình Chiểu, Quận 3, TP.HCM', '2023-12-01 17:00:00', 170),
('Hồ Văn N', 'ho.van.n@gmail.com', '0923456781', '12 Lý Tự Trọng, Quận 1, TP.HCM', '2024-01-01 19:00:00', 110),
('Nguyễn Thị O', 'nguyen.thi.o@gmail.com', '0934567892', '56 Nguyễn Công Trứ, Quận 7, TP.HCM', '2024-02-01 21:00:00', 240),
('Trần Văn P', 'tran.van.p@gmail.com', '0945678903', '89 Điện Biên Phủ, Bình Thạnh, TP.HCM', '2024-03-01 23:00:00', 80),
('Lê Thị Q', 'le.thi.q@gmail.com', '0956789014', '34 Lê Hồng Phong, Quận 5, TP.HCM', '2024-04-01 08:00:00', 190),
('Phạm Văn R', 'pham.van.r@gmail.com', '0967890125', '67 Nguyễn Văn Linh, Quận 7, TP.HCM', '2024-05-01 10:00:00', 140),
('Hoàng Thị S', 'hoang.thi.s@gmail.com', '0978901236', '90 Trần Quốc Toản, Gò Vấp, TP.HCM', '2024-06-01 12:00:00', 160),
('Vũ Văn T', 'vu.van.t@gmail.com', '0989012347', '23 Nguyễn Khánh Toàn, Thủ Đức, TP.HCM', '2024-07-01 14:00:00', 210),
('Đặng Thị U', 'dang.thi.u@gmail.com', '0990123458', '45 Nguyễn Trãi, Quận 5, TP.HCM', '2024-08-01 16:00:00', 230);

INSERT INTO product (product_name, import_price, price, stock_quantity, category_id, public_id) VALUES
('Áo Thun Đen', 50000, 100000, 50, 1, 'img/product1.jpg'),
('Áo dài tay Xám', 60000, 120000, 40, 2, 'img/product2.jpg'),
('Áo Polo Trắng', 70000, 140000, 30, 3, 'img/product3.jpg'),
('Áo khoác Xanh lam', 100000, 200000, 20, 4, 'img/product4.jpg'),
('Áo Sơ Mi Be', 80000, 160000, 35, 5, 'img/product5.jpg'),
('Áo Tanktop Đỏ', 40000, 80000, 60, 6, 'img/product6.jpg'),
('Áo thể thao Xanh lá', 60000, 120000, 45, 7, 'img/product7.jpg'),
('Áo Vest Nâu', 120000, 240000, 15, 8, 'img/product8.jpg'),
('Áo Len Hồng', 90000, 180000, 25, 9, 'img/product9.jpg'),
('Áo Hoodie Đen xám', 110000, 220000, 20, 10, 'img/product10.jpg'),
('Quần Jean Xanh navy', 100000, 200000, 30, 11, 'img/product11.jpg'),
('Quần Jogger Đỏ đô', 80000, 160000, 40, 12, 'img/product12.jpg'),
('Quần Kaki Be', 70000, 140000, 35, 13, 'img/product13.jpg'),
('Quần Pants Đen', 90000, 180000, 25, 14, 'img/product14.jpg'),
('Quần Shorts Xám', 50000, 100000, 50, 15, 'img/product15.jpg'),
('Quần Lót Trắng', 20000, 40000, 100, 16, 'img/product16.jpg'),
('Quần Đùi Phối màu', 30000, 60000, 80, 17, 'img/product17.jpg'),
('Quần bơi Xanh ngọc', 40000, 80000, 60, 18, 'img/product18.jpg'),
('Quần Legging Đen', 60000, 120000, 40, 19, 'img/product19.jpg'),
('Quần Culottes Vàng chanh', 70000, 140000, 30, 20, 'img/product20.jpg');

INSERT INTO product_color_link (product_id, color_id) VALUES
(1, 2),  -- Áo Thun Đen -> Đen
(2, 3),  -- Áo dài tay Xám -> Xám
(3, 4),  -- Áo Polo Trắng -> Trắng
(4, 6),  -- Áo khoác Xanh lam -> Xanh lam
(5, 5),  -- Áo Sơ Mi Be -> Be
(6, 9),  -- Áo Tanktop Đỏ -> Đỏ
(7, 7),  -- Áo thể thao Xanh lá -> Xanh lá
(8, 13), -- Áo Vest Nâu -> Nâu
(9, 14), -- Áo Len Hồng -> Hồng
(10, 17),-- Áo Hoodie Đen xám -> Đen xám
(11, 18),-- Quần Jean Xanh navy -> Xanh navy
(12, 19),-- Quần Jogger Đỏ đô -> Đỏ đô
(13, 5), -- Quần Kaki Be -> Be
(14, 2), -- Quần Pants Đen -> Đen
(15, 3), -- Quần Shorts Xám -> Xám
(16, 4), -- Quần Lót Trắng -> Trắng
(17, 1), -- Quần Đùi Phối màu -> Phối màu
(18, 8), -- Quần bơi Xanh ngọc -> Xanh ngọc
(19, 2), -- Quần Legging Đen -> Đen
(20, 20);-- Quần Culottes Vàng chanh -> Vàng chanh

INSERT INTO product_size_link (product_id, size_id) VALUES
(1, 1),  -- Áo Thun -> XS
(2, 2),  -- Áo dài tay -> S
(3, 3),  -- Áo Polo -> M
(4, 4),  -- Áo khoác -> L
(5, 5),  -- Áo Sơ Mi -> XL
(6, 6),  -- Áo Tanktop -> 2XL
(7, 7),  -- Áo thể thao -> 3XL
(8, 8),  -- Áo Vest -> 4XL
(9, 1),  -- Áo Len -> XS
(10, 2), -- Áo Hoodie -> S
(11, 9), -- Quần Jean -> 28
(12, 10),-- Quần Jogger -> 29
(13, 11),-- Quần Kaki -> 30
(14, 12),-- Quần Pants -> 31
(15, 13),-- Quần Shorts -> 32
(16, 14),-- Quần Lót -> 33
(17, 15),-- Quần Đùi -> 34
(18, 16),-- Quần bơi -> 36
(19, 17),-- Quần Legging -> 38
(20, 18);-- Quần Culottes -> 40

INSERT INTO "order" (order_date, customer_id, total_amount, payment_method_id) VALUES
('2024-01-01 10:00:00', 51, 100000, 1),
('2024-01-02 11:00:00', 52, 240000, 2),
('2024-01-03 12:00:00', 53, 140000, 1),
('2024-01-04 13:00:00', 54, 600000, 2),
('2024-01-05 14:00:00', 55, 320000, 1),
('2024-01-06 15:00:00', 56, 80000, 2),
('2024-01-07 16:00:00', 57, 240000, 1),
('2024-01-08 17:00:00', 58, 240000, 2),
('2024-01-09 18:00:00', 59, 540000, 1),
('2024-01-10 19:00:00', 60, 440000, 2),
('2024-01-11 10:00:00', 61, 200000, 1),
('2024-01-12 11:00:00', 62, 320000, 2),
('2024-01-13 12:00:00', 63, 140000, 1),
('2024-01-14 13:00:00', 64, 540000, 2),
('2024-01-15 14:00:00', 65, 200000, 1),
('2024-01-16 15:00:00', 66, 40000, 2),
('2024-01-17 16:00:00', 67, 120000, 1),
('2024-01-18 17:00:00', 68, 80000, 2),
('2024-01-19 18:00:00', 69, 360000, 1),
('2024-01-20 19:00:00', 70, 280000, 2);

INSERT INTO order_item (order_id, product_id, quantity, unit_price) VALUES
(41, 1, 1, 100000),
(42, 2, 2, 120000),
(43, 3, 1, 140000),
(44, 4, 3, 200000),
(45, 5, 2, 160000),
(46, 6, 1, 80000),
(47, 7, 2, 120000),
(48, 8, 1, 240000),
(49, 9, 3, 180000),
(50, 10, 2, 220000),
(51, 11, 1, 200000),
(52, 12, 2, 160000),
(53, 13, 1, 140000),
(54, 14, 3, 180000),
(55, 15, 2, 100000),
(56, 16, 1, 40000),
(57, 17, 2, 60000),
(58, 18, 1, 80000),
(59, 19, 3, 120000),
(60, 20, 2, 140000);

