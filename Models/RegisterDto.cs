using System.ComponentModel.DataAnnotations;

// Namespace ได้ถูกเปลี่ยนเป็น .Models
namespace ChatBackend.Models
{
    // DTO สำหรับรับข้อมูลการลงทะเบียน
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public required string Password { get; set; }

        [Required]
        [StringLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public required string LastName { get; set; }
    }
}