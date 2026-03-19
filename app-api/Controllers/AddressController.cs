using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using app_api.Models;
using app_api.Data;
using app_api.Services;
using Microsoft.EntityFrameworkCore;

namespace app_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly AppDbContext _context;

    public AddressController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取用户地址列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var addresses = await _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreateTime)
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = addresses
        });
    }

    /// <summary>
    /// 获取地址详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAddress(int id)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (address == null)
        {
            return NotFound(new { success = false, message = "地址不存在" });
        }

        return Ok(new
        {
            success = true,
            data = address
        });
    }

    /// <summary>
    /// 添加地址
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddAddress([FromBody] AddAddressRequest request)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        // 如果是默认地址，取消其他默认
        if (request.IsDefault)
        {
            var defaultAddresses = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();
            foreach (var addr in defaultAddresses)
            {
                addr.IsDefault = false;
            }
        }

        var address = new Address
        {
            UserId = userId.Value,
            Name = request.Name,
            Phone = request.Phone,
            Province = request.Province,
            City = request.City,
            District = request.District,
            Detail = request.Detail,
            IsDefault = request.IsDefault,
            CreateTime = DateTime.Now
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "地址添加成功",
            data = address
        });
    }

    /// <summary>
    /// 更新地址
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateAddressRequest request)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (address == null)
        {
            return NotFound(new { success = false, message = "地址不存在" });
        }

        // 如果设置为默认地址，取消其他默认
        if (request.IsDefault == true)
        {
            var defaultAddresses = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault && a.Id != id)
                .ToListAsync();
            foreach (var addr in defaultAddresses)
            {
                addr.IsDefault = false;
            }
        }

        address.Name = request.Name ?? address.Name;
        address.Phone = request.Phone ?? address.Phone;
        address.Province = request.Province ?? address.Province;
        address.City = request.City ?? address.City;
        address.District = request.District ?? address.District;
        address.Detail = request.Detail ?? address.Detail;
        
        if (request.IsDefault.HasValue)
            address.IsDefault = request.IsDefault.Value;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "地址更新成功"
        });
    }

    /// <summary>
    /// 删除地址
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (address == null)
        {
            return NotFound(new { success = false, message = "地址不存在" });
        }

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "地址删除成功"
        });
    }

    /// <summary>
    /// 设置默认地址
    /// </summary>
    [HttpPost("{id}/default")]
    public async Task<IActionResult> SetDefaultAddress(int id)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (address == null)
        {
            return NotFound(new { success = false, message = "地址不存在" });
        }

        // 取消其他默认
        var defaultAddresses = await _context.Addresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .ToListAsync();
        foreach (var addr in defaultAddresses)
        {
            addr.IsDefault = false;
        }

        address.IsDefault = true;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "默认地址设置成功"
        });
    }
}

// DTOs
public class AddAddressRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class UpdateAddressRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Detail { get; set; }
    public bool? IsDefault { get; set; }
}