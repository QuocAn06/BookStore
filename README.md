# BookStore

Ứng dụng web bán sách xây dựng với **ASP.NET Core MVC (.NET 6)**, **Entity Framework Core**, và **ASP.NET Core Identity**. Dự án dùng cho học tập / portfolio fresher .NET.

- Kế hoạch phát triển: [`plan.md`](plan.md)
- Yêu cầu sản phẩm: [`PRD.md`](PRD.md)

---

## Tech stack

| Thành phần | Công nghệ |
|------------|-----------|
| Framework | ASP.NET Core MVC 6.0 |
| ORM | Entity Framework Core 6 (SQL Server) |
| Authentication | ASP.NET Core Identity (`ApplicationUser`) |
| Giỏ hàng | Session + JSON (`CartSessionService`) |
| UI | Bootstrap 5, Razor Views |

---

## Cấu trúc thư mục chính

```
BookStore/
├── Areas/Admin/              # Quản trị: Category, Book, Books, Order
│   ├── Controllers/
│   └── Views/
├── Controllers/              # Client: Home, Account, Cart, Order
├── Data/                     # ApplicationDbContext
├── Models/                   # Entity, OrderStatuses, ViewModels
├── Services/                 # ICartSessionService, CartSessionService
├── Views/                    # Razor views (client)
└── Migrations/               # EF Core migrations
```

---

## Chạy project

### Yêu cầu

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- SQL Server (LocalDB hoặc instance đầy đủ)

### Cấu hình database

1. Sửa connection string trong `appsettings.json` (mặc định LocalDB: `BookStoreDb`).
2. Áp dụng migration:

```bash
dotnet ef database update
```

### Chạy ứng dụng

```bash
dotnet run
```

URL mặc định (xem `Properties/launchSettings.json`):

| Môi trường | HTTPS |
|------------|--------|
| `dotnet run` | `https://localhost:7199` |

---

## Route quan trọng

| URL | Mô tả | Ghi chú |
|-----|--------|---------|
| `/Account/Register` | Đăng ký | |
| `/Account/Login` | Đăng nhập | |
| `/Cart` | Giỏ hàng | Form test “Add book” (dev) |
| `/Order/Checkout` | Thanh toán | Cần đăng nhập |
| `/Admin/Category` | CRUD danh mục | |
| `/Admin/Book` | CRUD sách + upload ảnh | |
| `/Admin/Books` | Duyệt sách, search/filter | |
| `/Admin/Order` | Danh sách đơn hàng | `[Authorize(Roles = "Admin")]` |

---

## Tài khoản Admin (role `Admin`)

`Areas/Admin/Controllers/OrderController` yêu cầu role **Admin**. User đăng ký thường **không** có role này.

### Cách nhanh: Register + SQL

1. Đăng ký tại `/Account/Register` (ví dụ `admin@bookstore.local` / `Admin@123`).
2. Trong SSMS / Azure Data Studio, chạy trên database `BookStoreDb`:

```sql
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = N'Admin')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), N'Admin', N'ADMIN', NEWID());

DECLARE @UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = N'admin@bookstore.local');
DECLARE @RoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = N'Admin');

IF @UserId IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId
)
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);
```

3. **Logout** → **Login** lại bằng user admin.
4. Mở `/Admin/Order`.

**Kiểm tra:**

```sql
SELECT u.Email, r.Name AS RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id;
```

### Lỗi thường gặp

| Hiện tượng | Nguyên nhân | Xử lý |
|------------|-------------|--------|
| Redirect `/Account/AccessDenied?ReturnUrl=/Admin/Order` | Đã login nhưng **không** có role Admin | Gán role SQL + login lại |
| 404 trên `/Account/AccessDenied` | Chưa có action/view AccessDenied | Thêm `AccountController.AccessDenied` + view (tùy chọn) |
| 403 / AccessDenied dù SQL đã gán role | Chưa logout/login sau khi gán role | Logout → Login lại |

**Gợi ý test:** Dùng user khách (checkout) và user admin (quản lý đơn) — tách trình duyệt thường / Incognito để tránh nhầm cookie.

---

## Luồng trạng thái đơn hàng

| Status | Ý nghĩa |
|--------|---------|
| `Pending` | Vừa đặt (mặc định khi checkout) |
| `Processing` | Admin đang xử lý |
| `Shipped` | Đã gửi hàng |
| `Completed` | Hoàn tất |
| `Cancelled` | Đã hủy |

Hằng số: `Models/OrderStatuses.cs`. Admin cập nhật tại `/Admin/Order/Detail/{id}`.

---

## Tình trạng feature (cập nhật theo `plan.md`)

**Tiến độ tổng:** **9 / 15** task chính hoàn thành đủ Definition of Done (~**60%**).  
**Milestone “hoàn chỉnh” trong plan:** đạt **3 / 4** (CRUD Book/Category + Cart/Order + Admin quản lý đơn; còn storefront client và role Admin đồng bộ toàn Area).

### Phase 1 — Setup & Foundation (3/3)

| Task | Mô tả | Trạng thái |
|------|--------|------------|
| 1 | Init MVC + EF Core | Done |
| 2 | Identity Login / Register | Done |
| 3 | DbContext + Migration (Book, Category, Order, OrderDetail) | Done |

### Phase 2 — Core Business (2/4)

| Task | Mô tả | Trạng thái |
|------|--------|------------|
| 4 | Category CRUD (Admin) | Done |
| 5 | Book CRUD + upload ảnh (Admin) | Done |
| 6 | Client — danh sách & chi tiết sách (pagination) | Chưa |
| 7 | Search & filter (client, query trên DB) | Chưa (có filter tại `Admin/Books`, chưa phải storefront khách) |

### Phase 3 — Cart & Order (3/3)

| Task | Mô tả | Trạng thái |
|------|--------|------------|
| 8 | Giỏ hàng Session (add / remove / update) | Done |
| 9 | Checkout → Order + OrderDetail (copy giá, TotalAmount) | Done |
| 10 | Quản lý đơn Admin (list, detail, cập nhật status) | Done |

### Phase 4 — Authorization & Structure (1/2)

| Task | Mô tả | Trạng thái |
|------|--------|------------|
| 11 | Area Admin (`/Admin/...`) | Done |
| 12 | Role Admin + chặn route Admin | Một phần (`[Authorize(Roles = "Admin")]` trên `OrderController`; chưa seed tự động; các controller Admin khác chưa gắn role) |

### Phase 5 — Polish (0/3 hoàn chỉnh)

| Task | Mô tả | Trạng thái |
|------|--------|------------|
| 13 | Dashboard (tổng đơn, doanh thu) | Chưa |
| 14 | Validation + UX | Một phần (DataAnnotations trên model; admin form OK; client UX còn sơ) |
| 15 | Service layer (tách logic khỏi controller) | Một phần (chỉ `CartSessionService`) |

### Optional

| Task | Trạng thái |
|------|------------|
| 16 — Logging khi tạo order | Chưa |
| 17 — Seed Category + Book / Admin user | Chưa |
| 18 — Unit test | Chưa |

---

## Đã triển khai (highlights)

- **Identity:** đăng ký, đăng nhập, đăng xuất (`AccountController`, `_LoginPartial`).
- **Admin:** CRUD Category; CRUD Book kèm upload ảnh vào `wwwroot/images/books`.
- **Admin browse:** `Admin/Books` — tìm theo title, lọc category (LINQ `IQueryable` trên DB).
- **Giỏ hàng:** session JSON qua `ICartSessionService`.
- **Đặt hàng (client):** `OrderController` — `[Authorize]`; lưu Order/OrderDetail; `OrderStatuses.Pending`; copy `Price` từ Book; `TotalAmount`; trừ tồn kho trong transaction.
- **Quản lý đơn (admin):** `Admin/Order` — list (kèm email user), detail (line items + snapshot giá), `UpdateStatus` với whitelist `OrderStatuses.All`.

---

## Việc cần làm tiếp (ưu tiên mentor)

1. **Task 12 hoàn chỉnh:** `DbInitializer` seed role + user admin; `[Authorize(Roles = "Admin")]` cho toàn bộ controller trong `Areas/Admin`; trang `Account/AccessDenied`.
2. **Storefront client (Task 6–7):** trang chủ danh sách sách, chi tiết, pagination, search/filter; nút “Thêm vào giỏ” từ catalog.
3. **Polish (Task 13–15):** dashboard, mở rộng service layer, hoàn thiện UX/validation phía client.
4. **README / demo:** screenshot luồng checkout → admin đổi status.

---

## Quy ước & ghi chú kỹ thuật

- Logic nghiệp vụ nên dần chuyển sang `/Services` (theo `plan.md`); hiện Order/Book/Category vẫn nằm trong controller.
- Không query trong View; filter/search dùng `IQueryable` trên EF (đã áp dụng ở `Admin/Books`).
- Checkout cần user đã login (`[Authorize]` trên `OrderController` client).
- Giá trên `OrderDetail.Price` là snapshot — không đổi khi admin sửa giá sách sau này.

---

## Tài liệu tham khảo

- [ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/mvc/overview)
- [EF Core](https://learn.microsoft.com/ef/core/)
- [ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity)
- [Authorization trong MVC](https://learn.microsoft.com/aspnet/core/security/authorization/roles)
