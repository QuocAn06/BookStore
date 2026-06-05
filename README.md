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
| Giỏ hàng | Session + JSON (`ICartSessionService`) |
| Kiến trúc | MVC + Service Layer (`/Services`) |
| UI | Bootstrap 5, Razor Views |

---

## Tính năng chính

### Client (người mua)

- Trang chủ: danh sách sách, phân trang, nút **Thêm vào giỏ**
- Giỏ hàng: thêm / cập nhật số lượng / xóa (lưu Session)
- Checkout → tạo `Order` + `OrderDetail` (copy giá từ `Book`, tính `TotalAmount`, trừ tồn kho)
- Đăng ký / đăng nhập / đăng xuất

### Admin (`/Areas/Admin`)

- Dashboard: tổng đơn hàng, tổng doanh thu (chỉ đơn `Completed`), danh sách sách phân trang
- CRUD Category
- CRUD Book + upload ảnh (`wwwroot/images/books`)
- Duyệt sách: search theo title, filter theo category (`/Admin/Books`)
- Quản lý đơn hàng: list, detail, cập nhật trạng thái

---

## Cấu trúc thư mục

```
BookStore/
├── Areas/Admin/              # Quản trị (yêu cầu role Admin)
│   ├── Controllers/          # Category, Book, Books, Order, Home
│   └── Views/
├── Controllers/              # Client: Home, Account, Cart, Order
├── Data/                     # ApplicationDbContext, IdentitySeed
├── Infrastructure/           # SessionKeys
├── Models/                   # Entity, Roles, OrderStatuses, ViewModels
├── Services/                 # Business logic layer
│   ├── ICartSessionService / CartSessionService
│   ├── ICategoryService / CategoryService
│   ├── IBookService / BookService
│   ├── IOrderService / OrderService
│   ├── IDashboardService / DashboardService
│   └── PlaceOrderResult
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

2. Áp dụng migration:

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
| `/` | Trang chủ — catalog sách | Public |
| `/Account/Register` | Đăng ký | |
| `/Account/Login` | Đăng nhập | Admin login → redirect `/Admin/Home` |
| `/Account/AccessDenied` | Không đủ quyền | |
| `/Cart` | Giỏ hàng | Session |
| `/Order/Checkout` | Thanh toán | Cần đăng nhập |
| `/Order/Success/{id}` | Xác nhận đơn hàng | Cần đăng nhập |
| `/Admin/Home` | Dashboard admin | Role **Admin** |
| `/Admin/Category` | CRUD danh mục | Role **Admin** |
| `/Admin/Book` | CRUD sách + upload ảnh | Role **Admin** |
| `/Admin/Books` | Duyệt sách, search/filter | Role **Admin** |
| `/Admin/Order` | Quản lý đơn hàng | Role **Admin** |

Toàn bộ controller trong `Areas/Admin` kế thừa `AdminControllerBase` với `[Authorize(Roles = Roles.Admin)]`.

---

## Tài khoản Admin

Khi app khởi động, `Data/IdentitySeed.cs` tự động (idempotent):

1. Tạo role **Admin** nếu chưa có
2. Tạo user admin nếu chưa có
3. Gán user vào role Admin

| Trường | Giá trị mặc định (dev) |
|--------|-------------------------|
| Email | `admin@bookstore.com` |
| Mật khẩu | `Admin@123` |

User đăng ký qua `/Account/Register` **không** có role Admin.

### Kiểm tra role trong database

```sql
SELECT u.Email, r.Name AS RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id;
```

### Lỗi thường gặp

| Hiện tượng | Nguyên nhân | Xử lý |
|------------|-------------|--------|
| Redirect `/Account/Login?ReturnUrl=/Admin/...` | Chưa đăng nhập | Login bằng `admin@bookstore.com` |
| `/Account/AccessDenied` | Đã login nhưng không có role Admin | Dùng tài khoản seed |
| 403 sau khi gán role | Cookie cũ | Logout → Login lại |

---

## Luồng đặt hàng (Checkout)

```
Trang chủ → Thêm vào giỏ → /Cart → /Order/Checkout → PlaceOrder
    → OrderService: validate stock, transaction, trừ Stock
    → Clear cart → /Order/Success/{id}
```

### Quy tắc nghiệp vụ

- `OrderDetail.Price` = snapshot giá `Book` tại thời điểm đặt — không đổi khi admin sửa giá sau này
- `TotalAmount` = tổng `Price × Quantity` của các `OrderDetail`
- Không cho đặt vượt quá `Stock`
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

| Interface | Implementation | Dùng bởi |
|-----------|------------------|----------|
| `ICartSessionService` | `CartSessionService` | `CartController`, `OrderController` |
| `ICategoryService` | `CategoryService` | Admin `CategoryController` |
| `IBookService` | `BookService` | Admin `BookController`, `BooksController`, `CartController`, `HomeController` |
| `IOrderService` | `OrderService` | `OrderController`, Admin `OrderController` |
| `IDashboardService` | `DashboardService` | Admin `HomeController` |

Đăng ký DI trong `Program.cs`:

```csharp
builder.Services.AddScoped<ICartSessionService, CartSessionService>();
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
| Phase 5 — Polish | Done (Dashboard, Validation Admin, Service layer) |

### Optional (chưa làm)

| Task | Mô tả |
|------|--------|
| 16 | Logging khi tạo order |
| 17 | Seed Category + Book mẫu (hiện chỉ seed admin user) |
| 18 | Unit test service |
| — | Client: trang chi tiết sách, search/filter trên storefront |
| — | README screenshot / deploy |

---

## Manual test nhanh

### Luồng khách hàng

1. Mở `/` — thấy danh sách sách
2. **Thêm vào giỏ** → `/Cart`
3. Đăng ký hoặc login → `/Order/Checkout` → **Place Order**
4. Kiểm tra `/Order/Success/{id}` và DB: `Orders`, `OrderDetails`, `Books.Stock` giảm

### Luồng admin

1. Login `admin@bookstore.com` / `Admin@123` → vào `/Admin/Home`
2. CRUD Category, Book (thử upload ảnh)
3. `/Admin/Books` — search + filter
4. `/Admin/Order` — đổi status sang `Completed` → F5 Dashboard → doanh thu tăng

### Phân quyền

- User thường vào `/Admin/Book` → `/Account/AccessDenied`
- Navbar chỉ hiện link **Admin** khi `User.IsInRole("Admin")`

---

## Validation (Admin)

Data Annotations + jQuery Unobtrusive Validation trên form Create/Edit **Category** và **Book** (`BookFormVM`).

| Model | Quy tắc chính |
|-------|----------------|
| `Category` | Name: Required, StringLength(2–100) |
| `Book` / `BookFormVM` | Title, Author: Required; Price > 0; Stock ≥ 0; CategoryId: Range(1, int.MaxValue) |

`CategoryId` dùng `[Range(1, int.MaxValue)]` thay vì `[Required]` vì `int` mặc định = `0`.

---

## Ghi chú kỹ thuật

- Không query trong View; filter/search dùng `IQueryable` trên EF (`AsNoTracking`, `Include` khi cần)
- Checkout yêu cầu `[Authorize]` trên `OrderController`
- `PlaceOrder` dùng database transaction khi trừ stock và lưu order
- Upload ảnh: `IWebHostEnvironment` inject vào `BookService`, lưu tại `wwwroot/images/books`
- Catalog client (`GetCatalogAsync`) chỉ hiện sách `Stock > 0`

---

## Tài liệu tham khảo

- [ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/mvc/overview)
- [EF Core](https://learn.microsoft.com/ef/core/)
- [ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity)
- [Authorization (Roles)](https://learn.microsoft.com/aspnet/core/security/authorization/roles)
