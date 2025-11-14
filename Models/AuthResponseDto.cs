// Namespace ได้ถูกเปลี่ยนเป็น .Models
namespace ChatBackend.Models
{
    // DTO สำหรับส่ง Token และข้อมูล User กลับไปหลัง Login/Register สำเร็จ
    public class AuthResponseDto
    {
        /// <summary>
        /// Access Token (JWT) ที่ใช้ในการเข้าถึง API (อายสั้น)
        /// </summary>
        public required string AccessToken { get; set; }

        /// <summary>
        /// Refresh Token ที่ใช้ในการขอ Access Token ใหม่ (อายุนาน)
        /// </summary>
        public required string RefreshToken { get; set; }
        
        public required UserDto User { get; set; }
    }
}