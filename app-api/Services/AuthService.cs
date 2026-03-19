using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using app_api.Models;
using app_api.Data;
using Microsoft.EntityFrameworkCore;

namespace app_api.Services;

public class JwtSettings
{
    public string SecretKey { get; set; } = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
    public string Issuer { get; set; } = "app-api";
    public string Audience { get; set; } = "app-client";
    public int ExpirationMinutes { get; set; } = 60 * 24 * 7; // 7 天
}

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext context, JwtSettings jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings;
    }

    /// <summary>
    /// 用户登录（用户名密码）
    /// </summary>
    public async Task<(bool success, string message, User? user, string? token)> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            return (false, "用户不存在", null, null);
        }

        if (user.Status != 1)
        {
            return (false, "用户已被禁用", null, null);
        }

        // 验证密码（实际项目中应该使用哈希比对）
        if (user.Password != HashPassword(password))
        {
            return (false, "密码错误", null, null);
        }

        var token = GenerateJwtToken(user);
        return (true, "登录成功", user, token);
    }

    /// <summary>
    /// 微信登录（预留）
    /// </summary>
    public async Task<(bool success, string message, User? user, string? token)> WechatLoginAsync(string code)
    {
        // TODO: 调用微信接口获取 OpenId
        // 这里仅作示例
        var openId = "mock_openid_" + code;
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.OpenId == openId);
        if (user == null)
        {
            // 自动注册新用户
            user = new User
            {
                Username = "wx_" + openId.Substring(0, 8),
                Password = Guid.NewGuid().ToString("N"),
                OpenId = openId,
                Nickname = "微信用户",
                Role = "user",
                Status = 1,
                CreateTime = DateTime.Now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        if (user.Status != 1)
        {
            return (false, "用户已被禁用", null, null);
        }

        var token = GenerateJwtToken(user);
        return (true, "登录成功", user, token);
    }

    /// <summary>
    /// 生成 JWT Token
    /// </summary>
    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("role", user.Role),
            new Claim("shopId", user.ShopId?.ToString() ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 密码哈希（实际项目应使用更安全的算法）
    /// </summary>
    public static string HashPassword(string password)
    {
        // 简单示例，实际应使用 BCrypt 或 PBKDF2
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 获取当前用户 ID（从 Claims）
    /// </summary>
    public static int? GetCurrentUserId(System.Security.Claims.ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// 获取当前用户角色
    /// </summary>
    public static string? GetUserRole(System.Security.Claims.ClaimsPrincipal user)
    {
        return user.FindFirst("role")?.Value;
    }
}