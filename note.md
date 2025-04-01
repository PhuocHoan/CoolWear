# CoolWear

## File .env

NODE_ENV=development
POSTGRES_HOST=db_1
POSTGRES_PORT=5432
POSTGRES_DB=coolwear
POSTGRES_USER=postgres
POSTGRES_PASSWORD=1234

## Architecture, pattern, and techniques used

- Architecture:
  - MVVM
    - Model: like DTO
    - View Model: implement properties and commands which View data bind to and notify any state changes through the View through change notification events
    - View: define layout, appearance on the screen for user
- Pattern:
  - Observer: if object changes, then it notifies the dependent objects and changes too
  - unit of work: Apply changes to database in one place, keep all repositories inside
  - repository: Implement services for all kind of model
  - specification: for dynamic and complex queries
- Technique:
  - dependency injection (DI): register service
  - inversion of control (IoC): use service from ServiceManager

## Command Setup

- ef core:

  - Tạo toàn bộ database mapping từ database sang dotnet (scaffolding database using efcore):
    - dotnet ef dbcontext scaffold "Host=localhost;Database=coolwear;Username=postgres;Password=1234" Npgsql.EntityFrameworkCore.PostgreSQL --output-dir Models --context PostgresContext --no-onconfiguring --context-dir Data --force

- knex migration

  - knex migrate:make migration1 : tạo file migration1
  - Chỉnh sửa file migration1
  - knex migrate:latest (run migration1)

## Project Planning Features (Chính sách tích điểm)

- Chính sách tích điểm:

  - Hóa đơn trên 100000 đồng: +1 điểm
  - Hóa đơn trên 1000000 đồng: +10 điểm

- Hoàn trả sản phẩm:

  - Hóa đơn trên 100000 đồng: -1 điểm
  - Hóa đơn trên 1000000 đồng: -10 điểm

- Đổi điểm:

  - 1 điểm = 1000 đồng

- Đơn hàng:

  - Có 4 trạng thái: Đang xử lý (dùng khi giao hàng), hoàn thành, Đã hủy, Đã hoàn trả
  - Khi trạng thái đang xử lí thì có thể chuyển thành hoàn thành hoặc đã hủy

- Delete operation:

  - 2 options for delete:

    - 1: avoid delete if have these errors => show error when try to delete
    - 2: add new column isDeleted: boolean, default: false to table where have errors => do not make real delete, instead isDeleted to true (preferred)

  - Delete Case:
    - Delete category => product.category_id = null
    - Delete product => isDeleted = true (option 2)
    - Delete product_variant => product_variant.isDeleted = true
    - Delete color => product_variant.color_id = null
    - Delete size => product_variant.size_id = null
    - Delete customer => customer.isDeleted = true
    - Never delete order or order_item records

- Bug:
