# BookStore

Ứng dụng web bán sách xây dựng với **ASP.NET Core MVC (.NET 6)**, **Entity Framework Core Code First**, **ASP.NET Core Identity**, và **Service Layer**. Dự án dùng cho học tập / portfolio fresher .NET.

- Kế hoạch phát triển: [`plan.md`](plan.md)
- Yêu cầu sản phẩm: [`PRD.md`](PRD.md)

---

## Tech stack

| Thành phần | Công nghệ |
|------------|-----------|
| Framework | ASP.NET Core MVC 6.0 |
| ORM | Entity Framework Core 6 (SQL Server) |
| Authentication | ASP.NET Core Identity (`ApplicationUser`) |
| Authorization | Role-based (`Admin`) + Area Admin |
| Giỏ hàng | Session + JSON (`ICartSessionService`) + business rules (`ICartService`) |
| Kiến trúc | MVC + Service Layer (`/Services`) |
| UI | Bootstrap 5, Razor Views |

---

## Tính năng chính

### Client (người mua)

- Trang chủ: danh sách sách còn hàng (`Stock > 0`), phân trang, nút **Thêm vào giỏ**
- **Search & filter** trên storefront (`/`): tìm theo Title, lọc Category, khoảng giá (`minPrice`/`maxPrice`); kết hợp được; phân trang giữ query string (bookmarkable)
- Chi tiết sách: `/Book/Details/{id}` (404 nếu không tồn tại; gợi ý sách cùng category)
- Giỏ hàng: thêm / cập nhật số lượng / xóa (lưu Session); **validate tồn kho** trước khi add/update (thông báo qua `TempData`)
- Checkout → tạo `Order` + `OrderDetail` (copy giá từ `Book`, tính `TotalAmount`, trừ tồn kho)
- Đăng ký / đăng nhập / đăng xuất

### Admin (`/Areas/Admin`)

- Dashboard: tổng đơn hàng, tổng doanh thu (chỉ đơn `Completed`), danh sách sách phân trang
- CRUD Category (xóa an toàn — chặn khi category còn sách)
- Quản lý sách trên một controller Admin `Book`: list + search/filter (title, category), Details, CRUD + upload ảnh (`wwwroot/images/books`); xóa an toàn — chặn khi sách đã có trong đơn hàng
- Quản lý đơn hàng: list, detail, cập nhật trạng thái

---

## Cấu trúc thư mục

```
BookStore/
├── Areas/Admin/              # Quản trị (yêu cầu role Admin)
│   ├── Controllers/          # Category, Book, Order, Home
│   └── Views/                # Book (Index/Details/CRUD), Category, Order, Home
├── Controllers/              # Client: Home, Book, Account, Cart, Order
├── Data/                     # ApplicationDbContext, IdentitySeed, CatalogSeed
├── Infrastructure/           # SessionKeys
├── Models/                   # Entity, Roles, OrderStatuses, ViewModels
├── Services/                 # Business logic layer
│   ├── ICartSessionService / CartSessionService   # Session persistence
│   ├── ICartService / CartService                 # Cart business rules (stock)
│   ├── ICategoryService / CategoryService
│   ├── IBookService / BookService
│   ├── IOrderService / OrderService
│   ├── IDashboardService / DashboardService
│   └── CartOperationResult, PlaceOrderResult, DeleteResult
├── Views/                    # Razor views (client)
└── Migrations/               # EF Core migrations
```

**Nguyên tắc:** Controller xử lý HTTP (ModelState, View, Redirect, TempData); Service xử lý nghiệp vụ và truy cập DB.

---

## Chạy project

### Yêu cầu

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- SQL Server (LocalDB hoặc instance đầy đủ)

### Cấu hình database

1. Sửa connection string trong `appsettings.json` nếu cần (mặc định LocalDB: `BookStoreDb`).

```json
"Server=(localdb)\\MSSQLLocalDB;Database=BookStoreDb;Trusted_Connection=True;..."
```

2. Chạy app lần đầu — `Program.cs` tự gọi `MigrateAsync()` khi startup (tạo/cập nhật schema).

   Hoặc áp dụng migration thủ công (tùy chọn):

```bash
dotnet ef database update
```

### Chạy ứng dụng

```bash
dotnet run
```

| Môi trường | URL |
|------------|-----|
| HTTPS | `https://localhost:7199` |
| HTTP | `http://localhost:5127` |

---

## Route quan trọng

| URL | Mô tả | Ghi chú |
|-----|--------|---------|
| `/` | Trang chủ — catalog + search/filter + phân trang | Public; query: `search`, `categoryId`, `minPrice`, `maxPrice`, `page` |
| `/Book/Details/{id}` | Chi tiết sách | Public |
| `/Account/Register` | Đăng ký | |
| `/Account/Login` | Đăng nhập | Admin login → redirect `/Admin/Home` |
| `/Account/AccessDenied` | Không đủ quyền | |
| `/Cart` | Giỏ hàng | Session |
| `/Order/Checkout` | Thanh toán | Cần đăng nhập |
| `/Order/Success/{id}` | Xác nhận đơn hàng | Cần đăng nhập |
| `/Admin/Home` | Dashboard admin | Role **Admin** |
| `/Admin/Category` | CRUD danh mục | Role **Admin** |
| `/Admin/Book` | List + search/filter, Details, CRUD sách + upload ảnh | Role **Admin** |
| `/Admin/Book/Details/{id}` | Chi tiết sách (Admin) | Role **Admin** |
| `/Admin/Order` | Quản lý đơn hàng | Role **Admin** |

Toàn bộ controller trong `Areas/Admin` kế thừa `AdminControllerBase` với `[Authorize(Roles = Roles.Admin)]`.

---

## Seed dữ liệu (Development)

Khi app khởi động trong môi trường **Development**, `Program.cs` thực hiện:

```
MigrateAsync()  →  IdentitySeed  →  CatalogSeed
```

| Bước | File | Mô tả |
|------|------|--------|
| 1 | `Program.cs` | `Database.MigrateAsync()` — apply migration (mọi môi trường) |
| 2 | `Data/IdentitySeed.cs` | Role Admin + user admin (idempotent) |
| 3 | `Data/CatalogSeed.cs` | 3 category + 4 book mẫu (idempotent) |

Seed **chỉ chạy khi** `ASPNETCORE_ENVIRONMENT=Development` (mặc định khi F5).

### Dữ liệu mẫu (Catalog)

| Category | Sách |
|----------|------|
| Fiction | The Great Gatsby, 1984 |
| Science | A Brief History of Time |
| Technology | Clean Code |

- Không gán `Id` thủ công — SQL Server Identity tự sinh; Book dùng `CategoryId` từ entity đã lưu.
- Nếu bảng `Categories` đã có dòng → seed bỏ qua (không duplicate).

### Tài khoản Admin (Identity seed)

| Trường | Giá trị mặc định (dev) |
|--------|-------------------------|
| Email | `admin@bookstore.com` |
| Mật khẩu | `Admin@123` |

User đăng ký qua `/Account/Register` **không** có role Admin.

### Kiểm tra seed trong database

```sql
-- Identity
SELECT u.Email, r.Name AS RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id;

-- Catalog
SELECT COUNT(*) AS CategoryCount FROM Categories;  -- kỳ vọng: 3
SELECT COUNT(*) AS BookCount FROM Books;           -- kỳ vọng: 4

SELECT b.Title, c.Name AS CategoryName
FROM Books b
JOIN Categories c ON b.CategoryId = c.Id;
```

### Reset seed catalog (khi cần test lại)

```sql
DELETE FROM Books;
DELETE FROM Categories;
```

Sau đó restart app — seed chèn lại dữ liệu mẫu.

### Lỗi thường gặp

| Hiện tượng | Nguyên nhân | Xử lý |
|------------|-------------|--------|
| Redirect `/Account/Login?ReturnUrl=/Admin/...` | Chưa đăng nhập | Login bằng `admin@bookstore.com` |
| `/Account/AccessDenied` | Đã login nhưng không có role Admin | Dùng tài khoản seed |
| 403 sau khi gán role | Cookie cũ | Logout → Login lại |
| Trang chủ: "Chưa có sách nào" | Seed chưa chạy hoặc DB cũ | Kiểm tra `Development`; xóa Categories/Books rồi restart |
| `IDENTITY_INSERT is OFF` | Gán `Id` thủ công khi seed runtime | Bỏ `Id` trong seed; lưu Category trước, dùng `fiction.Id` cho Book |

---

## Luồng đặt hàng (Checkout)

```
Trang chủ → Thêm vào giỏ
    → CartService: validate stock (cộng dồn nếu sách đã có trong giỏ)
    → CartSessionService: lưu session
    → /Cart
    → /Order/Checkout → PlaceOrder
    → OrderService: validate stock lại, transaction, trừ Stock
    → Clear cart → /Order/Success/{id}
```

### Quy tắc nghiệp vụ

- `OrderDetail.Price` = snapshot giá `Book` tại thời điểm đặt — không đổi khi admin sửa giá sau này
- `TotalAmount` = tổng `Price × Quantity` của các `OrderDetail`
- **Stock — phòng thủ 2 lớp:**
  - Lớp 1 (giỏ): `CartService` chặn add/update vượt `Stock`; hiển thị lỗi qua `TempData["error"]`
  - Lớp 2 (checkout): `OrderService` validate lại trước khi trừ tồn kho (xử lý race condition khi stock thay đổi giữa các bước)
- Catalog client chỉ hiện sách `Stock > 0`
- Giỏ rỗng → redirect về `/Cart`

### Trạng thái đơn hàng

| Status | Ý nghĩa |
|--------|---------|
| `Pending` | Vừa đặt (mặc định) |
| `Processing` | Đang xử lý |
| `Shipped` | Đã gửi hàng |
| `Completed` | Hoàn tất — **tính vào doanh thu Dashboard** |
| `Cancelled` | Đã hủy |

Hằng số: `Models/OrderStatuses.cs`. Admin cập nhật tại `/Admin/Order/Detail/{id}`.

---

## Service Layer

| Interface | Implementation | Trách nhiệm | Dùng bởi |
|-----------|------------------|-------------|----------|
| `ICartSessionService` | `CartSessionService` | Đọc/ghi giỏ hàng trong Session (JSON) | `CartController`, `OrderController`, `CartService` |
| `ICartService` | `CartService` | Validate stock khi add/update giỏ | `CartController` |
| `ICategoryService` | `CategoryService` | CRUD category + xóa an toàn | Admin `CategoryController` |
| `IBookService` | `BookService` | CRUD book, catalog (filter + pagination), Admin search, detail VM | Admin `BookController`, client `HomeController` / `BookController`, `CartService` |
| `IOrderService` | `OrderService` | Checkout, validate stock, quản lý đơn | `OrderController`, Admin `OrderController` |
| `IDashboardService` | `DashboardService` | Thống kê dashboard | Admin `HomeController` |

Đăng ký DI trong `Program.cs`:

```csharp
builder.Services.AddScoped<ICartSessionService, CartSessionService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
```

---

## Data model

| Entity | Mô tả |
|--------|--------|
| `ApplicationUser` | User Identity |
| `Category` | Danh mục sách |
| `Book` | Sách (FK → Category) |
| `Order` | Đơn hàng (FK → User) |
| `OrderDetail` | Chi tiết đơn (FK → Order, Book) |

**Quan hệ:** Category 1–n Book · User 1–n Order · Order 1–n OrderDetail · Book 1–n OrderDetail

---

## Tình trạng theo `plan.md`

**Milestone lõi:** đạt đủ theo PRD (CRUD Admin, Cart/Order, quản lý đơn, Auth + Role, Service layer, Dashboard, storefront client).

| Phase | Trạng thái |
|-------|------------|
| Phase 1 — Setup & Foundation | Done |
| Phase 2 — Core Business | Done (Admin CRUD + client catalog) |
| Phase 3 — Cart & Order | Done |
| Phase 4 — Authorization & Area Admin | Done |
| Phase 5 — Polish | Done (Dashboard, Validation Admin, Service layer, Seed data) |

**Phase A (Critical Fix) — một phần:**

| Task | Mô tả | Trạng thái |
|------|--------|------------|
| A3 | Pagination catalog khớp filter `Stock > 0` | Done |
| A4 | Xóa Category/Book an toàn (`DeleteResult`) | Done |
| A5 | Validate stock khi add/update giỏ (`ICartService`) | Done |

**Client (plan Task 10–12):**

| Task | Mô tả | Trạng thái |
|------|--------|------------|
| 10 | Book listing + pagination | Done |
| 11 | Book detail (`/Book/Details/{id}`) | Done |
| 12 | Search & filter storefront (Title, Category, price range + pagination) | Done |

### Optional (chưa làm)

| Task | Mô tả |
|------|--------|
| 16 | Logging khi tạo order |
| 18 | Unit test service |
| — | README screenshot / deploy |

---

## Manual test nhanh

### Luồng khách hàng

1. Mở `/` — thấy **4 sách** seed (Fiction, Science, Technology)
2. **Search/filter:** Title + Category + khoảng giá; kiểm tra URL có query string; đổi trang vẫn giữ filter; **Xóa lọc** về `/`
3. Mở `/Book/Details/{id}` — thông tin đầy đủ + sách cùng category (nếu có)
4. **Thêm vào giỏ** → `/Cart` (alert xanh nếu thành công)
5. Thử **add/update vượt stock** → alert đỏ, giỏ không đổi
6. Đăng ký hoặc login → `/Order/Checkout` → **Place Order**
7. Kiểm tra `/Order/Success/{id}` và DB: `Orders`, `OrderDetails`, `Books.Stock` giảm

### Luồng admin

1. Login `admin@bookstore.com` / `Admin@123` → vào `/Admin/Home`
2. CRUD Category
3. `/Admin/Book` — search/filter + Details; Create/Edit/Delete (thử upload ảnh)
4. `/Admin/Order` — đổi status sang `Completed` → F5 Dashboard → doanh thu tăng

### Phân quyền

- User thường vào `/Admin/Book` → `/Account/AccessDenied`
- Navbar chỉ hiện link **Admin** khi `User.IsInRole("Admin")`

---

## Validation

### Admin (form)

Data Annotations + jQuery Unobtrusive Validation trên form Create/Edit **Category** và **Book** (`BookFormVM`).

| Model | Quy tắc chính |
|-------|----------------|
| `Category` | Name: Required, StringLength(2–100) |
| `Book` / `BookFormVM` | Title, Author: Required; Price > 0; Stock ≥ 0; CategoryId: Range(1, int.MaxValue) |

`CategoryId` dùng `[Range(1, int.MaxValue)]` thay vì `[Required]` vì `int` mặc định = `0`.

### Giỏ hàng (server-side)

| Thao tác | Service | Quy tắc |
|----------|---------|---------|
| Add | `CartService.TryAddAsync` | `existingQty + quantity ≤ Stock`; chặn nếu `Stock = 0` |
| Update | `CartService.TryUpdateQuantityAsync` | `quantity ≤ Stock`; `quantity ≤ 0` → xóa item |
| Checkout | `OrderService.PlaceOrderAsync` | Validate stock lại trước transaction |

Lỗi giỏ hàng hiển thị qua `TempData["error"]` trên `/Cart`.

### Admin delete (server-side)

| Entity | Quy tắc |
|--------|---------|
| Category | Không xóa nếu còn sách thuộc category |
| Book | Không xóa nếu sách đã có trong `OrderDetail` |

Kết quả trả về `DeleteResult`; controller hiển thị qua `TempData["error"]`.

---

## Ghi chú kỹ thuật

- **Admin Book:** một `BookController` cho list + search/filter + Details + CRUD (đã gộp, không còn `BooksController` / `/Admin/Books`)
- Storefront `BookController` (`/Book/Details/{id}`) tách Area — không xung đột với Admin
- Không query trong View; filter/search dùng `IQueryable` trên EF (`AsNoTracking`, `Include` khi cần)
- **Tách cart layer:** `CartSessionService` = persistence; `CartService` = business rules (stock)
- Checkout yêu cầu `[Authorize]` trên `OrderController`
- `PlaceOrder` dùng database transaction khi trừ stock và lưu order
- Upload ảnh: `IWebHostEnvironment` inject vào `BookService`, lưu tại `wwwroot/images/books`
- Catalog client (`GetCatalogAsync`): `Stock > 0` + optional Title/Category/price range trên `IQueryable` → `CountAsync` → `Skip`/`Take` (không load all vào memory); state filter nằm trong `BookCatalogVM`
- Runtime seed: idempotent, không dùng `HasData()`; Category lưu trước Book (FK); chỉ Development

---

## Tài liệu tham khảo

- [ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/mvc/overview)
- [EF Core](https://learn.microsoft.com/ef/core/)
- [ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity)
- [Authorization (Roles)](https://learn.microsoft.com/aspnet/core/security/authorization/roles)
