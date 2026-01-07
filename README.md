# .NET 9 API Base Template (PostgreSQL + Optimizations)

นี่คือโปรเจกต์ **Base API (Backend)** ที่สร้างด้วย **.NET 9** ซึ่งเป็น Template เริ่มต้นที่ครบพร้อมสำหรับระบบแชท, Auth, JWT, SignalR และ BFF โดยได้รับการปรับปรุงประสิทธิภาพและเปลี่ยนมาใช้ PostgreSQL

---

## 🧱 เทคโนโลยีหลัก (Tech Stack)

- **Backend:** .NET 9 / C#
- **Database:** PostgreSQL (ใช้ `Npgsql`)
- **ORM:** Entity Framework Core 9 (Snake Case Naming)
- **Authentication:** ASP.NET Core Identity + JWT (Access/Refresh Tokens)
- **Real-time:** SignalR (WebSockets)
- **Architecture:** Service Layer *(Controllers → Services → DbContext)*

---

## ✨ ฟีเจอร์ที่มีให้ (Features)

### 🚀 Optimization & Production Ready
- **Health Checks:** `/health` สำหรับตรวจสอบสถานะ Server
- **Rate Limiting:** จำกัดการเรียก API (ป้องกัน Spam/DDoS) Config ได้ใน `appsettings.json`
- **Response Compression:** บีบอัดข้อมูล (Gzip) เพื่อลดขนาด Response
- **Global Exception Handling:** จัดการ Error ทั้งหมดในที่เดียว (Return JSON มาตรฐาน)

### 🔐 Authentication System
- **Registers/Login:** พร้อม JWT Token
- **Refresh Token:** รองรับการต่ออายุ Token แบบปลอดภัย
- **Revoke Token:** Logout และยกเลิก Session
- **Configurable Expiry:** ตั้งเวลาหมดอายุ Token ได้ใน `appsettings.json`

### 💬 Chat System (Real-time)
- **One-to-One Chat:** สร้างห้องแชทส่วนตัวอัตโนมัติ
- **History:** เก็บและดึงประวัติข้อความ
- **SignalR Push:** ส่งข้อความหา User แบบ Real-time ทันที

---

## ⚙ Configuration (`appsettings.json`)

โปรเจกต์นี้รองรับการตั้งค่าผ่านไฟล์ JSON ได้โดยไม่ต้องแก้โค้ด:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ChatDb;Username=postgres;Password=YOUR_PASSWORD"
  },
  "JWT": {
    "Key": "YOUR_SUPER_SECRET_KEY_MUST_BE_LONG",
    "Issuer": "http://localhost:5212",
    "Audience": "http://localhost:3000",
    "ExpireDays": 7,              // อายุ Refresh Token (วัน)
    "AccessTokenExpireMinutes": 15 // อายุ Access Token (นาที)
  },
  "RateLimiting": {
    "PermitLimit": 100,    // จำนวน Request สูงสุด
    "WindowMinutes": 1,    // ต่อเวลา (นาที)
    "QueueLimit": 5        // คิวที่รอได้เมื่อเกิน Limit
  }
}
```

---

## 🗺️ เส้นทาง API (Endpoints)

### 🛠️ Infrastructure
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `GET` | `/health` | ตรวจสอบสถานะ Server (Healthy) |

### 👤 Authentication (`/api/accounts`)
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/register` | สมัครสมาชิก |
| `POST` | `/login` | เข้าสู่ระบบ |
| `POST` | `/refresh` | ต่ออายุ Token |
| `POST` | `/revoke` | ล้าง Token (Logout) |
| `GET` | `/me` | ดูข้อมูลส่วนตัว |

### 💬 Chat & Users
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/conversations/onetoone/{userId}` | เริ่มแชทกับ User อื่น |
| `POST` | `/api/messages` | ส่งข้อความ |
| `GET` | `/api/conversations` | ดูรายการห้องแชท |
| `GET` | `/api/users` | ค้นหา User ทั้งหมด |

---

## 📦 API Testing (Bruno)

โปรเจกต์นี้มาพร้อมกับ **Bruno Collection** 📂
ให้เปิดโฟลเดอร์ `bruno/` ในโปรแกรม [Bruno](https://www.usebruno.com/) เพื่อทดสอบ API ได้ทันที
- มีการตั้งค่า Environment (`Development`)
- จัดการ Token ให้อัตโนมัติ (Login แล้วยิง Request อื่นต่อได้เลย)

---

## 🚀 วิธีเริ่มต้นใช้งาน (Get Started)

### 1. Requirements
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/)

### 2. Setup Database
แก้ไข `appsettings.json` ให้ตรงกับ PostgreSQL ของคุณ แล้วรันคำสั่ง:

```bash
dotnet ef database update
```

### 3. Run Project

```bash
dotnet run
```

---

*Updated: 2026-01-07*
