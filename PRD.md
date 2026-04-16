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

---

### 3.2.3 Quản lý Category

* CRUD category

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
* (Optional) Cart, CartItem

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

## Stock (nếu implement)

* Không cho mua vượt quá tồn kho

---

# 7. 🧪 Validation

* Book:

  * Title: required
  * Price > 0
* Order:

  * Không được empty cart

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

```id="h7k2u4"
/Areas/Admin
/Controllers (Client)
/Models
/Services
/Data
/Views
```

---

# 10. 🧱 Out of Scope (không làm)

* Payment online
* Microservices
* Clean Architecture full
* Realtime

---

# 11. 📈 Future Enhancements (bonus)

* Search không dấu
* Recommendation
* Logging
* Caching

---

# 12. ✅ Success Criteria

Project được coi là hoàn thành khi:

* User có thể:

  * Xem sách
  * Thêm vào cart
  * Đặt hàng
* Admin có thể:

  * CRUD Book/Category
  * Quản lý Order
* Có Authentication + Authorization

---

# 13. 🎤 Góc nhìn phỏng vấn (quan trọng)

Bạn phải giải thích được:

* Vì sao dùng Area cho Admin?
* Vì sao có OrderDetail?
* Vì sao dùng Identity?
* Flow checkout hoạt động thế nào?