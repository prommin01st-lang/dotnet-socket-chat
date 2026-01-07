using ChatBackend.Data;
using ChatBackend.Entities;
using ChatBackend.Hubs;
using ChatBackend.Services; // (สำหรับ ITokenService, IConversationService)
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // (สำหรับ OpenApiInfo)
using System.Text;
using ChatBackend.Infrastructure; // New
using ChatBackend.Services.FileStorage; // New
using Microsoft.AspNetCore.RateLimiting; // New
using System.Threading.RateLimiting; // New

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// --- 1. ตั้งค่า CORS ---
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextApp", policy =>
    {
        // ถ้าใน config ไม่มีค่า ให้ Default เป็น localhost:3000 เพื่อกัน Error
        var origins = allowedOrigins != null && allowedOrigins.Length > 0 
                      ? allowedOrigins 
                      : new[] { "http://localhost:3000" };

        policy.WithOrigins(origins) 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

// --- 2. เชื่อมต่อ Database ---
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
           .UseSnakeCaseNamingConvention());


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
.AddDefaultTokenProviders(); 


// --- 4. ตั้งค่า Authentication (JWT) ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; 
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JWT:Issuer"],
        ValidAudience = configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!)),
        ClockSkew = TimeSpan.Zero 
    };
    
    // ตั้งค่าให้ SignalR อ่าน Token จาก Query String
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs"))) 
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Skip if response already started
            if (context.Response.HasStarted) return Task.CompletedTask;

            context.HandleResponse(); // Suppress default behavior
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            
            var result = System.Text.Json.JsonSerializer.Serialize(new { message = "You are not authorized." });
            return context.Response.WriteAsync(result);
        }
    };
});

// --- 5. ตั้งค่า Authorization ---
builder.Services.AddAuthorization();

// --- 6. ตั้งค่า SignalR (WebSocket) ---
builder.Services.AddSignalR();

// --- 7. ลงทะเบียน Services (Dependency Injection) ---
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFileStorageService, CloudflareR2StorageService>();

builder.Services.AddControllers();

// Custom Validation Response
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value != null && e.Value.Errors.Count > 0)
            .Select(e => new 
            { 
               Field = e.Key, 
               Error = e.Value!.Errors.First().ErrorMessage 
            })
            .ToList();

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
        {
            message = "Validation failed",
            errors = errors
        });
    };
});
builder.Services.AddEndpointsApiExplorer();

// --- 8. Optimization Services (New) ---
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
        limiterOptions.PermitLimit = rateLimitConfig.GetValue<int>("PermitLimit", 100);
        limiterOptions.Window = TimeSpan.FromMinutes(rateLimitConfig.GetValue<double>("WindowMinutes", 1));
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = rateLimitConfig.GetValue<int>("QueueLimit", 5);
    });
});

// --- 8. ตั้งค่า Swagger ให้รู้จัก JWT ---
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

// --- (ใหม่) เรียกใช้ SeedData (สร้าง Roles) ---
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
// --- จบส่วน SeedData ---


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// (*** FIX ***)
// ปิดการใช้งาน HttpsRedirection ชั่วคราว (สำหรับ Development)
// เพื่อแก้ Bug "Failed to determine the https port for redirect."
// app.UseHttpsRedirection();

// --- ใช้งาน Middleware (ลำดับสำคัญมาก) ---
app.UseExceptionHandler(); // New (Must be first)
app.UseRouting();
app.UseRateLimiter(); // New
app.UseCors("AllowNextApp");
app.UseResponseCompression(); // New

app.UseAuthentication(); 
app.UseAuthorization(); 

app.MapControllers();

// Map SignalR Hub
app.MapHub<ChatHub>("/hubs/chat");

// Map Health Check (New)
app.MapHealthChecks("/health");

app.Run();