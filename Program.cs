using ChatBackend.Data;
using ChatBackend.Entities;
using ChatBackend.Hubs; // (เราจะสร้างไฟล์ Hubs/ChatHub.cs ใน Phase 3)
using ChatBackend.Services; // <-- (1) เพิ่ม using ที่ขาดไป
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ChatBackend.Models;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Extensions.Hosting; // <-- (1) เพิ่ม using ที่ขาดไป
using Microsoft.Extensions.Logging; // <-- (1) เพิ่ม using ที่ขาดไป

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// --- 1. ตั้งค่า CORS ---
// (จำเป็นสำหรับ Next.js และ SignalR)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextApp", policy =>
    {
        policy.WithOrigins(configuration["JWT:Audience"]!) // ดึง URL มาจาก appsettings
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // <-- สำคัญมากสำหรับ SignalR (และ Auth)
    });
});


// --- 2. เชื่อมต่อ Database ---
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// --- 3. ตั้งค่า Identity (User/Role Management) ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // (Optional) ตั้งค่าความปลอดภัยของ Password
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // เพิ่ม Token Providers สำหรับ (เช่น reset password)


// --- 4. ตั้งค่า Authentication (JWT) ---
// (Phase 1, Step 5)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // (ตั้งเป็น true เมื่อ deploy จริง)
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JWT:Issuer"],
        ValidAudience = configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!)),
        // (สำคัญ) ตั้งค่า ClockSkew เป็น Zero เพื่อให้ Token หมดอายุตรงเวลา (ดีสำหรับการ Refresh)
        ClockSkew = TimeSpan.Zero 
    };
    
    // (สำคัญ) นี่คือการตั้งค่าให้ SignalR อ่าน Token จาก Query String
    // เพราะ WebSocket ไม่สามารถส่ง Auth Header ได้ตามปกติ
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs"))) // จำกัดเฉพาะ Endpoint ของ Hub
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// --- 5. ตั้งค่า Authorization ---
builder.Services.AddAuthorization();

// --- 6. ตั้งค่า SignalR (WebSocket) ---
// (Phase 3, Step 3)
builder.Services.AddSignalR();

//  Addd TokenService ลงใน DI Container
builder.Services.AddScoped<ITokenService, TokenService>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// (Optional) ตั้งค่า Swagger ให้รู้จัก JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatBackend API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // เรียกไฟล์ Data/SeedData.cs
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- 7. ใช้งาน Middleware (ลำดับสำคัญมาก) ---
app.UseRouting(); // ต้องมีก่อน UseCors และ UseAuthorization

app.UseCors("AllowNextApp"); // ใช้งาน CORS

app.UseAuthentication(); // ใช้งาน Authentication (ตรวจสอบ Token)
app.UseAuthorization(); // ใช้งาน Authorization (ตรวจสอบ Role)

app.MapControllers();

// 8. Map SignalR Hub
// (Phase 3, Step 3)
// (เราต้องสร้างไฟล์ ChatHub.cs ในโฟลเดอร์ /Hubs/ ก่อน)
app.MapHub<ChatHub>("/hubs/chat");

app.Run();