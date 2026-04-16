# 🗺️ DEVELOPMENT PLAN – BookStore MVC

## Tổng timeline gợi ý

* Phase 1 → 3: Core (bắt buộc)
* Phase 4 → 5: Hoàn thiện + điểm cộng

---

# 🚀 PHASE 1 – Setup & Foundation

## Task 1: Init project

### Yêu cầu

* Tạo project MVC (.NET 6)
* Setup ASP.NET Core MVC + EF Core

### Làm gì

* Create project
* Add EF Core + Identity
* Config connection string

### DoD

* Run được project
* Kết nối DB thành công

---

## Task 2: Setup Identity

### Yêu cầu

* Dùng Identity (không tự viết auth)

### Làm gì

* Tạo `ApplicationUser`
* Scaffold Identity (Login/Register)

### DoD

* Login / Register chạy OK
* Có bảng User trong DB

---

## Task 3: Setup DbContext + Migration

### Làm gì

* Tạo DbContext
* Add DbSet:

  * Book
  * Category
  * Order
  * OrderDetail

### DoD

* Chạy:

```bash
Add-Migration Init
Update-Database
```

* DB tạo đủ bảng

---

# 📦 PHASE 2 – Core Business (Quan trọng nhất)

## Task 4: Category CRUD (Admin)

### Làm gì

* Area: Admin
* Controller: CategoryController

### DoD

* Create / Edit / Delete / List
* Validation hoạt động

---

## Task 5: Book CRUD (Admin)

### Làm gì

* CRUD Book
* Upload image (lưu wwwroot)

### DoD

* Book hiển thị đúng category
* Upload ảnh thành công

---

## Task 6: Client – List & Detail Book

### Làm gì

* Home page (list book)
* Book detail

### DoD

* Có pagination
* Hiển thị đúng data từ DB

---

## Task 7: Search & Filter

### Làm gì

* Search theo Title
* Filter theo Category

### DoD

* Query chạy đúng
* Không load toàn bộ DB rồi filter

---

# 🛒 PHASE 3 – Cart & Order (flow quan trọng)

## Task 8: Cart (Session-based)

### Làm gì

* Add to cart
* Remove
* Update quantity

### DoD

* Lưu bằng Session
* Không lỗi khi refresh

---

## Task 9: Checkout → Order

### Làm gì

* Tạo Order + OrderDetail

### Logic bắt buộc

* Copy price từ Book → OrderDetail
* Tính TotalAmount

### DoD

* Sau checkout:

  * DB có Order
  * DB có OrderDetail

---

## Task 10: Order Management (Admin)

### Làm gì

* List order
* Update status

### DoD

* Admin xem được order
* Update trạng thái OK

---

# 🔐 PHASE 4 – Authorization & Structure

## Task 11: Area Admin

### Làm gì

* Tạo `/Areas/Admin`

### DoD

* URL dạng:

```
/Admin/Book
```

---

## Task 12: Role-based Authorization

### Làm gì

* Seed role Admin
* Gán user Admin

### DoD

* Route Admin bị chặn nếu không có role

---

# ⭐ PHASE 5 – Polish (điểm cộng)

## Task 13: Dashboard

### Làm gì

* Tổng:

  * Orders
  * Revenue

### DoD

* Query đúng
* Hiển thị số liệu

---

## Task 14: Validation + UX

### Làm gì

* Data annotation
* Error message

### DoD

* Form không submit nếu invalid

---

## Task 15: Refactor Service Layer

### Làm gì

* Tách logic ra:

```
/Services
```

### DoD

* Controller không chứa business logic

---

# 📌 OPTIONAL (nếu còn thời gian)

## Task 16: Logging

* Log khi tạo order

## Task 17: Seed data

* Seed Category + Book

## Task 18: Basic Unit Test

* Test service

---

# ⚠️ Quy tắc bắt buộc (mentor note)

## 1. Không làm kiểu “cho chạy”

Sai:

* Query trong View
* Logic trong Controller

Đúng:

* Service xử lý logic
* Controller gọi service

---

## 2. Không over-engineer

* ❌ Clean Architecture full
* ❌ Microservices
* ❌ Repository pattern phức tạp

---

## 3. Commit chuẩn GitHub

Ví dụ:

```
feat: add book CRUD
fix: cart calculation bug
refactor: move logic to service layer
```

---

# 🎯 Milestone hoàn chỉnh

Bạn DONE khi:

* CRUD Book + Category OK
* Cart + Order flow chạy
* Admin manage được order
* Auth + Role hoạt động

---

# 💬 Gợi ý cực quan trọng

Sau khi xong:

* Viết README:

  * Tech stack
  * Feature
  * Screenshot
* Deploy (nếu được)
