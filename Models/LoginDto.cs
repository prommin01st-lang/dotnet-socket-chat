using System.ComponentModel.DataAnnotations;

// Namespace ได้ถูกเปลี่ยนเป็น .Models
namespace ChatBackend.Models
{
    // DTO สำหรับรับข้อมูลการ Login
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}