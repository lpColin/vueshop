using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using app_api.Services;
using app_api.Models;
using app_api.Data;
using Microsoft.EntityFrameworkCore;

namespace app_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly AppDbContext _context;

    public AuthController(AuthService authService, AppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, message, user, token) = await _authService.LoginAsync(request.Username, request.Password);
        if (!success || user == null || string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { success = false, message });
        }

        return Ok(new
        {
            success = true,
            message,
            data = new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Nickname,
                    user.Avatar,
                    user.Phone,
                    user.Role,
                    user.ShopId
                }
            }
        });
    }

    /// <summary>
    /// 微信登录（预留）
    /// </summary>
    [HttpPost("wechat")]
    public async Task<IActionResult> WechatLogin([FromBody] WechatLoginRequest request)
    {
        var (success, message, user, token) = await _authService.WechatLoginAsync(request.Code);
        if (!success || user == null || string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { success = false, message });
        }

        return Ok(new
        {
            success = true,
            message,
            data = new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Nickname,
                    user.Avatar,
                    user.Phone,
                    user.Role,
                    user.ShopId
                }
            }
        });
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // 检查用户名是否已存在
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest(new { success = false, message = "用户名已存在" });
        }

        var newUser = new User
        {
            Username = request.Username,
            Password = AuthService.HashPassword(request.Password),
            Nickname = request.Nickname ?? request.Username,
            Role = "user",
            Status = 1,
            CreateTime = DateTime.Now
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "注册成功" });
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = AuthService.GetCurrentUserId(base.User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { success = false, message = "用户不存在" });
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                user.Id,
                user.Username,
                user.Nickname,
                user.Avatar,
                user.Phone,
                user.Role,
                user.ShopId
            }
        });
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
    {
        var userId = AuthService.GetCurrentUserId(base.User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { success = false, message = "用户不存在" });
        }

        if (!string.IsNullOrEmpty(request.Nickname))
            user.Nickname = request.Nickname;
        if (!string.IsNullOrEmpty(request.Avatar))
            user.Avatar = request.Avatar;
        if (!string.IsNullOrEmpty(request.Phone))
            user.Phone = request.Phone;

        user.UpdateTime = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "更新成功" });
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = AuthService.GetCurrentUserId(base.User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { success = false, message = "用户不存在" });
        }

        // 验证旧密码
        if (user.Password != AuthService.HashPassword(request.OldPassword))
        {
            return BadRequest(new { success = false, message = "原密码错误" });
        }

        user.Password = AuthService.HashPassword(request.NewPassword);
        user.UpdateTime = DateTime.Now;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "密码修改成功" });
    }
}

// DTOs
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class WechatLoginRequest
{
    public string Code { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Nickname { get; set; }
}

public class UpdateUserRequest
{
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public string? Phone { get; set; }
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}