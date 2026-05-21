# BookStore

Ứng dụng web bán sách xây dựng với **ASP.NET Core MVC (.NET 6)**, **Entity Framework Core**, và **ASP.NET Core Identity**. Dự án dùng cho học tập / portfolio fresher .NET.

Chi tiết kế hoạch phát triển: [`plan.md`](plan.md).

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
├── Areas/Admin/          # Quản trị (Category, Book, Books browse)
├── Controllers/          # Client: Home, Account, Cart, Order
├── Data/                 # ApplicationDbContext
├── Models/               # Entity + ViewModels
├── Services/             # ICartSessionService, CartSessionService
├── Views/                # Razor views (client)
└── Migrations/           # EF Core migrations
```

---

## Chạy project

### Yêu cầu

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- SQL Server (LocalDB hoặc instance đầy đủ)

### Cấu hình database

1. Sửa connection string trong `appsettings.json` (hoặc `appsettings.Development.json`).
2. Áp dụng migration:

```bash
dotnet ef database update
```

### Chạy ứng dụng

```bash
dotnet run
```

Mở trình duyệt theo URL trong console (thường `https://localhost:7xxx`).

---

## Tình trạng feature (cập nhật theo `plan.md`)

**Tiến độ tổng:** **8 / 15** task chính hoàn thành đủ Definition of Done (~**53%**).  
**Milestone “hoàn chỉnh” trong plan:** đạt **2 / 4** (CRUD Book/Category + Cart/Order flow; thiếu Admin quản lý order và Role Admin).

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
| 7 | Search & filter (client, query trên DB) | Chưa (có filter tương tự tại `Admin/Books`, chưa phải storefront khách) |

### Phase 3 — Cart & Order (2/3)

| Task | Mô tả | Trạng thái |
|------|--------|------------|
| 8 | Giỏ hàng Session (add / remove / update) | Done |
| 9 | Checkout → Order + OrderDetail (copy giá, TotalAmount) | Done |
| 10 | Quản lý đơn hàng Admin (list, cập nhật status) | Chưa |

### Phase 4 — Authorization & Structure (1/2)

| Task | Mô tả | Trạng thái |
|------|--------|------------|
| 11 | Area Admin (`/Admin/...`) | Done |
| 12 | Role Admin + chặn route Admin | Chưa |

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
| 17 — Seed Category + Book | Chưa |
| 18 — Unit test | Chưa |

---

## Đã triển khai (highlights)

- **Identity:** đăng ký, đăng nhập, đăng xuất (`AccountController`, `_LoginPartial`).
- **Admin:** CRUD Category; CRUD Book kèm upload ảnh vào `wwwroot/images/books`.
- **Admin browse:** `Admin/Books` — tìm theo title, lọc category (LINQ `IQueryable` trên DB).
- **Giỏ hàng:** session JSON qua `ICartSessionService`.
- **Đặt hàng:** `OrderController` — yêu cầu đăng nhập; lưu Order/OrderDetail; copy `Price` từ Book; tính `TotalAmount`; trừ tồn kho trong transaction.

---

## Việc cần làm tiếp (ưu tiên mentor)

1. **Storefront client (Task 6–7):** controller/view ngoài Area — trang chủ danh sách sách, chi tiết, pagination, search/filter; nút “Thêm vào giỏ” từ catalog.
2. **Bảo mật Admin (Task 12):** seed role `Admin`, gán user; `[Authorize(Roles = "Admin")]` cho controllers trong `Areas/Admin`.
3. **Quản lý đơn (Task 10):** Admin list order + cập nhật `Status`.
4. **Polish (Task 13–15):** dashboard, mở rộng service layer, hoàn thiện UX/validation phía client.
5. **README bổ sung:** screenshot, hướng dẫn tài khoản Admin demo (sau khi có seed).

---

## Quy ước & ghi chú kỹ thuật

- Logic nghiệp vụ nên dần chuyển sang `/Services` (theo `plan.md`); hiện Order/Book/Category vẫn nằm trong controller.
- Không query trong View; filter/search dùng `IQueryable` trên EF (đã áp dụng ở `Admin/Books`).
- Checkout cần user đã login (`[Authorize]` trên `OrderController`).

---

## Tài liệu tham khảo

- [ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/mvc/overview)
- [EF Core](https://learn.microsoft.com/ef/core/)
- [ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity)
