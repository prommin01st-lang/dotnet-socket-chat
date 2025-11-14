using ChatBackend.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChatBackend.Services
{
    /// <summary>
    /// Interface สำหรับ Token Service (ตาม Phase 2, Step 2)
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// สร้าง Access Token (JWT) สำหรับผู้ใช้
        /// </summary>
        Task<string> CreateAccessToken(ApplicationUser user);

        /// <summary>
        /// สร้าง Refresh Token (Random String)
        /// </summary>
        string CreateRefreshToken();

        /// <summary>
        /// ดึงข้อมูล ClaimsPrincipal (ข้อมูลผู้ใช้) จาก Access Token ที่หมดอายุแล้ว
        /// (ใช้สำหรับ Endpoint /refresh)
        /// </summary>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}