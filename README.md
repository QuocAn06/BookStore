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
├── Data/                     # ApplicationDbContext, IdentitySeed
├── Models/                   # Entity, Roles, OrderStatuses, ViewModels
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
| `/Admin/Home` | Dashboard admin (tổng đơn, doanh thu) + danh sách sách phân trang | Cần role **Admin** |
| `/Admin/Category` | CRUD danh mục | Cần role **Admin** |
| `/Admin/Book` | CRUD sách + upload ảnh | Cần role **Admin** |
| `/Admin/Books` | Duyệt sách, search/filter | Cần role **Admin** |
| `/Admin/Order` | Danh sách & chi tiết đơn hàng | Cần role **Admin** |

Toàn bộ controller trong `Areas/Admin` kế thừa `AdminControllerBase` với `[Authorize(Roles = Roles.Admin)]`.

---

## Tài khoản Admin (role `Admin`)

Khi app khởi động, `Data/IdentitySeed.cs` tự động (idempotent):

1. Tạo role **Admin** nếu chưa có.
2. Tạo user admin nếu chưa có.
3. Gán user vào role Admin.

| Trường | Giá trị mặc định (dev) |
|--------|-------------------------|
| Email | `admin@bookstore.com` |
| Mật khẩu | `Admin@123` |

**Đăng nhập:** `/Account/Login` → mở bất kỳ URL `/Admin/...`.

User đăng ký qua `/Account/Register` **không** có role Admin và **không** truy cập được Area Admin.

### Kiểm tra trong database

```sql
SELECT u.Email, r.Name AS RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id;
```

### Gán role thủ công (tùy chọn)

Nếu muốn nâng user đã đăng ký thành admin: đổi `AdminEmail` trong `IdentitySeed.cs` thành email đó rồi chạy lại app, hoặc chèn qua SQL vào `AspNetUserRoles` (xem migration Identity).

### Lỗi thường gặp

| Hiện tượng | Nguyên nhân | Xử lý |
|------------|-------------|--------|
| Redirect `/Account/Login?ReturnUrl=/Admin/...` | Chưa đăng nhập | Login bằng `admin@bookstore.com` |
| Redirect `/Account/AccessDenied?ReturnUrl=/Admin/...` | Đã login nhưng **không** có role Admin | Dùng tài khoản seed hoặc gán role |
| 404 trên `/Account/AccessDenied` | Chưa có action/view AccessDenied | Thêm `AccountController.AccessDenied` + view (tùy chọn) |
| 403 / AccessDenied dù đã gán role | Chưa logout/login sau khi gán role | Logout → Login lại |

**Gợi ý test:** User khách (checkout) và user admin (quản lý) — dùng cửa sổ thường / Incognito để tránh nhầm cookie.

**Cookie:** `Program.cs` cấu hình `ConfigureApplicationCookie` (`LoginPath`, `AccessDeniedPath`).

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

**Dashboard — tổng doanh thu:** chỉ cộng đơn có `Status == Completed` (đơn `Pending` / `Cancelled` không tính). **Tổng đơn:** đếm mọi đơn trong bảng `Orders`.

---

## Tình trạng feature (cập nhật theo `plan.md`)

**Tiến độ tổng:** **11 / 15** task chính hoàn thành đủ Definition of Done (~**73%**).  
**Milestone “hoàn chỉnh” trong plan:** đạt **4 / 4** phần lõi (CRUD Admin, Cart/Order, quản lý đơn, role Admin); còn **storefront client** (Task 6–7) và polish (Phase 5 — Task 15 service layer).

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

### Phase 4 — Authorization & Structure (2/2)

| Task | Mô tả | Trạng thái |
|------|--------|------------|
| 11 | Area Admin (`/Admin/...`) | Done |
| 12 | Role Admin + seed + chặn toàn Area Admin | Done (`Roles.cs`, `IdentitySeed`, `AdminControllerBase`) |

### Phase 5 — Polish (1/3 hoàn chỉnh)

| Task | Mô tả | Trạng thái |
|------|--------|------------|
| 13 | Dashboard (tổng đơn, doanh thu) | Done |
| 14 | Validation + UX | **Done (Admin)** — DataAnnotations + hiển thị lỗi trên form Create/Edit Book & Category; client storefront chưa có (Task 6–7) |
| 15 | Service layer (tách logic khỏi controller) | Một phần (chỉ `CartSessionService`) |

### Optional

| Task | Trạng thái |
|------|------------|
| 16 — Logging khi tạo order | Chưa |
| 17 — Seed Category + Book / Admin user | Một phần (seed admin user + role qua `IdentitySeed`; chưa seed sách/danh mục) |
| 18 — Unit test | Chưa |

---

## Đã triển khai (highlights)

- **Identity:** đăng ký, đăng nhập, đăng xuất (`AccountController`, `_LoginPartial`).
- **Admin:** CRUD Category; CRUD Book kèm upload ảnh vào `wwwroot/images/books`.
- **Validation (Admin):** Data Annotations trên `Category`, `Book`, `BookFormVM`; kiểm tra `ModelState.IsValid` trong controller; hiển thị lỗi qua `asp-validation-for` + `asp-validation-summary="All"` + `_ValidationScriptsPartial` (client-side).
- **Admin browse:** `Admin/Books` — tìm theo title, lọc category (LINQ `IQueryable` trên DB).
- **Giỏ hàng:** session JSON qua `ICartSessionService`.
- **Đặt hàng (client):** `OrderController` — `[Authorize]`; lưu Order/OrderDetail; `OrderStatuses.Pending`; copy `Price` từ Book; `TotalAmount`; trừ tồn kho trong transaction.
- **Quản lý đơn (admin):** `Admin/Order` — list (kèm email user), detail (line items + snapshot giá), `UpdateStatus` với whitelist `OrderStatuses.All`.
- **Dashboard (admin):** `Admin/Home` — card **Tổng đơn hàng** (`CountAsync` trên `Orders`), **Tổng doanh thu** (`SumAsync` trên `TotalAmount`, filter `Completed`); ViewModel `AdminHomeBooksVM`; query trong `Areas/Admin/Controllers/HomeController`.
- **Phân quyền:** `Models/Roles.cs`; seed role/user qua `IdentitySeed` khi startup; `AdminControllerBase` bảo vệ mọi controller trong `Areas/Admin`.

---

## Validation — Book & Category (Admin)

Validation dùng **Data Annotations** (server-side) và **jQuery Unobtrusive Validation** (client-side).

### Model / ViewModel

| File | Quy tắc chính |
|------|----------------|
| `Models/Category.cs` | `Name`: Required, StringLength(2–100); `Description`: StringLength(500) |
| `Models/Book.cs` | `Title`, `Author`: Required + StringLength; `Price`: Range > 0; `Stock`: Range ≥ 0; `CategoryId`: Range(1, int.MaxValue) |
| `Models/ViewModels/BookFormVM.cs` | Cùng quy tắc form Book (form bind vào VM, **không** bind trực tiếp entity `Book`) |

**Lưu ý:** `CategoryId` dùng `[Range(1, int.MaxValue)]` thay vì `[Required]` vì `int` mặc định = `0` khi dropdown chọn `-- Select category --`.

### Controller

- `CategoryController` — Create/Edit POST: `if (!ModelState.IsValid) return View(category);`
- `BookController` — Create/Edit POST: kiểm tra `ModelState`, reload `CategoryList` trước khi `return View(vm)`

### View

Mỗi field: `<label asp-for>` + `<input/select asp-for>` + `<span asp-validation-for class="text-danger">`.  
Đầu form: `<div asp-validation-summary="All">`.  
Cuối view: `@section Scripts { <partial name="_ValidationScriptsPartial" /> }`

Form áp dụng: `Areas/Admin/Views/Category/Create.cshtml`, `Edit.cshtml`, `Book/Create.cshtml`, `Edit.cshtml`.

### Manual test — Validation

1. `/Admin/Category/Create` — để trống **Name** → lỗi đỏ, không redirect.
2. `/Admin/Category/Create` — **Name** = `A` → lỗi minimum length (2 ký tự).
3. `/Admin/Book/Create` — **Price** = `0` → lỗi Price.
4. `/Admin/Book/Create` — không chọn Category → `"Category is required."`
5. Submit form invalid → trang giữ nguyên, lỗi hiện dưới field và ở đầu form.

---

## Manual test — Dashboard (Task 13)

1. `dotnet run` → đăng nhập `admin@bookstore.com` / `Admin@123`.
2. Mở `/Admin/Home` — kiểm tra hai card và bảng sách phía dưới.
3. **Tổng đơn:** tạo đơn qua `/Cart` → `/Order/Checkout` (user đã login) → F5 dashboard → số đơn tăng.
4. **Tổng doanh thu:** đơn mới mặc định `Pending` (doanh thu chưa tăng) → `/Admin/Order/Detail/{id}` → đổi **Completed** → F5 → doanh thu cộng đúng `TotalAmount`.
5. (Tùy chọn) SQL: `SELECT COUNT(*) FROM Orders`; `SELECT SUM(TotalAmount) FROM Orders WHERE Status = N'Completed'`.

User không có role Admin không vào được `/Admin/Home` (403 / Access Denied).

---

## Việc cần làm tiếp (ưu tiên mentor)

1. **Storefront client (Task 6–7):** trang chủ danh sách sách, chi tiết, pagination, search/filter; nút “Thêm vào giỏ” từ catalog.
2. **Polish (Task 15):** mở rộng service layer; validation client catalog khi làm Task 6–7.
3. **Dashboard (tùy chọn PRD):** card **Tổng sách** trên `/Admin/Home` (dùng sẵn `TotalCount` trong `AdminHomeBooksVM`).
4. **Tùy chọn UX auth:** `Account/AccessDenied` + view; hiện link Admin trên `_Layout` chỉ khi `User.IsInRole(Roles.Admin)`.
5. **Demo:** screenshot luồng checkout → admin đổi status → dashboard cập nhật.

---

## Quy ước & ghi chú kỹ thuật

- Logic nghiệp vụ nên dần chuyển sang `/Services` (theo `plan.md`); hiện Order/Book/Category vẫn nằm trong controller.
- Không query trong View; filter/search dùng `IQueryable` trên EF (đã áp dụng ở `Admin/Books`).
- Checkout cần user đã login (`[Authorize]` trên `OrderController` client).
- Area Admin cần role **Admin** (`AdminControllerBase`); seed chạy trong `Program.cs` sau `builder.Build()`.
- Giá trên `OrderDetail.Price` là snapshot — không đổi khi admin sửa giá sách sau này.
- Dashboard query dùng `AsNoTracking()`; doanh thu filter `OrderStatuses.Completed` — đổi quy tắc (ví dụ loại trừ `Cancelled`) cần sửa `HomeController` và cập nhật mục test/README cho nhất quán.
- Form Book bind `BookFormVM` — annotations validation phải đặt trên VM; entity `Book` dùng khi map sang DB.
- `CategoryId` trên dropdown: dùng `[Range(1, int.MaxValue)]`, không dùng `[Required]` trên kiểu `int`.

---

## Tài liệu tham khảo

- [ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/mvc/overview)
- [EF Core](https://learn.microsoft.com/ef/core/)
- [ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity)
- [Authorization trong MVC](https://learn.microsoft.com/aspnet/core/security/authorization/roles)
