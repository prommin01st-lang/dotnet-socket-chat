# .NET 9 API Base Template (JWT + SignalR + BFF)

นี่คือโปรเจกต์ **Base API (Backend)** ที่สร้างด้วย **.NET 9** ซึ่งเป็น Template เริ่มต้นที่ครบพร้อมสำหรับระบบแชท, Auth, JWT, SignalR และ BFF

---

## 🧱 เทคโนโลยีหลัก (Tech Stack)

- **Backend:** .NET 9 / C#
- **Authentication:** ASP.NET Core Identity
- **Authorization:** JWT (Access Tokens + Refresh Tokens)
- **Real-time:** SignalR (WebSockets)
- **Database:** Entity Framework Core 9 (SQL Server)
- **Architecture:** Service Layer  
  *(Controllers → Services → DbContext)*

---

## ✨ ฟีเจอร์ที่มีให้ (Features)

### 🔐 ระบบ Auth (BFF Ready)

- `POST /register`
- `POST /login`
- `POST /refresh` – ใช้กับ HttpOnly Cookies
- `POST /revoke` – สำหรับ Logout
- `GET /me` – ดึงข้อมูลผู้ใช้ที่ล็อกอิน

---

### 👤 Role Management

- มี **Seed Data** สร้าง Role:
  - `Admin`
  - `User`

---

### ⚡ Real-time (SignalR)

- `Hubs/ChatHub.cs` ใช้ `[Authorize]`
- รองรับ Logic “Push” ไปยัง User ID โดยตรง (ผ่าน MessagesController)

---

### 💬 Chat System (API)

- `Controllers/ConversationsController.cs`
  - ดึงรายชื่อห้องแชท
  - ดึงข้อความในห้องแชท
  - สร้างห้อง 1-1 อัตโนมัติ
- `Controllers/MessagesController.cs`
  - ส่งข้อความ
  - Trigger SignalR เพื่อ Push ให้ Client ที่เกี่ยวข้อง

---

## ⚙ Configuration

- มี `.gitignore` เพื่อป้องกันไม่ให้ `appsettings.Development.json` หลุดขึ้น Git
- Template สำหรับไฟล์ตั้งค่า:
  - `appsettings.Template.json`

---

## 🚀 วิธีเริ่มต้นใช้งาน (Get Started)

### 1️⃣ Clone Repository

```bash
git clone [YOUR_API_REPO_URL]
cd [your-repo-name]
