using ChatBackend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatBackend.Services
{
    /// <summary>
    /// Implementation ของ Token Service (ตาม Phase 2, Step 2)
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;
        private readonly string _audience;
        public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
            _issuer = config["JWT:Issuer"]!;
            _audience = config["JWT:Audience"]!;
        }

        /// <summary>
        /// 1. สร้าง Access Token
        /// </summary>
        public async Task<string> CreateAccessToken(ApplicationUser user)
        {
            // 1. สร้าง Claims (ข้อมูลที่จะเก็บใน Token)
            var claims = new List<Claim>
            {
                // User ID (มาตรฐาน JWT คือ 'sub')
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                // Email (มาตรฐาน JWT คือ 'email')
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                // JTI (Unique ID ของ Token)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 2. ดึง Roles ของ User มาใส่ใน Claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 3. สร้าง Signing Credentials
            // (FIX) เปลี่ยน Algorithm เป็น HmacSha256Signature
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

            // 4. สร้าง Token Descriptor (พิมพ์เขียวของ Token)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15), // ตั้งหมดอายุ (เช่น 15 นาที)
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = creds
            };

            // 5. สร้างและ Write Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }


        /// <summary>
        /// 2. สร้าง Refresh Token
        /// </summary>
        public string CreateRefreshToken()
        {
            // สร้างเลขสุ่ม 64-byte และแปลงเป็น Base64
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// 3. ดึงข้อมูลจาก Token ที่หมดอายุ
        /// </summary>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // <-- (สำคัญ) ปิดการตรวจสอบวันหมดอายุ
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = _key,
                ClockSkew = TimeSpan.Zero // (เหมือนใน Program.cs)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;

            try
            {
                // พยายามอ่าน Token
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;

                // ตรวจสอบว่า Algorithm ตรงกันกับที่เราใช้สร้างหรือไม่
                // (FIX) เปลี่ยน Algorithm เป็น HmacSha256
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null; // Algorithm ไม่ตรงกัน
                }

                return principal;
            }
            catch (Exception)
            {
                return null; // Token อ่านไม่ได้ (อาจจะผิด)
            }
        }
    }
}