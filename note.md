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
    - dotnet build -p:Platform=x64
    - dotnet ef migrations add AllowNullForeignKeysForAttributes --msbuildparams "/p:Platform=x64"

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

  - Có 4 trạng thái: Đang xử lý (dùng khi giao hàng), Hoàn thành, Đã hủy, Đã hoàn trả
  - Khi trạng thái Đang xử lí thì có thể chuyển thành Hoàn thành hoặc Đã hủy
  - Khi trạng thái Hoàn thành thì có thể chuyển thành Đã hoàn trả

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

- Trang bán hàng:

  - Có thanh tìm kiếm theo số điện thoại khách hàng, nếu không nhập gì thì hiển thị danh sách tài khoản khách hàng hiện có. Khi bấm vào 1 tài khoản thì là chọn tài khoản đó để thanh toán.
  - Trong giao diện bán hàng sẽ có nút "thanh toán"
    Bấm thanh toán sẽ hiển thị content dialog như sau:

    - Phương thức: combo box tiền mặt/chuyển khoản

    - Vận chuyển: checkbox vận chuyển
      - Có, và có tài khoản thì -> có vận chuyển và tích điểm (1)
      - Có, kh tài khoản -> exception và show error dialog (2)
      - Không, và có tài khoản thì -> không vận chuyển và chỉ tích điểm (3)
      - Không, không có tài khoản -> không vận chuyển và không tích điểm, customer_id trong hóa đơn = null (4)
    - Hiển thị số điểm thưởng hiện có:
      - Nếu có tài khoản thì tài khoản sẽ có thể có điểm thưởng >= 0
      - Nếu không có tài khoản thì sẽ không có điểm thưởng nhưng vẫn hiển thị là 0
    - Dùng điểm thưởng: NumberBox
      - Nhập số điểm thưởng cần dùng
    - button Thanh toán:
      - Đầu tiên check nếu:
        - Nếu có tài khoản thì check nếu số điểm thưởng cần dùng lớn hơn số điểm thưởng hiện có thì sẽ show error dialog (2)
        - Nếu không có tài khoản mà số điểm thưởng cần dùng > 0 thì sẽ show error dialog (2)
      - (1) -> tạo hóa đơn với status "Đang xử lý", chưa cộng điểm cho khách hàng (nếu có tài khoản)
      - (3), (4) -> tạo hóa đơn với status "Hoàn thành", cộng điểm cho khách hàng (nếu có tài khoản)
      - (2) -> show error dialog
    - Khi tạo hóa đơn, sẽ tự động tạo order mới trong bảng order, các order_item mới dựa trên các biến thể mua trong đơn hàng, giảm số lượng của các biến thể trong bảng biến thể (product_variant), cộng điểm dựa trên button Thanh toán ở trên. Nhớ là khi có dùng điểm thưởng thì sẽ trừ đi số tiền tương ứng với số điểm thưởng đã dùng trong hóa đơn (net_total). Subtotal thì là tổng tiền ban đầu chưa trừ số tiền tương ứng số điểm thưởng cần dùng.

- Trang order: (chỉ có thể sửa chửa đơn hàng, không thể xóa đơn hàng; Tạo đơn hàng thì nằm trong trang bán hàng)

  - Khi chỉnh sửa đơn hàng (trạng thái đơn hàng).
    - Khi khách hàng hoàn trả sản phẩm, người chủ sẽ vào trang order để sửa đơn hàng đó thành "đã hoàn trả" (tìm kiếm theo mã hóa đơn)
    - Khi chỉnh sửa đơn hàng thành "đã hoàn trả" thì sẽ tự động cộng điểm cho khách hàng (điểm sẽ được cộng vào tài khoản của khách hàng), số sản phẩm trong đơn hàng sẽ được cộng vào kho hàng (số lượng sản phẩm trong kho sẽ được cộng thêm số lượng sản phẩm trong đơn hàng) (**1**)
    - Khi giao hàng thành công, người chủ sẽ vào trang order để sửa đơn hàng đó thành "đã hoàn thành" (tìm kiếm theo mã hóa đơn)
    - Khi giao hàng không thành công (bị hủy đơn), người chủ sẽ vào trang order để sửa đơn hàng đó thành "đã hủy" (tìm kiếm theo mã hóa đơn) và sẽ tự động cộng điểm cho khách hàng (điểm sẽ được cộng vào tài khoản của khách hàng), số sản phẩm trong đơn hàng sẽ được cộng vào kho hàng (số lượng sản phẩm trong kho sẽ được cộng thêm số lượng sản phẩm trong đơn hàng) (**1**)

- Report:

  - Doanh thu:

    - Mặc định là sẽ hiển thị theo ngày trước, có thể chọn theo tháng hoặc theo năm.
      - Bấm vào lợi nhuận theo ngày/tháng/năm sẽ hiển thị lợi nhuận theo ngày/tháng/năm và Hiển thị số đơn đã hoàn thành, số đơn đã hủy, số đơn đã hoàn trả trong ngày hôm nay/tháng này/năm này
      - Lợi nhuận theo ngày = tổng net_total trong ngày của các đơn đã hoàn thành - tổng import_price của các sản phẩm có trong các đơn đã hoàn thành trong ngày hôm nay
      - Lợi nhuận theo tháng = tổng net_total trong tháng của các đơn đã hoàn thành - tổng import_price của các sản phẩm có trong các đơn đã hoàn thành trong tháng này
      - Lợi nhuận theo năm = tổng net_total trong năm của các đơn đã hoàn thành - tổng import_price của các sản phẩm có trong các đơn đã hoàn thành trong năm này

  - Biểu đồ:
    - Doanh thu
      - Doanh thu theo ngày (10 ngày gần nhất)
      - Doanh thu theo tháng (12 tháng)
      - Doanh thu theo năm (5 năm gần nhất)
    - Sản phẩm bán chạy
      - Hiển thị 10 sản phẩm bán chạy nhất trong ngày/tháng/năm (theo số lượng mua)

- Bug:
