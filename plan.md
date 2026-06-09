# 🗺️ DEVELOPMENT PLAN – BookStore MVC

> **Vai trò:** Technical Architect → Mentor cho Fresher BackEnd Developer .NET  
> **Mục tiêu:** Xây dựng ứng dụng BookStore MVC theo từng bước nhỏ, mỗi task có thể commit và kiểm tra độc lập.

---

## Tổng timeline gợi ý

| Phase | Task | Mô tả ngắn |
|-------|------|------------|
| 1 – Setup | 1 → 3 | Khởi tạo project, Identity, DbContext |
| 2 – Domain | 4 → 6 | Entity + Migration từng bước |
| 3 – Admin | 7 → 9 | Area Admin + CRUD |
| 4 – Client | 10 → 12 | Hiển thị sách, tìm kiếm |
| 5 – Order | 13 → 15 | Giỏ hàng → Checkout → Quản lý đơn |
| 6 – Security | 16 → 17 | Role + Dashboard |
| 7 – Polish | 18 → 20 | Validation, Service Layer, Seed |
| 8 – DevOps | 21 | Docker + docker-compose |

---

# 🚀 PHASE 1 – Setup & Foundation

## Task 1 – Init Project

### Yêu cầu

* Tạo project ASP.NET Core MVC (.NET 6 hoặc 8)
* Cài đặt EF Core packages cơ bản

### Làm gì

* `dotnet new mvc -n BookStore`
* Add packages: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`
* Cấu hình `appsettings.json` → connection string
* Đăng ký DbContext trong `Program.cs` (chưa cần entity)

### DoD (Definition of Done)

* [ ] Project chạy được (`dotnet run`)
* [ ] Kết nối SQL Server thành công (không lỗi startup)
* [ ] Có `.gitignore` phù hợp cho .NET

### 💡 Mentor note

* Commit ngay sau task này: `feat: init BookStore MVC project`
* Không thêm logic business ở bước này — chỉ setup skeleton

---

## Task 2 – Identity Setup

### Yêu cầu

* Dùng **ASP.NET Core Identity** — không tự viết auth từ đầu

### Làm gì

* Add packages Identity + EF Core
* Tạo `ApplicationUser` kế thừa `IdentityUser`
* Cấu hình Identity trong `Program.cs`
* Scaffold UI: Login / Register / Logout (Razor Pages hoặc MVC)

### DoD

* [ ] Register tạo user mới thành công
* [ ] Login / Logout hoạt động
* [ ] Bảng `AspNetUsers`, `AspNetRoles` có trong DB sau migration

### 💡 Mentor note

* Identity là chuẩn industry — fresher cần hiểu flow này trước khi làm authorization (Task 16)

---

## Task 3 – DbContext + Migration

### Yêu cầu

* Tạo `ApplicationDbContext` kế thừa `IdentityDbContext<ApplicationUser>`
* Chạy migration lần đầu (chỉ Identity tables)

### Làm gì

* Tạo class `ApplicationDbContext`
* Override `OnModelCreating` nếu cần (để trống cũng được ở bước này)
* Chạy:

```bash
Add-Migration InitIdentity
Update-Database
```

### DoD

* [ ] Migration chạy không lỗi
* [ ] DB có đủ bảng Identity
* [ ] **Chưa** thêm DbSet Book/Category/Order — sẽ làm ở Task 4–6

### 💡 Mentor note

* Tách migration theo entity giúp dễ debug khi fresher gặp lỗi schema
* Mỗi entity = 1 migration riêng (Task 4, 5, 6)

---

# 📦 PHASE 2 – Domain Models (Entity-first)

> **Tại sao entity trước CRUD?** Fresher cần hiểu data model trước khi viết controller. Đây là thứ tự đúng trong thực tế.

## Task 4 – Category Entity

### Yêu cầu

* Tạo entity `Category` với các field cơ bản

### Làm gì

* Tạo model `Category`:

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
}
```

* Thêm `DbSet<Category>` vào DbContext
* Migration: `Add-Migration AddCategory`

### DoD

* [ ] Bảng `Categories` tồn tại trong DB
* [ ] Có thể insert 1 record test qua SQL hoặc seed tạm

---

## Task 5 – Book Entity

### Yêu cầu

* Tạo entity `Book` có quan hệ với `Category`

### Làm gì

* Tạo model `Book`:

```csharp
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}
```

* Cấu hình FK trong `OnModelCreating` (hoặc convention)
* Migration: `Add-Migration AddBook`

### DoD

* [ ] Bảng `Books` có FK → `Categories`
* [ ] Quan hệ 1-n (1 Category – nhiều Book) hoạt động

### 💡 Mentor note

* Nhắc fresher: **Price lưu dạng `decimal`**, không dùng `float`/`double` cho tiền

---

## Task 6 – Order + OrderDetail

### Yêu cầu

* Tạo entity cho luồng đặt hàng

### Làm gì

* Tạo `Order`:

```csharp
public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } // Pending, Shipped, Completed...
    public ApplicationUser User { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; }
}
```

* Tạo `OrderDetail`:

```csharp
public class OrderDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int BookId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // snapshot giá tại thời điểm mua
    public Order Order { get; set; }
    public Book Book { get; set; }
}
```

* Migration: `Add-Migration AddOrder`

### DoD

* [ ] Bảng `Orders`, `OrderDetails` tạo thành công
* [ ] FK: Order → User, OrderDetail → Order, OrderDetail → Book
* [ ] Hiểu được tại sao cần `UnitPrice` (giá sách có thể thay đổi sau này)

### 💡 Mentor note

* Đây là bài học quan trọng: **snapshot price** — không reference Price từ Book khi hiển thị order cũ

---

# 🔧 PHASE 3 – Admin Area & CRUD

## Task 7 – Admin Area

### Yêu cầu

* Tạo cấu trúc Area riêng cho Admin

### Làm gì

* Tạo `/Areas/Admin/Controllers`, `/Areas/Admin/Views`
* Layout riêng cho Admin (`_Layout.cshtml` trong Admin)
* Route mặc định: `/Admin/{Controller}/{Action}`

### DoD

* [ ] Truy cập `/Admin/Home/Index` (hoặc Dashboard placeholder) không lỗi 404
* [ ] Admin layout khác với client layout
* [ ] Menu sidebar/header cơ bản

### 💡 Mentor note

* Làm Area **trước** CRUD — mọi controller admin sau này đặt trong Area này

---

## Task 8 – Category CRUD

### Yêu cầu

* Admin quản lý danh mục sách

### Làm gì

* `Areas/Admin/Controllers/CategoryController`
* Actions: Index, Create, Edit, Delete
* Views tương ứng + form validation cơ bản

### DoD

* [ ] List / Create / Edit / Delete Category hoạt động
* [ ] Không xóa Category nếu còn Book liên quan (hoặc cascade có chủ đích)
* [ ] Redirect + thông báo sau mỗi action

---

## Task 9 – Book CRUD

### Yêu cầu

* Admin quản lý sách, gồm upload ảnh

### Làm gì

* `Areas/Admin/Controllers/BookController`
* CRUD đầy đủ
* Upload image → lưu `wwwroot/images/books/`
* Dropdown chọn Category khi Create/Edit

### DoD

* [ ] Book hiển thị đúng Category
* [ ] Upload ảnh thành công, `ImageUrl` lưu path đúng
* [ ] Edit không mất ảnh cũ nếu không upload mới

---

# 🌐 PHASE 4 – Client (Storefront)

## Task 10 – Book Listing

### Yêu cầu

* Trang chủ / danh sách sách cho khách hàng

### Làm gì

* `HomeController` hoặc `BookController` (client area)
* Hiển thị grid/list book từ DB
* **Pagination** (Skip/Take hoặc library)

### DoD

* [ ] Hiển thị Title, Author, Price, ảnh, Category
* [ ] Có phân trang (vd: 12 sách/trang)
* [ ] Query từ DB, không hardcode

---

## Task 11 – Book Detail

### Yêu cầu

* Trang chi tiết 1 cuốn sách

### Làm gì

* Action `Details(int id)`
* Hiển thị đầy đủ thông tin + nút "Add to Cart" (wire ở Task 13)

### DoD

* [ ] Route: `/Book/Details/{id}`
* [ ] 404 hoặc NotFound nếu id không tồn tại
* [ ] Hiển thị ảnh, mô tả, giá, tác giả, category

---

## Task 12 – Search & Filter

### Yêu cầu

* Tìm kiếm và lọc sách trên trang listing

### Làm gì

* Search theo `Title` (và/hoặc `Author`)
* Filter theo `CategoryId`
* Query filter **ở DB** (LINQ Where), không load all rồi filter in-memory

### DoD

* [ ] Search + Filter kết hợp được
* [ ] Pagination vẫn hoạt động sau filter
* [ ] URL có thể bookmark (query string: `?search=...&categoryId=...`)

### 💡 Mentor note

* Đây là task test kỹ năng LINQ — review query plan nếu fresher dùng `.ToList()` quá sớm

---

# 🛒 PHASE 5 – Cart & Order Flow

## Task 13 – Cart Session

### Yêu cầu

* Giỏ hàng lưu bằng **Session** (không cần bảng Cart trong DB)

### Làm gì

* Model `CartItem` (BookId, Title, Price, Quantity, ImageUrl)
* Serialize cart → Session (JSON)
* Actions: Add, Remove, Update quantity, View cart

### DoD

* [ ] Add to cart từ Book Detail
* [ ] Refresh page không mất cart
* [ ] Tính subtotal đúng (Price × Quantity)
* [ ] Session configured trong `Program.cs`

---

## Task 14 – Checkout

### Yêu cầu

* Chuyển giỏ hàng thành Order trong DB

### Làm gì

* Action Checkout (chỉ user đã login)
* Tạo `Order` + các `OrderDetail` từ cart
* Logic bắt buộc:
  * Copy `Book.Price` → `OrderDetail.UnitPrice`
  * Tính `Order.TotalAmount`
  * Set `Status = "Pending"`
* Clear cart sau khi thành công

### DoD

* [ ] Sau checkout: DB có Order + OrderDetails
* [ ] UnitPrice = giá tại thời điểm mua (không phải giá hiện tại của Book nếu đã đổi)
* [ ] Cart session được xóa
* [ ] Redirect đến trang xác nhận đơn hàng

### 💡 Mentor note

* Dùng transaction nếu có thể — fresher nên học `BeginTransaction` ở đây

---

## Task 15 – Order Management

### Yêu cầu

* Admin xem và cập nhật trạng thái đơn hàng

### Làm gì

* `Areas/Admin/Controllers/OrderController`
* Index: list orders (kèm user, ngày, total, status)
* Details: xem chi tiết OrderDetails
* Update status: Pending → Shipped → Completed

### DoD

* [ ] Admin xem được tất cả orders
* [ ] Update status thành công
* [ ] User (client) xem được lịch sử đơn hàng của mình (bonus)

---

# 🔐 PHASE 6 – Security & Reporting

## Task 16 – Role Authorization

### Yêu cầu

* Phân quyền Admin vs Customer

### Làm gì

* Seed role `"Admin"` và `"Customer"`
* Gán role Admin cho 1 user test
* `[Authorize(Roles = "Admin")]` trên Admin Area controllers
* Redirect unauthorized → Access Denied page

### DoD

* [ ] User thường không truy cập được `/Admin/*`
* [ ] User Admin truy cập được
* [ ] Seed role chạy khi app start (hoặc migration seed)

---

## Task 17 – Dashboard

### Yêu cầu

* Trang tổng quan cho Admin

### Làm gì

* `Admin/Home/Index` hoặc `DashboardController`
* Hiển thị:
  * Tổng số Orders
  * Tổng Revenue (sum TotalAmount)
  * (Bonus) Orders theo status, top books...

### DoD

* [ ] Số liệu query từ DB, chính xác
* [ ] Không query N+1 — dùng aggregate LINQ

---

# ⭐ PHASE 7 – Polish & Architecture

## Task 18 – Validation

### Yêu cầu

* Validation nhất quán trên toàn app

### Làm gì

* Data Annotations trên Entity / ViewModel (`[Required]`, `[Range]`, `[StringLength]`)
* Hiển thị `ValidationSummary` + field errors trên form
* Validate phía server (ModelState.IsValid)

### DoD

* [ ] Form không submit khi invalid
* [ ] Error message rõ ràng, tiếng Việt hoặc tiếng Anh thống nhất
* [ ] Price không âm, Title không rỗng, Quantity > 0

### 💡 Mentor note

* Client-side validation (jquery unobtrusive) là bonus — server-side là bắt buộc

---

## Task 19 – Service Layer

### Yêu cầu

* Tách business logic ra khỏi Controller

### Làm gì

* Tạo folder `/Services`
* Ví dụ: `IBookService`, `BookService`, `IOrderService`, `OrderService`...
* Đăng ký DI trong `Program.cs`
* Controller chỉ: nhận request → gọi service → trả view/redirect

### DoD

* [ ] Controller mỏng (< 15 dòng/action ideally)
* [ ] Logic query, tính toán nằm trong Service
* [ ] Có interface cho mỗi service (dễ test sau này)

### 💡 Mentor note

* Refactor **sau** khi feature chạy — không premature abstraction từ Task 1

---

## Task 20 – Seed Data

### Yêu cầu

* Dữ liệu mẫu để demo và test

### Làm gì

* Tạo `DbInitializer` hoặc `DataSeeder`
* Seed: Categories, Books, Admin user + role
* Gọi seed khi app start (chỉ Development) hoặc migration extension

### DoD

* [ ] App chạy lên có sẵn vài Category + Book
* [ ] Có tài khoản Admin để login test
* [ ] Seed idempotent (chạy lại không duplicate)

---

# 🐳 PHASE 8 – DevOps

## Task 21 – Docker Setup

### Yêu cầu

* Containerize ứng dụng BookStore + SQL Server để chạy local bằng một lệnh

### Làm gì

* Tạo `Dockerfile` cho project MVC (multi-stage build):

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookStore.dll"]
```

* Tạo `.dockerignore` (loại `bin/`, `obj/`, `.git`, `.vs`...)
* Tạo `docker-compose.yml`:

```yaml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "Your_strong_Password123"
    ports:
      - "1433:1433"

  web:
    build: .
    ports:
      - "8080:8080"
    environment:
      ConnectionStrings__DefaultConnection: "Server=db;Database=BookStore;User Id=sa;Password=Your_strong_Password123;TrustServerCertificate=True"
    depends_on:
      - db
```

* Cấu hình app đọc connection string từ **environment variable** (không hardcode trong `appsettings.json`)
* Chạy migration khi container start (hoặc script init riêng)

### DoD

* [ ] `docker compose up --build` chạy thành công
* [ ] Truy cập app tại `http://localhost:8080`
* [ ] App kết nối DB trong container `db`, migration/seed chạy OK
* [ ] `.dockerignore` giúp build nhanh, image không phình to
* [ ] README có hướng dẫn chạy bằng Docker

### 💡 Mentor note

* Làm task này **sau Task 20** — cần app + seed data ổn định trước
* Password SA chỉ dùng cho **local/dev** — không commit password production
* Fresher nên hiểu: `ConnectionStrings__DefaultConnection` map vào `ConnectionStrings:DefaultConnection` trong .NET config
* Bonus: thêm `docker-compose.override.yml` cho dev, `.env` cho biến môi trường

---

# ⚠️ Quy tắc bắt buộc (Mentor Guidelines)

## 1. Thứ tự làm việc

```
Entity → Migration → CRUD/Feature → Refactor
```

Không nhảy cóc sang UI khi chưa có model và migration.

## 2. Không làm kiểu "cho chạy"

| ❌ Sai | ✅ Đúng |
|--------|---------|
| Query trong View | Query trong Service |
| Logic nặng trong Controller | Controller điều phối, Service xử lý |
| Load all DB rồi filter | LINQ filter trên IQueryable |

## 3. Không over-engineer

* ❌ Clean Architecture full (UseCase, Domain Event...)
* ❌ Microservices
* ❌ Generic Repository + Unit of Work phức tạp
* ✅ Service Layer đơn giản là đủ cho project này

## 4. Commit chuẩn

```
feat: add category entity and migration
feat: implement cart session
fix: order total calculation
refactor: extract book logic to service layer
```

Mỗi task ≈ 1–2 commit. Không commit 20 file lung tung không liên quan.

---

# 🎯 Milestone hoàn chỉnh

Bạn **DONE** khi hoàn thành Task 1–21 và:

* [ ] CRUD Book + Category (Admin) OK
* [ ] Client: list, detail, search/filter OK
* [ ] Cart → Checkout → Order flow end-to-end
* [ ] Admin quản lý order + dashboard
* [ ] Auth + Role Admin hoạt động
* [ ] Service layer tách biệt rõ ràng
* [ ] Seed data để demo nhanh
* [ ] Chạy được toàn bộ stack bằng Docker Compose

---

# 💬 Gợi ý sau khi hoàn thành

* Viết **README**:
  * Tech stack
  * Hướng dẫn chạy local
  * Tài khoản test (Admin / Customer)
  * Screenshot các màn hình chính
* Docker đã có ở Task 21 — dùng image để deploy lên Azure App Service, VPS hoặc cloud khác
* (Bonus) Viết 2–3 unit test cho OrderService

---

# 📋 Checklist nhanh (21 Tasks)

- [ ] Task 1 – Init Project
- [ ] Task 2 – Identity Setup
- [ ] Task 3 – DbContext + Migration
- [ ] Task 4 – Category Entity
- [ ] Task 5 – Book Entity
- [ ] Task 6 – Order + OrderDetail
- [ ] Task 7 – Admin Area
- [ ] Task 8 – Category CRUD
- [ ] Task 9 – Book CRUD
- [ ] Task 10 – Book Listing
- [ ] Task 11 – Book Detail
- [ ] Task 12 – Search & Filter
- [ ] Task 13 – Cart Session
- [ ] Task 14 – Checkout
- [ ] Task 15 – Order Management
- [ ] Task 16 – Role Authorization
- [ ] Task 17 – Dashboard
- [ ] Task 18 – Validation
- [ ] Task 19 – Service Layer
- [ ] Task 20 – Seed Data
- [ ] Task 21 – Docker Setup
