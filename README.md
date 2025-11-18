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

## 🗺️ เส้นทาง API (Available Endpoints)

### Authentication (`/api/accounts`)

| Method | Endpoint | Protection | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/register` | Public | ลงทะเบียน User ใหม่ |
| `POST` | `/login` | Public | เข้าสู่ระบบ (รับ Tokens) |
| `POST` | `/refresh` | Public | ขอ Token ใหม่ (ใช้ Refresh Token) |
| `POST` | `/revoke` | `[Authorize]` | ยกเลิก Refresh Token (สำหรับ Logout) |
| `GET` | `/me` | `[Authorize]` | ดึงข้อมูล User (ที่ Login อยู่) |

### Chat System (`/api/`)

| Method | Endpoint | Protection | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/conversations` | `[Authorize]` | ดึง "รายชื่อห้องแชท" (Sidebar) |
| `GET` | `/conversations/{id}/messages` | `[Authorize]` | ดึง "ข้อความเก่า" ในห้องแชท |
| `POST` | `/conversations/onetoone/{userId}` | `[Authorize]` | "เริ่ม" แชท 1-1 (หรือค้นหาห้องเดิม) |
| `POST` | `/messages` | `[Authorize]` | "ส่ง" ข้อความใหม่ (และ Push ผ่าน SignalR) |

### WebSocket (`/hubs`)

| Protocol | Endpoint | Protection | Description |
| :--- | :--- | :--- | :--- |
| `wss://` | `/hubs/chat` | `[Authorize]` (JWT) | เชื่อมต่อ Real-time (ต้องส่ง `access_token` ใน Query String) |

---

## 🚀 วิธีเริ่มต้นใช้งาน (Get Started)

### 1.  Clone Repository

```bash
git clone [YOUR_API_REPO_URL]
cd [your-repo-name]
```


### 2. สร้างไฟล์ตั้งค่า (สำคัญมาก)
- คัดลอกไฟล์ appsettings.Template.json

- สร้างไฟล์ใหม่ชื่อ appsettings.Development.json

- วางเนื้อหาจาก Template ลงไป

### 3. แก้ไข appsettings.Development.json
- เปลี่ยน ConnectionStrings:DefaultConnection ให้เป็น SQL Server ของคุณ
    - (แนะนำ) เปลี่ยน JWT:Key เป็นค่า Secret ใหม่ (สุ่มใหม่)
- แก้ไข JWT:Issuer (API URL) และ JWT:Audience (Frontend URL)

### 4. สร้าง Database
(ตรวจสอบว่า Connection String ถูกต้องแล้ว)

```bash
dotnet ef database update
```

### 5. รันโปรเจกต์

```bash
dotnet run
```

(API จะรันที่ (เช่น) http://localhost:5212)

```bash
dotnet run
```
