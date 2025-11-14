using System.ComponentModel.DataAnnotations;

// Namespace คือ .Models
namespace ChatBackend.Models
{
    /// <summary>
    /// DTO ใหม่นี้ ใช้สำหรับรับ Token 2 ตัว
    /// ใน Endpoint /api/accounts/refresh เท่านั้น
    /// </summary>
    public class RefreshTokenRequestDto
    {
        [Required]
        public required string AccessToken { get; set; }

        [Required]
        public required string RefreshToken { get; set; }
    }
}