TÀI LIỆU ĐẶC TẢ YÊU CẦU - PHÂN HỆ BACKEND & WEB ADMIN
Dự án: Hệ thống Đặt vé xe khách trực tuyến
Nền tảng: ASP.NET Web Application (MVC & Web API) - .NET Framework

Ngày: 2026-05-24

Tóm tắt
------
Tài liệu này mô tả chi tiết yêu cầu chức năng (functional) và phi chức năng (non-functional) cho phân hệ Web Admin (giao diện quản trị) và Backend API (RESTful) phục vụ Mobile App. Nội dung giúp đội phát triển thực thi, review và kiểm thử tính năng.

Checklist (những bước cần thực hiện)
- [ ] Tạo/hoàn thiện các endpoint RESTful cho Mobile theo phần 3
- [ ] Đảm bảo mã hoá mật khẩu khi đăng ký (hashing) và sinh JWT khi đăng nhập
- [ ] Áp dụng [Authorize(Roles = "Admin")] cho tất cả Controller trong Areas/Admin
- [ ] Triển khai tính năng Khóa ghế (Lock Seat) với xử lý lỗi HTTP 409 khi conflict
- [ ] Sử dụng TransactionScope / transaction LINQ to SQL cho nghiệp vụ Đặt vé/Thanh toán
- [ ] Sử dụng DataLoadOptions (Eager Loading) khi cần lấy dữ liệu liên kết để tránh N+1
- [ ] Viết unit/integration tests cho các API quan trọng (Lock Seat, Create Order, Confirm Payment)
- [ ] Tối ưu Indexes ở DB cho các query tìm kiếm theo Trip/Route/Phone/OrderCode

1. MÔ TẢ TỔNG QUAN
------------------
1.1. Mục đích phân hệ
- Web Admin: giao diện quản trị (MVC views) cho Admin thực hiện CRUD dữ liệu lõi, quản lý đơn, khuyến mãi, báo cáo
- Backend API: tập hợp các API trả JSON để Mobile App (Android) truy xuất và thao tác thời gian thực

1.2. Môi trường công nghệ
- Kiến trúc: MVC + Web API trên .NET (project hiện tại sử dụng LINQ to SQL với .dbml)
- Bảo mật: Token-based Authentication (JWT / Bearer Token). Sử dụng header Authorization: Bearer <token>

2. YÊU CẦU CHỨC NĂNG: WEB ADMIN (GIAO DIỆN QUẢN TRỊ)
-------------------------------------------------
Lưu ý: Những controller trong Areas/Admin/* là nơi triển khai; cần bảo vệ bằng role Admin.

2.1. Module Tổng quan (Dashboard)
- AD-01: Các thẻ thống kê: Tổng doanh thu, Tổng vé đã bán, Số KH mới, Số nhà xe đối tác. (API/Service trả các số liệu này để View hiển thị)
- AD-02: Biểu đồ xu hướng đặt vé (7-30 ngày). API trả dữ liệu series theo ngày: {date, revenue, ticketsSold}
- AD-03: Bảng top 10 đơn mới nhất trạng thái chờ xử lý (Pending). Hiển thị OrderCode, CustomerName, Phone, Trip info, Tổng tiền, Thời gian tạo

2.2. Module Quản lý Dữ liệu lõi (Core Data CRUD)
- AD-04 Quản lý Nhà xe (BusOperator): CRUD (Tên, Hotline, Chính sách mô tả, trạng thái)
- AD-05 Quản lý Tuyến (Route): Điểm đi, điểm đến (có thể kèm mã/slug)
- AD-06 Quản lý Chuyến (Trip): Tạo lịch chạy, liên kết FK tới BusOperator và Route, thông tin ngày giờ xuất phát, giá vé cơ bản, số ghế, loại xe

2.3. Module Quản lý Kinh doanh & Vận hành
- AD-07 Quản lý Đơn hàng/Vé: Lọc/tìm kiếm theo mã vé (order code) hoặc SĐT, export CSV, thao tác Hủy vé (manual cancel) — cập nhật trạng thái order và giải phóng ghế
- AD-08 Quản lý Mã khuyến mãi (Promotion): Tạo mã code, kiểu giảm (fixed / percent), điều kiện (tối thiểu), số lần dùng, thời hạn
- AD-09 Quản lý Người dùng: Danh sách khách hàng, hạng thành viên, điểm tích luỹ, lịch sử mua vé

3. YÊU CẦU CHỨC NĂNG: BACKEND API (CHO MOBILE APP)
------------------------------------------------
Ghi chú chung: Các API trả JSON. Trừ trang quản trị MVC, tất cả API cần xác thực bằng token khi yêu cầu.

3.1. API Tài khoản & Định danh
- API-01 Đăng ký (POST /api/account/register)
  - Input: { FullName, Phone, Email, Password }
  - Xử lý: hash password (ví dụ BCrypt) trước khi lưu vào User table
  - Output: { success: true, message }

- API-02 Đăng nhập (POST /api/account/login)
  - Input: { PhoneOrEmail, Password }
  - Xử lý: kiểm tra, nếu hợp lệ -> tạo JWT (chuỗi token) kèm expiry (ví dụ 24h)
  - Output: { token, expiresIn, user: { id, name, phone, rank, points } }

- API-03 Hồ sơ người dùng (GET /api/account/profile)
  - Auth required
  - Output: { id, fullName, phone, email, points, rank, purchaseHistorySummary }

3.2. API Tìm kiếm & Dữ liệu
- API-04 Tìm chuyến xe (GET /api/trips/search?from=...&to=...&date=yyyy-MM-dd)
  - Truy vấn LINQ liên kết Trips, Routes, BusOperators
  - Sử dụng Eager Loading (DataLoadOptions) để lấy thông tin BusOperator + Route cùng lúc
  - Output: list of trips: { TripId, Operator: {id,name}, Route: {...}, departTime, basePrice, seatsAvailable }

- API-05 Lấy sơ đồ ghế (GET /api/trips/{tripId}/seats)
  - Output: [{ SeatId, SeatNumber, Status }] status: 0 empty, 1 locked, 2 sold

3.3. API Đặt vé (Nghiệp vụ cốt lõi)
- API-06 Khóa ghế (POST /api/trips/{tripId}/lock)
  - Input: { userId, seats: [seatNumber,...] }
  - Xử lý: kiểm tra trạng thái seat trong DB
    - Nếu trống -> cập nhật trạng thái=1 (locked) và ghi thời gian lock (DateTime.UtcNow) + ttl (ví dụ 10 phút)
    - Nếu seat đang locked or sold bởi người khác -> trả HTTP 409 Conflict cùng body { error: "SeatConflict", seat: 12 }
  - Trả về: { success: true, lockedSeats: [...] }

- API-07 Tạo đơn hàng (POST /api/orders)
  - Input: { userId, tripId, seats: [...], promotionCode?, usePoints? }
  - Xử lý: phải chạy trong Transaction (TransactionScope)
    - kiểm tra trạng thái ghế (vẫn locked bởi user hoặc free)
    - tính giá, áp dụng giảm giá, trừ điểm nếu có
    - lưu Order và OrderDetails
    - nếu tất cả OK -> trả OrderId và trạng thái pending payment
  - Output: { orderId, totalAmount, status }

- API-08 Xác nhận thanh toán (POST /api/orders/{orderId}/confirm)
  - Input: { paymentInfo }
  - Xử lý (Transaction): cập nhật trạng thái order = Paid, cập nhật ghế thành Sold (2)
  - Trong trường hợp bất kỳ bước nào lỗi -> rollback và trả lỗi phù hợp

HTTP Codes được sử dụng
- 200 OK - Thao tác thành công kèm payload
- 201 Created - Tạo mới resource
- 400 Bad Request - Dữ liệu đầu vào không hợp lệ
- 401 Unauthorized - Token sai hoặc hết hạn
- 403 Forbidden - Không có quyền truy cập
- 404 Not Found - Resource không tồn tại
- 409 Conflict - Tranh chấp ghế (seat already locked/sold)
- 500 Internal Server Error - lỗi server

4. YÊU CẦU PHI CHỨC NĂNG
---------------------
4.1. Bảo mật
- Web Admin: mọi Controller trong Areas/Admin phải có [Authorize(Roles = "Admin")]
  - Ví dụ file paths: Areas/Admin/Controllers/* (DashboardController.cs, BusOperatorController.cs, RouteController.cs, TripController.cs, TicketController.cs, PromotionController.cs, UserController.cs)
  - Kiểm tra Startup/Program.cs để đảm bảo JWT middleware được cấu hình và role claim tồn tại trong token

4.2. Toàn vẹn dữ liệu (Data Integrity)
- Đặt vé / Thanh toán phải dùng TransactionScope (LINQ to SQL) để đảm bảo atomicity
  - Nếu dùng System.Transactions.TransactionScope, hãy đảm bảo ambient transaction cover DataContext.SubmitChanges()
  - Hoặc dùng explicit DbTransaction nếu sử dụng ADO.NET/SqlConnection trực tiếp

4.3. Hiệu năng
- Sử dụng Eager Loading khi cần lấy dữ liệu quan hệ để tránh N+1 (LINQ to SQL: DataLoadOptions.LoadWith)
- Tối ưu các query tìm kiếm: thêm index cho cột Trip.Date, Route.FromId/ToId, Orders.OrderCode, Users.Phone
- Giới hạn số bản ghi trả về (paging) cho các API danh sách

5. GỢI Ý KIẾN TRÚC & THIẾT KẾ CHI TIẾT
-----------------------------------
5.1. Models / Schema chính (tương ứng với /Models trong repo)
- User: Id, FullName, Phone, Email, PasswordHash, Points, Rank
- BusOperator: Id, Name, Hotline, Policy, Status
- Route: Id, From, To, Distance?
- Trip: Id, RouteId, OperatorId, DepartAt (DateTime), BasePrice, TotalSeats
- Seat: Id, TripId, SeatNumber, Status (0,1,2), LockedByUserId?, LockedAt?
- Order: Id, OrderCode, UserId, TripId, TotalAmount, Status (Pending, Paid, Cancelled), CreatedAt
- OrderDetail: Id, OrderId, SeatId, Price
- Promotion: Id, Code, DiscountAmount, DiscountPercent, MinOrderAmount, ExpiryDate

5.2. Dịch vụ (Services)
- IAccountService: Register, Login, GetProfile
- ITripService: SearchTrips, GetSeats
- IOrderService: LockSeats, CreateOrder (Transaction), ConfirmPayment (Transaction), CancelOrder
- IPromotionService: ValidatePromotion

5.3. Mẫu API signatures (gợi ý implementation)
- POST /api/account/register
- POST /api/account/login
- GET /api/account/profile
- GET /api/trips/search?from=&to=&date=
- GET /api/trips/{tripId}/seats
- POST /api/trips/{tripId}/lock
- POST /api/orders
- POST /api/orders/{orderId}/confirm

5.4. Flow Khóa ghế -> Tạo đơn -> Xác nhận thanh toán
1) Client gọi /trips/{id}/lock để giữ ghế (server lưu status=1 và LockedAt)
2) Client gọi /orders để tạo đơn (kiểm tra ghế vẫn locked bởi user hoặc release expired locks)
3) Client thực hiện thanh toán; khi xác nhận -> /orders/{id}/confirm (server đổi ghế status=2 và đổi order status=Paid)
Transaction: bước 2 và 3 cần transaction để đồng thời cập nhật orders + seats

5.5. Xử lý lock expired
- Nên có job/background (cron) hoặc khi truy vấn ghế, tự động release các lock > TTL (ví dụ 10 phút)

6. Kiểm thử & Triển khai
-----------------------
- Viết unit tests cho service layer (mock DataContext)
- Viết integration tests cho endpoint trọng yếu (Lock seat conflict, create order rollback scenario)
- Khi triển khai, bật logging chi tiết cho các transaction thất bại để dễ debug

7. Tài liệu triển khai nhanh (Quick Run)
- Kiểm tra file `Helpers/JwtHelper.cs` để biết cách tạo token và cấu trúc claims
- Kiểm tra `Areas/Admin/Controllers` để gắn thêm [Authorize(Roles = "Admin")] nếu chưa có
- Kiểm tra `Controllers/AuthController.cs` hiện có để tái sử dụng endpoint login/register hoặc sửa cho tạo JWT

8. Phụ lục: Ví dụ lỗi khi cố gắng khóa ghế
Response HTTP 409
{
  "error": "SeatConflict",
  "message": "Seat 12 is already locked or sold",
  "seat": 12,
  "currentStatus": 2
}

Kết luận
-------
Tài liệu này cung cấp đặc tả chi tiết để hoàn thiện phân hệ Web Admin và Backend API phục vụ Mobile App. Nếu bạn muốn, tôi có thể tiếp theo:
- Sinh các stub controllers & API signatures dựa trên spec này trong project hiện có
- Thực hiện các chỉnh sửa bảo mật (thêm [Authorize] vào controllers trong Areas/Admin)
- Viết unit/integration tests cho các endpoint quan trọng

Hãy cho biết bước tiếp theo bạn muốn tôi thực hiện (ví dụ: tạo stub endpoints, thêm authorize attributes, hoặc implement tính năng Lock Seat + Transaction ở service layer).

