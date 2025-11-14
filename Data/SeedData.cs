using ChatBackend.Entities;
using Microsoft.AspNetCore.Identity;

namespace ChatBackend.Data
{
    /// <summary>
    /// คลาสสำหรับสร้างข้อมูลเริ่มต้น (เช่น Roles) ตอนที่แอปเริ่มทำงาน
    /// </summary>
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // เราต้องใช้ RoleManager
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                // ตรวจสอบว่า Role นี้มีอยู่แล้วหรือยัง
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    // ถ้ายังไม่มี ให้สร้าง Role ใหม่
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // (Optional) เราสามารถสร้าง User Admin เริ่มต้นได้ที่นี่
            // var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            // var adminUser = await userManager.FindByEmailAsync("admin@admin.com");
            // if (adminUser == null)
            // {
            //     var newAdmin = new ApplicationUser { ... };
            //     await userManager.CreateAsync(newAdmin, "AdminPassword123!");
            //     await userManager.AddToRoleAsync(newAdmin, "Admin");
            // }
        }
    }
}