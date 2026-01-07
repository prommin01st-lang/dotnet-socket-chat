using System.ComponentModel.DataAnnotations;

namespace ChatBackend.Models
{
    public class RoleAssignmentDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}
