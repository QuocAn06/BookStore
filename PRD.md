# 📘 PRODUCT REQUIREMENTS DOCUMENT

## Project: BookStore Management System

Tech: ASP.NET Core MVC (.NET 6, MVC, EF Core Code First)

---

# 1. 🎯 Mục tiêu sản phẩm

Xây dựng hệ thống web bán sách gồm:

* Client (người dùng mua sách)
* Admin (quản lý hệ thống)

Mục tiêu:

* Thể hiện kỹ năng MVC + EF Core + Authentication
* Demo end-to-end flow: Browse → Cart → Order

---

# 2. 👥 Đối tượng sử dụng

## 2.1 User (Customer)

* Xem và mua sách

## 2.2 Admin

* Quản lý sách, danh mục, đơn hàng

---

# 3. 🧩 Phạm vi hệ thống

## 3.1 Client (Public site)

### 3.1.1 Trang chủ

* Hiển thị danh sách sách
* Phân trang

---

### 3.1.2 Xem chi tiết sách

* Title
* Author
* Price
* Image
* Category

---

### 3.1.3 Tìm kiếm & lọc

* Search theo tên
* Filter:

  * Category
  * Price range

---

### 3.1.4 Giỏ hàng

* Thêm sách
* Cập nhật số lượng
* Xóa sản phẩm
* Lưu trữ: **Session** (JSON) — không có bảng `Cart` trong DB
* **Validate tồn kho** khi thêm / cập nhật số lượng:
  * Không cho vượt `Book.Stock` (cộng dồn nếu sách đã có trong giỏ)
  * Hiển thị thông báo lỗi thân thiện (`TempData`) khi vượt stock hoặc hết hàng
  * Logic nằm trong `CartService`; `CartSessionService` chỉ đọc/ghi session

---

### 3.1.5 Đặt hàng

* Checkout
* Tạo Order + OrderDetails

---

### 3.1.6 Authentication

* Register / Login / Logout

---

## 3.2 Admin (Area)

### 3.2.1 Dashboard

* Tổng số đơn hàng
* Tổng doanh thu
* Tổng số sách

---

### 3.2.2 Quản lý Book

* Create / Read / Update / Delete
* Upload ảnh
* Xóa an toàn: không cho xóa sách đã có trong `OrderDetail`

---

### 3.2.3 Quản lý Category

* CRUD category
* Xóa an toàn: không cho xóa category còn sách

---

### 3.2.4 Quản lý Order

* Xem danh sách đơn hàng
* Update status:

  * Pending
  * Completed

---

# 4. 🗄️ Data Model (Code First)

## Entity chính:

* ApplicationUser (Identity)
* Category
* Book
* Order
* OrderDetail

**Giỏ hàng (client):** dùng Session (`Cart` / `CartItem` model), không persist trong DB.

---

## Quan hệ:

* Category 1 - n Book
* Order 1 - n OrderDetail
* Book 1 - n OrderDetail
* User 1 - n Order

---

# 5. 🔐 Authorization

* User:

  * Mua hàng, xem order

* Admin:

  * Truy cập `/Admin`

```csharp
[Authorize(Roles = "Admin")]
```

---

# 6. ⚙️ Business Rules

## Order

* Khi checkout:

  * Tạo Order
  * Tạo OrderDetails
  * Tính `TotalAmount`

---

## Price

* Lưu price trong OrderDetail
  👉 Không phụ thuộc giá Book hiện tại

---

## Stock

* Không cho mua vượt quá tồn kho — **đã implement**
* **Lớp 1 — Giỏ hàng:** `CartService` validate trước khi add/update quantity
  * Add: `existingQty + quantity ≤ Stock`
  * Update: `quantity ≤ Stock`
  * Hết hàng (`Stock = 0`): không cho thêm
* **Lớp 2 — Checkout:** `OrderService` validate lại + trừ stock trong transaction
  * Bảo vệ khi stock thay đổi sau khi user thêm vào giỏ (race condition)
* Catalog client chỉ hiển thị sách `Stock > 0`

---

# 7. 🧪 Validation

## Book (Admin form)

* Title: required
* Price > 0
* Stock ≥ 0

## Cart (server-side)

* Add: tổng số lượng trong giỏ không vượt `Stock`
* Update: số lượng mới không vượt `Stock`
* Lỗi hiển thị trên trang giỏ hàng

## Order

* Không được empty cart
* Validate stock lại lúc checkout

## Admin delete

* Category: không xóa khi còn sách
* Book: không xóa khi đã có trong đơn hàng

---

# 8. 🚀 Non-functional Requirements

* Kiến trúc:

  * MVC pattern
  * Service layer

* Performance:

  * Pagination
  * Query tối ưu (Include, Select)

---

# 9. 📂 Cấu trúc project

```
/Areas/Admin
/Controllers (Client)
/Models
/Services
  ├── CartSessionService   # Session persistence
  ├── CartService          # Cart business rules (stock)
  ├── BookService, OrderService, ...
/Data
/Views
/Infrastructure           # SessionKeys
```

---

# 10. 🧱 Out of Scope (không làm)

* Payment online
* Microservices
* Clean Architecture full
* Realtime

---

# 11. 📈 Future Enhancements (bonus)

* Client: trang chi tiết sách (`/Home/Detail/{id}`)
* Client: search & filter trên storefront (hiện có ở Admin `/Admin/Books`)
* Customer: lịch sử đơn hàng (`My Orders`)
* `Order.OrderDate` timestamp
* Search không dấu
* Recommendation
* Logging
* Caching
* Unit tests cho Service layer
* Nâng cấp .NET 8 LTS

---

# 11.1 📋 Trạng thái triển khai (tham chiếu)

| Tính năng | Trạng thái |
|-----------|------------|
| Catalog + phân trang (client) | ✅ Done |
| Giỏ hàng Session | ✅ Done |
| Validate stock (add/update cart) | ✅ Done |
| Checkout + trừ stock | ✅ Done |
| Admin CRUD + Dashboard | ✅ Done |
| Xóa an toàn Category/Book | ✅ Done |
| Chi tiết sách (client) | ⏳ Chưa |
| Search/filter storefront | ⏳ Chưa |
| My Orders (customer) | ⏳ Chưa |

---

# 12. ✅ Success Criteria

Project được coi là hoàn thành khi:

* User có thể:

  * Xem sách (catalog, phân trang)
  * Thêm vào cart (validate stock)
  * Cập nhật giỏ hàng (validate stock)
  * Đặt hàng
* Admin có thể:

  * CRUD Book/Category (xóa an toàn khi có ràng buộc FK)
  * Quản lý Order
* Có Authentication + Authorization
* Business logic nằm trong Service layer; controller mỏng

---

# 13. 🎤 Góc nhìn phỏng vấn (quan trọng)

Bạn phải giải thích được:

* Vì sao dùng Area cho Admin?
* Vì sao có OrderDetail?
* Vì sao dùng Identity?
* Flow checkout hoạt động thế nào?
* Vì sao tách `CartSessionService` và `CartService`?
* Vì sao validate stock ở cả giỏ hàng **và** checkout?