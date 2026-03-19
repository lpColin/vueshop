using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using app_api.Models;
using app_api.Data;
using app_api.Services;
using Microsoft.EntityFrameworkCore;

namespace app_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取商品列表（分页）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? shopId = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? keyword = null)
    {
        var query = _context.Products.Where(p => p.Status == 1);

        if (shopId.HasValue)
            query = query.Where(p => p.ShopId == shopId.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(p => p.Name.Contains(keyword));

        var total = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                total,
                page,
                pageSize,
                list = products.Select(p => new
                {
                    p.Id,
                    p.ShopId,
                    p.CategoryId,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.OriginalPrice,
                    p.Stock,
                    p.Sales,
                    p.Image,
                    p.Images
                })
            }
        });
    }

    /// <summary>
    /// 获取商品详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { success = false, message = "商品不存在" });
        }

        return Ok(new
        {
            success = true,
            data = product
        });
    }

    /// <summary>
    /// 获取分类商品列表
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        var products = await _context.Products
            .Where(p => p.CategoryId == categoryId && p.Status == 1)
            .OrderByDescending(p => p.Sales)
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = products
        });
    }

    /// <summary>
    /// 搜索商品
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string keyword)
    {
        var products = await _context.Products
            .Where(p => p.Status == 1 && p.Name.Contains(keyword))
            .OrderByDescending(p => p.Sales)
            .Take(20)
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = products
        });
    }

    /// <summary>
    /// 获取商家商品（商家权限）
    /// </summary>
    [Authorize]
    [HttpGet("shop/my")]
    public async Task<IActionResult> GetMyShopProducts()
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.Role != "merchant" || user.ShopId == null)
        {
            return Forbid();
        }

        var products = await _context.Products
            .Where(p => p.ShopId == user.ShopId)
            .OrderByDescending(p => p.CreateTime)
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = products
        });
    }

    /// <summary>
    /// 创建商品（商家权限）
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.Role != "merchant" || user.ShopId == null)
        {
            return Forbid();
        }

        var product = new Product
        {
            ShopId = user.ShopId.Value,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Price = request.Price,
            OriginalPrice = request.OriginalPrice > 0 ? request.OriginalPrice : request.Price,
            Stock = request.Stock,
            Sales = 0,
            Image = request.Image ?? (request.Images != null && request.Images.Count > 0 ? request.Images[0] : ""),
            Images = request.Images != null && request.Images.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(request.Images) : null,
            Status = request.Status,
            CreateTime = DateTime.Now
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "商品创建成功",
            data = product
        });
    }

    /// <summary>
    /// 更新商品（商家权限）
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.Role != "merchant" || user.ShopId == null)
        {
            return Forbid();
        }

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.ShopId == user.ShopId);
        if (product == null)
        {
            return NotFound(new { success = false, message = "商品不存在" });
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            product.Name = request.Name;
        if (request.Description != null)
            product.Description = request.Description;
        if (request.Price > 0)
        {
            product.Price = request.Price;
            if (request.OriginalPrice > 0)
                product.OriginalPrice = request.OriginalPrice;
        }
        if (request.Stock > 0)
            product.Stock = request.Stock;
        if (request.Image != null)
            product.Image = request.Image;
        if (request.Images != null)
            product.Images = request.Images.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(request.Images) : null;
        if (request.Status >= 0)
            product.Status = request.Status;
        product.UpdateTime = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "商品更新成功"
        });
    }

    /// <summary>
    /// 删除商品（商家权限）
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var userId = AuthService.GetCurrentUserId(User);
        if (userId == null)
        {
            return Unauthorized(new { success = false, message = "未登录" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.Role != "merchant" || user.ShopId == null)
        {
            return Forbid();
        }

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.ShopId == user.ShopId);
        if (product == null)
        {
            return NotFound(new { success = false, message = "商品不存在" });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "商品删除成功"
        });
    }
}

// DTOs
public class CreateProductRequest
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int Stock { get; set; }
    public string? Image { get; set; }
    public List<string>? Images { get; set; }
    public int Status { get; set; } = 1;
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int Stock { get; set; }
    public string? Image { get; set; }
    public List<string>? Images { get; set; }
    public int Status { get; set; }
}