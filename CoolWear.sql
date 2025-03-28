CREATE TABLE "product" (
  "product_id" serial PRIMARY KEY,
  "product_name" varchar not null unique,
  "import_price" integer not null,
  "price" integer not null,
  "stock_quantity" integer not null,
  "category" varchar not null,
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

ALTER TABLE "product_color_link" ADD FOREIGN KEY ("product_id") REFERENCES "product" ("product_id");

ALTER TABLE "product_color_link" ADD FOREIGN KEY ("color_id") REFERENCES "product_color" ("color_id");

ALTER TABLE "product_size_link" ADD FOREIGN KEY ("product_id") REFERENCES "product" ("product_id");

ALTER TABLE "product_size_link" ADD FOREIGN KEY ("size_id") REFERENCES "product_size" ("size_id");

ALTER TABLE "order" ADD FOREIGN KEY ("customer_id") REFERENCES "customer" ("customer_id");

ALTER TABLE "order_item" ADD FOREIGN KEY ("order_id") REFERENCES "order" ("order_id");

ALTER TABLE "order_item" ADD FOREIGN KEY ("product_id") REFERENCES "product" ("product_id");

ALTER TABLE "order" ADD FOREIGN KEY ("payment_method_id") REFERENCES "payment_method" ("payment_method_id");

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

-- Bảng chi tiết đơn hàng
COMMENT ON TABLE "order_item" IS 'Bảng lưu trữ thông tin chi tiết đơn hàng';
COMMENT ON COLUMN "order_item"."order_item_id" IS 'Mã chi tiết đơn hàng (khóa chính)';
COMMENT ON COLUMN "order_item"."order_id" IS 'Mã đơn hàng (khóa ngoại)';
COMMENT ON COLUMN "order_item"."product_id" IS 'Mã sản phẩm (khóa ngoại)';
COMMENT ON COLUMN "order_item"."quantity" IS 'Số lượng';
COMMENT ON COLUMN "order_item"."unit_price" IS 'Đơn giá';
