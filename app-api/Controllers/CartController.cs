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
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取购物车列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var items = await _context.CartItems
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreateTime)
            .ToListAsync();

        var total = items.Sum(i => i.Quantity * i.ProductPrice);
        var selectedTotal = items.Where(i => i.Selected).Sum(i => i.Quantity * i.ProductPrice);

        return Ok(new
        {
            success = true,
            data = new
            {
                list = items,
                total,
                selectedTotal,
                count = items.Count,
                selectedCount = items.Count(i => i.Selected)
            }
        });
    }

    /// <summary>
    /// 添加商品到购物车
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        // 检查商品是否存在
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null)
        {
            return NotFound(new { success = false, message = "商品不存在" });
        }

        // 检查是否已在购物车中
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.ProductPrice = product.Price;
            existingItem.ProductName = product.Name;
            existingItem.ProductImage = product.Image;
        }
        else
        {
            var cartItem = new CartItem
            {
                UserId = userId.Value,
                ProductId = request.ProductId,
                ProductName = product.Name,
                ProductPrice = product.Price,
                ProductImage = product.Image,
                Quantity = request.Quantity,
                Selected = true,
                CreateTime = DateTime.Now
            };
            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "添加成功"
        });
    }

    /// <summary>
    /// 更新购物车商品数量
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartRequest request)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (cartItem == null)
        {
            return NotFound(new { success = false, message = "购物车商品不存在" });
        }

        if (request.Quantity.HasValue)
            cartItem.Quantity = request.Quantity.Value;
        if (request.Selected.HasValue)
            cartItem.Selected = request.Selected.Value;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "更新成功"
        });
    }

    /// <summary>
    /// 删除购物车商品
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCartItem(int id)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (cartItem == null)
        {
            return NotFound(new { success = false, message = "购物车商品不存在" });
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "删除成功"
        });
    }

    /// <summary>
    /// 清空购物车
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "清空成功"
        });
    }

    /// <summary>
    /// 批量选择/取消选择
    /// </summary>
    [HttpPost("select")]
    public async Task<IActionResult> SelectItems([FromBody] SelectItemsRequest request)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var items = await _context.CartItems
            .Where(c => c.UserId == userId && request.Ids.Contains(c.Id))
            .ToListAsync();

        foreach (var item in items)
        {
            item.Selected = request.Selected;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "操作成功"
        });
    }
}

// DTOs
public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartRequest
{
    public int? Quantity { get; set; }
    public bool? Selected { get; set; }
}

public class SelectItemsRequest
{
    public List<int> Ids { get; set; } = new();
    public bool Selected { get; set; } = true;
}