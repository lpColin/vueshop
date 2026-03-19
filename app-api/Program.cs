using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using app_api.Data;
using app_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 配置 JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<FileUploadService>();

// 配置数据库
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=app.db"));

// 配置 JWT 认证
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        // 使用迁移而不是 EnsureCreated
        var pendingMigrations = context.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            Console.WriteLine($"应用 {pendingMigrations.Count()} 个待处理的迁移...");
            context.Database.Migrate();
            Console.WriteLine("数据库迁移完成！");
        }
    }
    catch (Exception ex)
    {
        // 如果迁移失败（如表已存在），使用 EnsureCreated
        Console.WriteLine($"迁移失败：{ex.Message}");
        Console.WriteLine("使用 EnsureCreated 确保数据库存在...");
        context.Database.EnsureCreated();
    }
    
    // 检查是否需要初始化测试数据
    if (!context.Users.Any())
    {
        SeedData.Initialize(context);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 启用静态文件服务（wwwroot 目录）
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 健康检查
app.MapGet("/", () => Results.Ok(new 
{ 
    message = "VueShop API 运行中",
    version = "1.0.0",
    timestamp = DateTime.Now 
}));

app.Run();