using ChatBackend.Data;
using ChatBackend.Entities;
using ChatBackend.Models;
using ChatBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        // Inject สิ่งที่เราต้องใช้
        public AccountsController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // --- 1. Endpoint: [POST] api/accounts/register ---
        [HttpPost("register")]
        [AllowAnonymous] // อนุญาตให้ทุกคนเรียกได้
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // ตรวจสอบว่า Email ซ้ำหรือไม่
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email address is already in use." });
            }

            // สร้าง User object
            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // สร้าง User ใน DB
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // *** กำหนด Role "User" เป็นค่าเริ่มต้น ***
            // (เราจะมั่นใจว่า Role นี้มีอยู่ จากไฟล์ SeedData.cs)
            await _userManager.AddToRoleAsync(user, "User");

            // --- สร้าง Token (เหมือนตอน Login) ---
            var authResponse = await GenerateAuthResponse(user);
            return Ok(new { authResponse });
        }

        // --- 2. Endpoint: [POST] api/accounts/login ---
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // ค้นหา User
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
            {
                // ไม่ควรบอกว่า "Password ผิด" หรือ "User ไม่มี" เพื่อความปลอดภัย
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // ตรวจสอบ Password
            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // --- สร้าง Token ---
            var authResponse = await GenerateAuthResponse(user);
            return Ok(new { authResponse });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto tokens)
        {
            string accessToken = tokens.AccessToken;
            string refreshToken = tokens.RefreshToken;

            // 1. อ่านข้อมูล User (Principal) จาก Access Token "ที่หมดอายุ"
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);

            // --- (*** FIX ***) ---
            // Bug อยู่ตรงนี้: เราต้องตรวจสอบ 'sub' (User ID)
            // ไม่ใช่ 'principal.Identity.Name' (ซึ่งเราไม่ได้ตั้งค่าไว้ใน Token)
            var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // ถ้า Token ไม่มี 'sub' (User ID) ก็ถือว่า Token ผิด
                return BadRequest(new { message = "Invalid token." });
            }
            // --- (จบการแก้ไข) ---

            // 2. ดึง User จาก ID
            var user = await _userManager.FindByIdAsync(userId);

            // 3. ตรวจสอบว่า User, Refresh Token ที่ส่งมา, และ Token ใน DB ตรงกันหรือไม่
            // และ Token ใน DB ยังไม่หมดอายุ
            if (user == null ||
                user.RefreshToken != refreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid refresh token or token expired." });
            }

            // 4. (สำเร็จ) สร้าง Token ชุดใหม่ (เหมือนตอน Login)
            var authResponse = await GenerateAuthResponse(user);
            return Ok(new { authResponse });
        }
        // --- 4. Endpoint: [POST] api/accounts/revoke ---
        // (*** นี่คือส่วนที่เพิ่มเข้ามา ***)
        [HttpPost("revoke")]
        [Authorize] // ต้อง Login อยู่ (มี Access Token ที่ยังไม่หมดอายุ) ถึงจะ Revoke ได้
        public async Task<IActionResult> RevokeToken()
        {
            // ดึง User ID จาก Token ปัจจุบัน
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
            {
                return BadRequest(new { message = "Invalid user." });
            }

            // 5. "ยกเลิก" Token โดยการลบออกจาก DB
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Token revoked successfully." });
        }


        // --- 3. Endpoint: [GET] api/accounts/me ---
        [HttpGet("me")]
        [Authorize] // *** ต้อง Login เท่านั้น (ส่ง JWT มา) ถึงจะเรียกได้ ***
        public async Task<IActionResult> GetCurrentUser()
        {
            // ดึง User ID จาก Token (ClaimTypes.NameIdentifier คือ 'sub' ใน JWT)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(); // ไม่น่าเกิดขึ้นได้เพราะ [Authorize]
            }

            var user = await _userManager.Users
                .Select(u => new UserDto // ใช้ DTO เพื่อความปลอดภัย
                {
                    Id = u.Id,
                    Email = u.Email!,
                    FirstName = u.FirstName!,
                    LastName = u.LastName!,
                    ProfilePictureUrl = u.ProfilePictureUrl,
                    Roles = new List<string>() // เดี๋ยวเราจะไปดึง Role มาใส่
                })
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // ดึง Role มาใส่ใน DTOฝ
            var appUser = await _userManager.FindByIdAsync(user.Id);
            user.Roles = await _userManager.GetRolesAsync(appUser!);

            return Ok(user);
        }


        // --- 4. (Helper Method) สร้าง Token Response ---
        private async Task<AuthResponseDto> GenerateAuthResponse(ApplicationUser user)
        {
            // 1. สร้าง Access Token (JWT)
            var accessToken = await _tokenService.CreateAccessToken(user);

            // 2. สร้าง Refresh Token
            var refreshToken = _tokenService.CreateRefreshToken();

            // 3. บันทึก Refresh Token ลง Database
            user.RefreshToken = refreshToken;
            var expireDays = _configuration.GetValue<int>("JWT:ExpireDays", 7);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(expireDays);
            await _userManager.UpdateAsync(user);

            // 4. ดึง Roles
            var roles = await _userManager.GetRolesAsync(user);

            // 5. สร้าง UserDto (สำหรับแสดงข้อมูลที่ปลอดภัย)
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Roles = roles
            };

            // 6. ส่งกลับ AuthResponseDto
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = userDto
            };
        }
    }
}