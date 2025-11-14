// Namespace ได้ถูกเปลี่ยนเป็น .Models
namespace ChatBackend.Models
{
    // DTO สำหรับส่งข้อมูล User ที่ "ปลอดภัย" กลับไปให้ Client
    // (เราจะไม่ส่ง Password Hash หรือข้อมูล sensitive อื่นๆ)
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public required IList<string> Roles { get; set; }
    }
}